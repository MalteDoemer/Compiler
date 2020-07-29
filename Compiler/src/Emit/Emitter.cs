using System;
using System.Linq;
using System.Collections.Generic;
using Compiler.Binding;
using Compiler.Diagnostics;
using Compiler.Symbols;
using Compiler.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.IO;
using System.Collections.Immutable;
using Compiler.Lowering;

namespace Compiler.Emit
{
    internal sealed class Emiter : IDiagnostable
    {
        private readonly BoundProgram program;
        private readonly DiagnosticBag diagnostics;
        private readonly AssemblyDefinition mainAssembly;
        private readonly TypeDefinition mainClass;

        private readonly ImmutableArray<AssemblyDefinition> references;
        private readonly ImmutableDictionary<TypeSymbol, TypeReference> resolvedTypes;
        private readonly Dictionary<FunctionSymbol, MethodDefinition?> functions;

        private readonly Dictionary<BoundLabel, int> labels;
        private readonly List<(int, BoundLabel)> fixups;

        private readonly TypeReference? randomTypeReference;
        private readonly MethodReference? randomCtorReference;
        private readonly MethodReference? randomNextReference;
        private readonly MethodReference? randomNextDoubleReference;

        private readonly MethodReference? consoleWriteReference;
        private readonly MethodReference? consoleWriteLineReference;
        private readonly MethodReference? cosnoleReadLineReference;
        private readonly MethodReference? cosnoleClearReference;
        private readonly MethodReference? stringConcatReference;
        private readonly MethodReference? mathPowReference;
        private readonly MethodReference? convertToStringReference;
        private readonly MethodReference? stringEqualsReference;
        private readonly MethodReference? objectEqualsReference;
        private readonly MethodReference? environmentExitReference;
        private readonly MethodReference? stringGetLengthReference;

        private readonly Dictionary<VariableSymbol, FieldDefinition> globalVariables;
        private readonly Dictionary<LocalVariableSymbol, VariableDefinition> locals;

        private FieldDefinition? randomDefiniton;
        private bool needsRandom;

        public IEnumerable<Diagnostic> GetDiagnostics() => diagnostics;

        public Emiter(BoundProgram program)
        {
            this.program = program;
            this.references = program.ReferencAssemblies;
            this.mainAssembly = program.MainAssembly;
            this.resolvedTypes = program.ResolvedTypes;
            this.diagnostics = new DiagnosticBag();
            this.functions = new Dictionary<FunctionSymbol, MethodDefinition?>();
            this.globalVariables = new Dictionary<VariableSymbol, FieldDefinition>();
            this.locals = new Dictionary<LocalVariableSymbol, VariableDefinition>();
            this.labels = new Dictionary<BoundLabel, int>();
            this.fixups = new List<(int, BoundLabel)>();
            mainClass = new TypeDefinition("", "Program", TypeAttributes.Abstract | TypeAttributes.Sealed | TypeAttributes.Public, ResolveType("System.Object"));

            randomTypeReference = ResolveType("System.Random");
            consoleWriteReference = ResolveMethod("System.Console", "Write", "System.Void", "System.Object");
            consoleWriteLineReference = ResolveMethod("System.Console", "WriteLine", "System.Void", "System.Object");
            cosnoleReadLineReference = ResolveMethod("System.Console", "ReadLine", "System.String");
            cosnoleClearReference = ResolveMethod("System.Console", "Clear", "System.Void");
            stringConcatReference = ResolveMethod("System.String", "Concat", "System.String", "System.String", "System.String");
            mathPowReference = ResolveMethod("System.Math", "Pow", "System.Double", "System.Double", "System.Double");
            convertToStringReference = ResolveMethod("System.Convert", "ToString", "System.String", "System.Object");
            stringEqualsReference = ResolveMethod("System.String", "Equals", "System.Boolean", "System.String", "System.String");
            objectEqualsReference = ResolveMethod("System.Object", "Equals", "System.Boolean", "System.Object", "System.Object");
            environmentExitReference = ResolveMethod("System.Environment", "Exit", "System.Void", "System.Int32");
            stringGetLengthReference = ResolveMethod("System.String", "get_Length", "System.Int32");
            randomCtorReference = ResolveMethod("System.Random", ".ctor", "System.Void");
            randomNextReference = ResolveMethod("System.Random", "Next", "System.Int32", "System.Int32", "System.Int32");
            randomNextDoubleReference = ResolveMethod("System.Random", "NextDouble", "System.Double");
        }

        public void Emit()
        {
            if (diagnostics.Count(d => d.Level == ErrorLevel.Error) > 0)
                return;

            foreach (var variable in program.GlobalVariables)
                AddGlobalVariable(variable);

            foreach (var func in program.Functions.Keys)
                EmitFunctionDecleration(func);

            foreach (var func in program.Functions)
                EmitFunctionBody(func.Key, func.Value);


            if (program.GlobalFunction != FunctionSymbol.Invalid || needsRandom)
            {
                const MethodAttributes attrs = MethodAttributes.SpecialName | MethodAttributes.Static | MethodAttributes.Private | MethodAttributes.RTSpecialName;
                var staticCtor = new MethodDefinition(".cctor", attrs, ResolveType("System.Void"));
                mainClass!.Methods.Add(staticCtor);
                var ilProcessor = staticCtor.Body.GetILProcessor();

                if (needsRandom)
                {
                    ilProcessor.Emit(OpCodes.Newobj, randomCtorReference);
                    ilProcessor.Emit(OpCodes.Stsfld, randomDefiniton);
                }

                if (!(program.GlobalFunction is null))
                    ilProcessor.Emit(OpCodes.Call, functions[program.GlobalFunction]);

                ilProcessor.Emit(OpCodes.Ret);
            }

            mainAssembly.MainModule.Types.Add(mainClass);
            if (!(program.MainFunction is null))
                mainAssembly.EntryPoint = functions[program.MainFunction];
        }

        public void WriteTo(string outputPath) => mainAssembly.Write(outputPath);

        public void WriteTo(Stream stream) => mainAssembly.Write(stream);

        private void AddGlobalVariable(VariableSymbol variable)
        {
            if (variable.Constant is null)
            {
                const FieldAttributes attrs = FieldAttributes.Static | FieldAttributes.Private;
                var type = resolvedTypes[variable.Type];
                var field = new FieldDefinition(variable.Name, attrs, type);
                globalVariables.Add(variable, field);
                mainClass!.Fields.Add(field);
            }
        }

        private void EmitFunctionDecleration(FunctionSymbol symbol)
        {
            const MethodAttributes attrs = MethodAttributes.Static | MethodAttributes.Private;
            var returnType = resolvedTypes[symbol.ReturnType];
            var function = new MethodDefinition(symbol.Name, attrs, returnType);

            foreach (var parameter in symbol.Parameters)
            {
                var type = resolvedTypes[parameter.Type];
                var parameterDefinition = new ParameterDefinition(parameter.Name, ParameterAttributes.None, type);
                function.Parameters.Add(parameterDefinition);
            }

            functions.Add(symbol, function);
            mainClass!.Methods.Add(function);
        }

        private void EmitFunctionBody(FunctionSymbol symbol, BoundBlockStatement body)
        {
            var function = functions[symbol];
            locals.Clear();
            fixups.Clear();
            labels.Clear();
            var ilProcessor = function!.Body.GetILProcessor();

            foreach (var statement in body.Statements)
                EmitStatement(ilProcessor, statement);

            foreach (var (index, label) in fixups)
            {
                var targetInst = ilProcessor.Body.Instructions[labels[label]];
                var instToFix = ilProcessor.Body.Instructions[index];
                instToFix.Operand = targetInst;
            }
        }

        private void EmitStatement(ILProcessor ilProcessor, BoundStatement node)
        {
            switch (node.Kind)
            {
                case BoundNodeKind.BoundVariableDeclarationStatement:
                    EmitVariableDeclarationStatement(ilProcessor, (BoundVariableDeclarationStatement)node);
                    break;
                case BoundNodeKind.BoundConditionalGotoStatement:
                    EmitConditionalGotoStatement(ilProcessor, (BoundConditionalGotoStatement)node);
                    break;
                case BoundNodeKind.BoundGotoStatement:
                    EmitGotoStatement(ilProcessor, (BoundGotoStatement)node);
                    break;
                case BoundNodeKind.BoundLabelStatement:
                    EmitLabelStatement(ilProcessor, (BoundLabelStatement)node);
                    break;
                case BoundNodeKind.BoundReturnStatement:
                    EmitReturnStatement(ilProcessor, (BoundReturnStatement)node);
                    break;
                case BoundNodeKind.BoundExpressionStatement:
                    EmitExpressionStatement(ilProcessor, (BoundExpressionStatement)node);
                    break;
                case BoundNodeKind.BoundNopStatement:
                    ilProcessor.Emit(OpCodes.Nop);
                    break;
                default: throw new Exception("Unexpected kind");
            }
        }

        private void EmitVariableDeclarationStatement(ILProcessor ilProcessor, BoundVariableDeclarationStatement node)
        {
            if (node.Variable.Constant is null)
            {
                if (node.Variable is GlobalVariableSymbol globalVariable)
                {
                    var field = globalVariables[globalVariable];
                    EmitExpression(ilProcessor, node.Expression);
                    ilProcessor.Emit(OpCodes.Stsfld, field);
                }
                else if (node.Variable is LocalVariableSymbol localVariable)
                {
                    var type = resolvedTypes[localVariable.Type];
                    var variable = new VariableDefinition(type);
                    locals.Add(localVariable, variable);
                    ilProcessor.Body.Variables.Add(variable);

                    EmitExpression(ilProcessor, node.Expression);
                    ilProcessor.Emit(OpCodes.Stloc, variable);
                }
                else throw new Exception("Unexpected VariableSymbol");
            }
        }

        private void EmitLabelStatement(ILProcessor ilProcessor, BoundLabelStatement node)
        {
            labels.Add(node.Label, ilProcessor.Body.Instructions.Count);
        }

        private void EmitGotoStatement(ILProcessor ilProcessor, BoundGotoStatement node)
        {
            fixups.Add((ilProcessor.Body.Instructions.Count, node.Label));
            ilProcessor.Emit(OpCodes.Br, Instruction.Create(OpCodes.Nop));
        }

        private void EmitConditionalGotoStatement(ILProcessor ilProcessor, BoundConditionalGotoStatement node)
        {
            EmitExpression(ilProcessor, node.Condition);

            fixups.Add((ilProcessor.Body.Instructions.Count, node.Label));
            var opCode = node.JumpIfFalse ? OpCodes.Brfalse : OpCodes.Brtrue;
            ilProcessor.Emit(opCode, Instruction.Create(OpCodes.Nop));
        }

        private void EmitReturnStatement(ILProcessor ilProcessor, BoundReturnStatement node)
        {
            if (!(node.Expression is null))
                EmitExpression(ilProcessor, node.Expression);
            ilProcessor.Emit(OpCodes.Ret);
        }

        private void EmitExpressionStatement(ILProcessor ilProcessor, BoundExpressionStatement node)
        {
            EmitExpression(ilProcessor, node.Expression);

            if (node.Expression.ResultType != TypeSymbol.Void && node.ShouldPop)
                ilProcessor.Emit(OpCodes.Pop);
        }

        private void EmitExpression(ILProcessor ilProcessor, BoundExpression node)
        {
            if (node.Constant is null)
                switch (node.Kind)
                {
                    case BoundNodeKind.BoundVariableExpression:
                        EmitVariableExpression(ilProcessor, (BoundVariableExpression)node);
                        break;
                    case BoundNodeKind.BoundUnaryExpression:
                        EmitUnaryExpression(ilProcessor, (BoundUnaryExpression)node);
                        break;
                    case BoundNodeKind.BoundBinaryExpression:
                        EmitBinaryExpression(ilProcessor, (BoundBinaryExpression)node);
                        break;
                    case BoundNodeKind.BoundCallExpression:
                        EmitCallExpression(ilProcessor, (BoundCallExpression)node);
                        break;
                    case BoundNodeKind.BoundConversionExpression:
                        EmitConversionExpression(ilProcessor, (BoundConversionExpression)node);
                        break;
                    case BoundNodeKind.BoundAssignmentExpression:
                        EmitAssignmentExpression(ilProcessor, (BoundAssignmentExpression)node);
                        break;
                    case BoundNodeKind.BoundNewArray:
                        EmitNewArrayExpession(ilProcessor, (BoundNewArray)node);
                        break;
                    case BoundNodeKind.BoundStatementExpression:
                        EmitStatementExpression(ilProcessor, (BoundStatementExpression)node);
                        break;
                    default: throw new Exception($"Unexpected kind <{node.Kind}>");
                }
            else
                EmitConstatnt(ilProcessor, node.Constant);
        }

        private void EmitStatementExpression(ILProcessor ilProcessor, BoundStatementExpression node)
        {
            // TODO do this somewhere else
            var statements = Lowerer.Flatten(FunctionSymbol.Invalid, node.Statement).Statements;
            foreach (var stmt in statements)
                EmitStatement(ilProcessor, stmt);
        }

        private void EmitNewArrayExpession(ILProcessor ilProcessor, BoundNewArray node)
        {
            EmitExpression(ilProcessor, node.Size);
            var type = resolvedTypes[node.UnderlyingType];
            ilProcessor.Emit(OpCodes.Newarr, type);
        }

        private void EmitConstatnt(ILProcessor ilProcessor, BoundConstant constant)
        {
            var value = constant.Value;

            if (value is int i)
            {
                switch (i)
                {
                    case -1: ilProcessor.Emit(OpCodes.Ldc_I4_M1); break;
                    case 0: ilProcessor.Emit(OpCodes.Ldc_I4_0); break;
                    case 1: ilProcessor.Emit(OpCodes.Ldc_I4_1); break;
                    case 2: ilProcessor.Emit(OpCodes.Ldc_I4_2); break;
                    case 3: ilProcessor.Emit(OpCodes.Ldc_I4_3); break;
                    case 4: ilProcessor.Emit(OpCodes.Ldc_I4_4); break;
                    case 5: ilProcessor.Emit(OpCodes.Ldc_I4_5); break;
                    case 6: ilProcessor.Emit(OpCodes.Ldc_I4_6); break;
                    case 7: ilProcessor.Emit(OpCodes.Ldc_I4_7); break;
                    case 8: ilProcessor.Emit(OpCodes.Ldc_I4_8); break;
                    default:
                        if (i <= sbyte.MaxValue)
                            ilProcessor.Emit(OpCodes.Ldc_I4_S, (sbyte)i);
                        else
                            ilProcessor.Emit(OpCodes.Ldc_I4, i);
                        break;
                }
            }
            else if (value is double d)
            {
                ilProcessor.Emit(OpCodes.Ldc_R8, d);
            }
            else if (value is bool b)
            {
                var opcode = b ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0;
                ilProcessor.Emit(opcode);
            }
            else if (value is string s)
            {
                ilProcessor.Emit(OpCodes.Ldstr, s);
            }
            else if (value is null)
            {
                ilProcessor.Emit(OpCodes.Ldnull);
            }
            else throw new Exception($"Unexpected constant type ${value.GetType()}");
        }

        private void EmitVariableExpression(ILProcessor ilProcessor, BoundVariableExpression node)
        {
            if (node.Variable is GlobalVariableSymbol globalVariable)
            {
                var field = globalVariables[globalVariable];
                ilProcessor.Emit(OpCodes.Ldsfld, field);
            }
            else if (node.Variable is LocalVariableSymbol localVariable)
            {
                var variable = locals[localVariable];
                ilProcessor.Emit(OpCodes.Ldloc, variable);
            }
            else if (node.Variable is ParameterSymbol parameter)
            {
                ilProcessor.Emit(OpCodes.Ldarg, parameter.Index);
            }
            else throw new Exception("Unexpected VariableSymbol");
        }

        private void EmitUnaryExpression(ILProcessor ilProcessor, BoundUnaryExpression node)
        {
            EmitExpression(ilProcessor, node.Expression);

            switch (node.Op)
            {
                case BoundUnaryOperator.Identety:
                    // Nothing
                    break;
                case BoundUnaryOperator.Negation:
                    ilProcessor.Emit(OpCodes.Neg);
                    break;
                case BoundUnaryOperator.LogicalNot:
                    ilProcessor.Emit(OpCodes.Ldc_I4_0);
                    ilProcessor.Emit(OpCodes.Ceq);
                    break;
                case BoundUnaryOperator.BitwiseNot:
                    ilProcessor.Emit(OpCodes.Not);
                    break;
                default: throw new Exception($"Unexpceted unary operator {node.Op}");
            }
        }

        private void EmitBinaryExpression(ILProcessor ilProcessor, BoundBinaryExpression node)
        {
            var leftType = node.Left.ResultType;
            var rightType = node.Right.ResultType;

            EmitExpression(ilProcessor, node.Left);
            EmitExpression(ilProcessor, node.Right);


            if (node.Op == BoundBinaryOperator.Addition && leftType == TypeSymbol.String && rightType == TypeSymbol.String)
            {
                ilProcessor.Emit(OpCodes.Call, stringConcatReference);
                return;
            }

            if (node.Op == BoundBinaryOperator.EqualEqual && leftType == TypeSymbol.String && rightType == TypeSymbol.String)
            {
                ilProcessor.Emit(OpCodes.Call, stringEqualsReference);
                return;
            }

            if (node.Op == BoundBinaryOperator.EqualEqual && leftType == TypeSymbol.Obj && rightType == TypeSymbol.Obj)
            {
                ilProcessor.Emit(OpCodes.Call, objectEqualsReference);
                return;
            }

            if (node.Op == BoundBinaryOperator.NotEqual && leftType == TypeSymbol.String && rightType == TypeSymbol.String)
            {
                ilProcessor.Emit(OpCodes.Call, stringEqualsReference);
                ilProcessor.Emit(OpCodes.Ldc_I4_0);
                ilProcessor.Emit(OpCodes.Ceq);
                return;
            }

            if (node.Op == BoundBinaryOperator.NotEqual && leftType == TypeSymbol.Obj && rightType == TypeSymbol.Obj)
            {
                ilProcessor.Emit(OpCodes.Callvirt, objectEqualsReference);
                ilProcessor.Emit(OpCodes.Ldc_I4_0);
                ilProcessor.Emit(OpCodes.Ceq);
                return;
            }


            switch (node.Op)
            {
                case BoundBinaryOperator.Addition:
                    ilProcessor.Emit(OpCodes.Add);
                    break;
                case BoundBinaryOperator.Subtraction:
                    ilProcessor.Emit(OpCodes.Sub);
                    break;
                case BoundBinaryOperator.Multiplication:
                    ilProcessor.Emit(OpCodes.Mul);
                    break;
                case BoundBinaryOperator.Division:
                    ilProcessor.Emit(OpCodes.Div);
                    break;
                case BoundBinaryOperator.Modulo:
                    ilProcessor.Emit(OpCodes.Rem);
                    break;
                case BoundBinaryOperator.Power:
                    ilProcessor.Emit(OpCodes.Call, mathPowReference);
                    break;
                case BoundBinaryOperator.EqualEqual:
                    ilProcessor.Emit(OpCodes.Ceq);
                    break;
                case BoundBinaryOperator.NotEqual:
                    ilProcessor.Emit(OpCodes.Ceq);
                    ilProcessor.Emit(OpCodes.Ldc_I4_0);
                    ilProcessor.Emit(OpCodes.Ceq);
                    break;
                case BoundBinaryOperator.LessThan:
                    ilProcessor.Emit(OpCodes.Clt);
                    break;
                case BoundBinaryOperator.GreaterThan:
                    ilProcessor.Emit(OpCodes.Cgt);
                    break;
                case BoundBinaryOperator.LessEqual:
                    ilProcessor.Emit(OpCodes.Cgt);
                    ilProcessor.Emit(OpCodes.Ldc_I4_0);
                    ilProcessor.Emit(OpCodes.Ceq);
                    break;
                case BoundBinaryOperator.GreaterEqual:
                    ilProcessor.Emit(OpCodes.Clt);
                    ilProcessor.Emit(OpCodes.Ldc_I4_0);
                    ilProcessor.Emit(OpCodes.Ceq);
                    break;
                case BoundBinaryOperator.BitwiseAnd:
                    ilProcessor.Emit(OpCodes.And);
                    break;
                case BoundBinaryOperator.BitwiseOr:
                    ilProcessor.Emit(OpCodes.Or);
                    break;
                case BoundBinaryOperator.BitwiseXor:
                    ilProcessor.Emit(OpCodes.Xor);
                    break;

                default: throw new Exception($"Unexpected binary operator {node.Op}");
            }
        }

        private void EmitCallExpression(ILProcessor ilProcessor, BoundCallExpression node)
        {
            if (node.Symbol == BuiltInFunctions.Random || node.Symbol == BuiltInFunctions.RandomFloat)
            {
                if (randomDefiniton is null)
                {
                    randomDefiniton = new FieldDefinition("$random", FieldAttributes.Static | FieldAttributes.Private, randomTypeReference);
                    mainClass!.Fields.Add(randomDefiniton);
                    needsRandom = true;
                }

                ilProcessor.Emit(OpCodes.Ldsfld, randomDefiniton);
            }

            foreach (var arg in node.Arguments)
                EmitExpression(ilProcessor, arg);

            if (node.Symbol == BuiltInFunctions.Print)
                ilProcessor.Emit(OpCodes.Call, consoleWriteReference);
            else if (node.Symbol == BuiltInFunctions.PrintLine)
                ilProcessor.Emit(OpCodes.Call, consoleWriteLineReference);
            else if (node.Symbol == BuiltInFunctions.Input)
                ilProcessor.Emit(OpCodes.Call, cosnoleReadLineReference);
            else if (node.Symbol == BuiltInFunctions.Len)
                ilProcessor.Emit(OpCodes.Call, stringGetLengthReference);
            else if (node.Symbol == BuiltInFunctions.Clear)
                ilProcessor.Emit(OpCodes.Call, cosnoleClearReference);
            else if (node.Symbol == BuiltInFunctions.Exit)
                ilProcessor.Emit(OpCodes.Call, environmentExitReference);
            else if (node.Symbol == BuiltInFunctions.Random)
                ilProcessor.Emit(OpCodes.Callvirt, randomNextReference);
            else if (node.Symbol == BuiltInFunctions.RandomFloat)
                ilProcessor.Emit(OpCodes.Callvirt, randomNextDoubleReference);
            else
            {
                var function = functions[node.Symbol!];
                ilProcessor.Emit(OpCodes.Call, function);
            }

        }

        private void EmitConversionExpression(ILProcessor ilProcessor, BoundConversionExpression node)
        {
            var from = node.Expression.ResultType.Name;
            var to = node.ResultType.Name;

            EmitExpression(ilProcessor, node.Expression);

            switch (from, to)
            {

                case ("int", "float"):
                    ilProcessor.Emit(OpCodes.Conv_R8);
                    break;
                case ("float", "int"):
                    ilProcessor.Emit(OpCodes.Conv_I8);
                    break;
                case ("obj", "str"):
                    ilProcessor.Emit(OpCodes.Call, convertToStringReference);
                    break;
                case ("int", "str"):
                case ("float", "str"):
                case ("bool", "str"):
                    var type1 = resolvedTypes[node.Expression.ResultType];
                    ilProcessor.Emit(OpCodes.Box, type1);
                    ilProcessor.Emit(OpCodes.Call, convertToStringReference);
                    break;
                case ("int", "obj"):
                case ("float", "obj"):
                case ("bool", "obj"):
                    var type2 = resolvedTypes[node.Expression.ResultType];
                    ilProcessor.Emit(OpCodes.Box, type2);
                    break;
                case ("str", "obj"):
                    break;
                case ("obj", "int"):
                case ("obj", "float"):
                case ("obj", "bool"):
                    var type3 = resolvedTypes[node.ResultType];
                    ilProcessor.Emit(OpCodes.Unbox_Any, type3);
                    break;
                default:
                    if (to == "obj" && node.Expression.ResultType is ArrayTypeSymbol)
                        break;
                    else
                        throw new Exception($"Unexpected conversion from {from} to {to}");
            }
        }

        private void EmitAssignmentExpression(ILProcessor ilProcessor, BoundAssignmentExpression node)
        {
            if (!(node.Variable!.Constant is null))
                return;

            if (node.Variable is GlobalVariableSymbol globalVariable)
            {
                var field = globalVariables[globalVariable];
                EmitExpression(ilProcessor, node.Expression);
                ilProcessor.Emit(OpCodes.Dup);
                ilProcessor.Emit(OpCodes.Stsfld, field);
            }
            else if (node.Variable is LocalVariableSymbol localVariable)
            {
                var variable = locals[localVariable];
                EmitExpression(ilProcessor, node.Expression);
                ilProcessor.Emit(OpCodes.Dup);
                ilProcessor.Emit(OpCodes.Stloc, variable);
            }
            else if (node.Variable is ParameterSymbol parameter)
            {
                EmitExpression(ilProcessor, node.Expression);
                ilProcessor.Emit(OpCodes.Dup);
                ilProcessor.Emit(OpCodes.Starg, parameter.Index);
            }
            else throw new Exception("Unexpected VariableSymbol");
        }

        private TypeReference? ResolveType(string metadataName)
        {
            var definition = ResolveTypeDefinition(metadataName);
            if (definition is null)
                return null;
            return mainAssembly.MainModule.ImportReference(definition);
        }

        private TypeDefinition? ResolveTypeDefinition(string metadataName)
        {
            var foundTypes = references.SelectMany(a => a.Modules)
                                           .SelectMany(m => m.Types)
                                           .Where(t => t.FullName == metadataName)
                                           .ToArray();

            if (foundTypes.Length == 1)
            {
                return foundTypes.Single();
            }
            else if (foundTypes.Length == 0)
            {
                diagnostics.ReportError(ErrorMessage.MissingRequiredType, TextLocation.Undefined, metadataName);
                return null;
            }
            else
            {
                var names = foundTypes.Select(t => t.Module.Assembly.Name.Name);
                diagnostics.ReportError(ErrorMessage.AmbiguousRequiredType, TextLocation.Undefined, metadataName, string.Join(", ", names));
                return null;
            }
        }

        private MethodReference? ResolveMethod(string type, string name, string returnType, params string[] parameterTypes)
        {
            var definition = ResolveMethodDefinition(type, name, returnType, parameterTypes);
            if (definition is null)
                return null;
            return mainAssembly.MainModule.ImportReference(definition);
        }

        private MethodDefinition? ResolveMethodDefinition(string type, string name, string returnType, params string[] parameterTypes)
        {
            var returnTypeDef = ResolveTypeDefinition(returnType);
            var typeDef = ResolveTypeDefinition(type);

            if (returnTypeDef is null || typeDef is null)
            {
                var missingName = typeDef is null ? type : returnType;
                diagnostics.ReportError(ErrorMessage.MissingRequiredType, TextLocation.Undefined, missingName);
                return null;
            }

            var fullName = $"{returnTypeDef.FullName} {typeDef.FullName}::{name}({string.Join(",", parameterTypes)})";
            var foundMethods = typeDef.Methods.Where(m => m.FullName == fullName);

            if (foundMethods.Count() == 1)
                return foundMethods.Single();
            else if (foundMethods.Count() == 0)
            {
                diagnostics.ReportError(ErrorMessage.MissingRequiredMethod, TextLocation.Undefined, $"{typeDef.FullName}.{name}({string.Join(", ", parameterTypes)})");
                return null;
            }
            else
            {
                var methodDecl = $"{typeDef.FullName}.{name}({string.Join(", ", parameterTypes)})";
                var names = foundMethods.Select(t => t.Module.Assembly.Name.Name);
                diagnostics.ReportError(ErrorMessage.AmbiguousRequiredMethod, TextLocation.Undefined, methodDecl, string.Join(", ", names));
                return null;
            }
        }
    }
}