using System;
using System.Linq;
using System.Collections.Generic;
using Compiler.Binding;
using Compiler.Diagnostics;
using Compiler.Symbols;
using Compiler.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Compiler.Emit
{
    internal sealed class Emiter : IDiagnostable
    {
        private readonly BoundProgram program;
        private readonly DiagnosticBag diagnostics;
        private readonly AssemblyDefinition mainAssebly;
        private readonly TypeDefinition mainClass;
        private readonly List<AssemblyDefinition> references;
        private readonly Dictionary<TypeSymbol, TypeReference> builtInTypes;
        private readonly Dictionary<FunctionSymbol, MethodDefinition> functions;
        private readonly Dictionary<BoundLabel, int> labels;
        private readonly List<(int, BoundLabel)> fixups;

        private readonly TypeReference randomTypeReference;
        private readonly MethodReference randomCtorReference;
        private readonly MethodReference randomNextReference;
        private readonly MethodReference randomNextDoubleReference;

        private readonly MethodReference consoleWriteReference;
        private readonly MethodReference cosnoleReadLineReference;
        private readonly MethodReference cosnoleClearReference;
        private readonly MethodReference stringConcatReference;
        private readonly MethodReference mathPowReference;
        private readonly MethodReference convertToStringReference;
        private readonly MethodReference stringEqualsReference;
        private readonly MethodReference objectEqualsReference;
        private readonly MethodReference environmentExitReference;
        private readonly MethodReference stringGetLengthReference;

        private readonly Dictionary<GlobalVariableSymbol, FieldDefinition> globalVariables;
        private readonly Dictionary<LocalVariableSymbol, VariableDefinition> locals;

        private FieldDefinition randomDefiniton;
        private MethodDefinition staticCtor;

        public IEnumerable<Diagnostic> GetDiagnostics() => diagnostics;

        public Emiter(BoundProgram program, string moduleName, string[] referencePaths)
        {
            this.program = program;
            this.diagnostics = new DiagnosticBag();
            this.functions = new Dictionary<FunctionSymbol, MethodDefinition>();
            this.builtInTypes = new Dictionary<TypeSymbol, TypeReference>();
            this.globalVariables = new Dictionary<GlobalVariableSymbol, FieldDefinition>();
            this.locals = new Dictionary<LocalVariableSymbol, VariableDefinition>();
            this.labels = new Dictionary<BoundLabel, int>();
            this.fixups = new List<(int, BoundLabel)>();

            var assembylInfo = new AssemblyNameDefinition(moduleName, new Version(1, 0));
            mainAssebly = AssemblyDefinition.CreateAssembly(assembylInfo, moduleName, ModuleKind.Console);
            references = new List<AssemblyDefinition>();
            foreach (var reference in referencePaths)
            {
                try
                {
                    var assembly = AssemblyDefinition.ReadAssembly(reference);
                    references.Add(assembly);
                }
                catch (BadImageFormatException)
                {
                    diagnostics.ReportError(ErrorMessage.InvalidReference, TextLocation.Undefined, reference);
                    return;
                }
            }

            builtInTypes.Add(TypeSymbol.Any, ResolveType("System.Object"));
            builtInTypes.Add(TypeSymbol.Int, ResolveType("System.Int32"));
            builtInTypes.Add(TypeSymbol.Float, ResolveType("System.Double"));
            builtInTypes.Add(TypeSymbol.Bool, ResolveType("System.Boolean"));
            builtInTypes.Add(TypeSymbol.String, ResolveType("System.String"));
            builtInTypes.Add(TypeSymbol.Void, ResolveType("System.Void"));
            randomTypeReference = ResolveType("System.Random");

            mainClass = new TypeDefinition("", "Program", TypeAttributes.Abstract | TypeAttributes.Sealed | TypeAttributes.Public, builtInTypes[TypeSymbol.Any]);
            consoleWriteReference = ResolveMethod("System.Console", "Write", "System.Void", "System.Object");
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

        public void Emit(string outputPath)
        {
            if (diagnostics.Count(d => d.Level == ErrorLevel.Error) > 0)
                return;

            foreach (var variable in program.GlobalVariables)
                AddGlobalVariable(variable);

            foreach (var func in program.Functions.Keys)
                EmitFunctionDecleration(func);

            foreach (var func in program.Functions)
                EmitFunctionBody(func.Key, func.Value);

            mainAssebly.MainModule.Types.Add(mainClass);
            mainAssebly.EntryPoint = functions[program.MainFunction];
            mainAssebly.Write(outputPath);
        }

        private void AddGlobalVariable(GlobalVariableSymbol variable)
        {
            if (variable.Constant == null)
            {
                const FieldAttributes attrs = FieldAttributes.Static | FieldAttributes.Private;
                var type = builtInTypes[variable.Type];
                var field = new FieldDefinition(variable.Name, attrs, type);
                globalVariables.Add(variable, field);
                mainClass.Fields.Add(field);
            }
        }

        private void EmitFunctionDecleration(FunctionSymbol symbol)
        {
            const MethodAttributes attrs = MethodAttributes.Static | MethodAttributes.Private;
            var returnType = builtInTypes[symbol.ReturnType];
            var function = new MethodDefinition(symbol.Name, attrs, returnType);

            foreach (var parameter in symbol.Parameters)
            {
                var type = builtInTypes[parameter.Type];
                var parameterDefinition = new ParameterDefinition(parameter.Name, ParameterAttributes.None, type);
                function.Parameters.Add(parameterDefinition);
            }

            functions.Add(symbol, function);
            mainClass.Methods.Add(function);
        }

        private void EmitFunctionBody(FunctionSymbol symbol, BoundBlockStatement body)
        {
            var function = functions[symbol];
            locals.Clear();
            fixups.Clear();
            labels.Clear();
            var ilProcesser = function.Body.GetILProcessor();

            foreach (var statement in body.Statements)
                EmitStatement(ilProcesser, statement);

            foreach (var (index, label) in fixups)
            {
                var targetInst = ilProcesser.Body.Instructions[labels[label]];
                var instToFix = ilProcesser.Body.Instructions[index];
                instToFix.Operand = targetInst;

            }
        }

        private void EmitStatement(ILProcessor ilProcesser, BoundStatement node)
        {
            switch (node.Kind)
            {
                case BoundNodeKind.BoundVariableDeclarationStatement:
                    EmitVariableDeclarationStatement(ilProcesser, (BoundVariableDeclarationStatement)node);
                    break;
                case BoundNodeKind.BoundConditionalGotoStatement:
                    EmitConditionalGotoStatement(ilProcesser, (BoundConditionalGotoStatement)node);
                    break;
                case BoundNodeKind.BoundGotoStatement:
                    EmitGotoStatement(ilProcesser, (BoundGotoStatement)node);
                    break;
                case BoundNodeKind.BoundLabelStatement:
                    EmitLabelStatement(ilProcesser, (BoundLabelStatement)node);
                    break;
                case BoundNodeKind.BoundReturnStatement:
                    EmitReturnStatement(ilProcesser, (BoundReturnStatement)node);
                    break;
                case BoundNodeKind.BoundExpressionStatement:
                    EmitExpressionStatement(ilProcesser, (BoundExpressionStatement)node);
                    break;
                case BoundNodeKind.BoundNopStatement:
                    ilProcesser.Emit(OpCodes.Nop);
                    break;
                default: throw new Exception("Unexpected kind");
            }
        }

        private void EmitVariableDeclarationStatement(ILProcessor ilProcesser, BoundVariableDeclarationStatement node)
        {
            if (node.Variable.IsConst)
                return;

            if (node.Variable is GlobalVariableSymbol globalVariable)
            {
                var field = globalVariables[globalVariable];
                EmitExpression(ilProcesser, node.Expression);
                ilProcesser.Emit(OpCodes.Stsfld, field);
            }
            else if (node.Variable is LocalVariableSymbol localVariable)
            {
                var type = builtInTypes[localVariable.Type];
                var variable = new VariableDefinition(type);
                locals.Add(localVariable, variable);
                ilProcesser.Body.Variables.Add(variable);

                EmitExpression(ilProcesser, node.Expression);
                ilProcesser.Emit(OpCodes.Stloc, variable);
            }
            else throw new Exception("Unexpected VariableSymbol");
        }

        private void EmitLabelStatement(ILProcessor ilProcesser, BoundLabelStatement node)
        {
            labels.Add(node.Label, ilProcesser.Body.Instructions.Count);
        }

        private void EmitGotoStatement(ILProcessor ilProcesser, BoundGotoStatement node)
        {
            fixups.Add((ilProcesser.Body.Instructions.Count, node.Label));
            ilProcesser.Emit(OpCodes.Br, Instruction.Create(OpCodes.Nop));
        }

        private void EmitConditionalGotoStatement(ILProcessor ilProcesser, BoundConditionalGotoStatement node)
        {
            EmitExpression(ilProcesser, node.Condition);

            fixups.Add((ilProcesser.Body.Instructions.Count, node.Label));
            var opCode = node.JumpIfFalse ? OpCodes.Brfalse : OpCodes.Brtrue;
            ilProcesser.Emit(opCode, Instruction.Create(OpCodes.Nop));
        }

        private void EmitReturnStatement(ILProcessor ilProcesser, BoundReturnStatement node)
        {
            if (node.Expression != null)
                EmitExpression(ilProcesser, node.Expression);
            ilProcesser.Emit(OpCodes.Ret);
        }

        private void EmitExpressionStatement(ILProcessor ilProcesser, BoundExpressionStatement node)
        {
            EmitExpression(ilProcesser, node.Expression);

            if (node.Expression.ResultType != TypeSymbol.Void)
                ilProcesser.Emit(OpCodes.Pop);
        }

        private void EmitExpression(ILProcessor ilProcesser, BoundExpression node)
        {
            if (node.HasConstant)
            {
                EmitConstatnt(ilProcesser, node);
                return;
            }

            switch (node.Kind)
            {
                case BoundNodeKind.BoundVariableExpression:
                    EmitVariableExpression(ilProcesser, (BoundVariableExpression)node);
                    break;
                case BoundNodeKind.BoundUnaryExpression:
                    EmitUnaryExpression(ilProcesser, (BoundUnaryExpression)node);
                    break;
                case BoundNodeKind.BoundBinaryExpression:
                    EmitBinaryExpression(ilProcesser, (BoundBinaryExpression)node);
                    break;
                case BoundNodeKind.BoundCallExpression:
                    EmitCallExpression(ilProcesser, (BoundCallExpression)node);
                    break;
                case BoundNodeKind.BoundConversionExpression:
                    EmitConversionExpression(ilProcesser, (BoundConversionExpression)node);
                    break;
                case BoundNodeKind.BoundAssignmentExpression:
                    EmitAssignmentExpression(ilProcesser, (BoundAssignmentExpression)node);
                    break;
            }
        }

        private void EmitConstatnt(ILProcessor ilProcesser, BoundExpression node)
        {
            var value = node.Constant.Value;

            if (value is int i)
            {
                switch (i)
                {
                    case -1: ilProcesser.Emit(OpCodes.Ldc_I4_M1); break;
                    case 0: ilProcesser.Emit(OpCodes.Ldc_I4_0); break;
                    case 1: ilProcesser.Emit(OpCodes.Ldc_I4_1); break;
                    case 2: ilProcesser.Emit(OpCodes.Ldc_I4_2); break;
                    case 3: ilProcesser.Emit(OpCodes.Ldc_I4_3); break;
                    case 4: ilProcesser.Emit(OpCodes.Ldc_I4_4); break;
                    case 5: ilProcesser.Emit(OpCodes.Ldc_I4_5); break;
                    case 6: ilProcesser.Emit(OpCodes.Ldc_I4_6); break;
                    case 7: ilProcesser.Emit(OpCodes.Ldc_I4_7); break;
                    case 8: ilProcesser.Emit(OpCodes.Ldc_I4_8); break;
                    default:
                        if (i <= sbyte.MaxValue)
                            ilProcesser.Emit(OpCodes.Ldc_I4_S, (sbyte)i);
                        else
                            ilProcesser.Emit(OpCodes.Ldc_I4, i);
                        break;
                }
            }
            else if (value is double d)
            {
                ilProcesser.Emit(OpCodes.Ldc_R8, d);
            }
            else if (value is bool b)
            {
                var opcode = b ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0;
                ilProcesser.Emit(opcode);
            }
            else if (value is string s)
            {
                ilProcesser.Emit(OpCodes.Ldstr, s);
            }
            else throw new Exception($"Unexpected constant type ${value.GetType()}");
        }

        private void EmitVariableExpression(ILProcessor ilProcesser, BoundVariableExpression node)
        {
            if (node.Variable is GlobalVariableSymbol globalVariable)
            {
                var field = globalVariables[globalVariable];
                ilProcesser.Emit(OpCodes.Ldsfld, field);
            }
            else if (node.Variable is LocalVariableSymbol localVariable)
            {
                var variable = locals[localVariable];
                ilProcesser.Emit(OpCodes.Ldloc, variable);
            }
            else if (node.Variable is ParameterSymbol parameter)
            {
                ilProcesser.Emit(OpCodes.Ldarg, parameter.Index);
            }
            else throw new Exception("Unexpected VariableSymbol");
        }

        private void EmitUnaryExpression(ILProcessor ilProcesser, BoundUnaryExpression node)
        {
            EmitExpression(ilProcesser, node.Expression);

            switch (node.Op)
            {
                case BoundUnaryOperator.Identety:
                    // Nothing
                    break;
                case BoundUnaryOperator.Negation:
                    ilProcesser.Emit(OpCodes.Neg);
                    break;
                case BoundUnaryOperator.LogicalNot:
                    ilProcesser.Emit(OpCodes.Ldc_I4_0);
                    ilProcesser.Emit(OpCodes.Ceq);
                    break;
                case BoundUnaryOperator.BitwiseNot:
                    ilProcesser.Emit(OpCodes.Not);
                    break;
                default: throw new Exception($"Unexpceted unary operator {node.Op}");
            }
        }

        private void EmitBinaryExpression(ILProcessor ilProcesser, BoundBinaryExpression node)
        {
            var leftType = node.Left.ResultType;
            var rightType = node.Right.ResultType;

            EmitExpression(ilProcesser, node.Left);
            EmitExpression(ilProcesser, node.Right);


            if (node.Op == BoundBinaryOperator.Addition && leftType == TypeSymbol.String && rightType == TypeSymbol.String)
            {
                ilProcesser.Emit(OpCodes.Call, stringConcatReference);
                return;
            }

            if (node.Op == BoundBinaryOperator.EqualEqual && leftType == TypeSymbol.String && rightType == TypeSymbol.String)
            {
                ilProcesser.Emit(OpCodes.Call, stringEqualsReference);
                return;
            }

            if (node.Op == BoundBinaryOperator.EqualEqual && leftType == TypeSymbol.Any && rightType == TypeSymbol.Any)
            {
                ilProcesser.Emit(OpCodes.Call, objectEqualsReference);
                return;
            }

            if (node.Op == BoundBinaryOperator.NotEqual && leftType == TypeSymbol.String && rightType == TypeSymbol.String)
            {
                ilProcesser.Emit(OpCodes.Call, stringEqualsReference);
                ilProcesser.Emit(OpCodes.Ldc_I4_0);
                ilProcesser.Emit(OpCodes.Ceq);
                return;
            }

            if (node.Op == BoundBinaryOperator.NotEqual && leftType == TypeSymbol.Any && rightType == TypeSymbol.Any)
            {
                ilProcesser.Emit(OpCodes.Callvirt, objectEqualsReference);
                ilProcesser.Emit(OpCodes.Ldc_I4_0);
                ilProcesser.Emit(OpCodes.Ceq);
                return;
            }


            switch (node.Op)
            {
                case BoundBinaryOperator.Addition:
                    ilProcesser.Emit(OpCodes.Add);
                    break;
                case BoundBinaryOperator.Subtraction:
                    ilProcesser.Emit(OpCodes.Sub);
                    break;
                case BoundBinaryOperator.Multiplication:
                    ilProcesser.Emit(OpCodes.Mul);
                    break;
                case BoundBinaryOperator.Division:
                    ilProcesser.Emit(OpCodes.Div);
                    break;
                case BoundBinaryOperator.Modulo:
                    ilProcesser.Emit(OpCodes.Rem);
                    break;
                case BoundBinaryOperator.Power:
                    ilProcesser.Emit(OpCodes.Call, mathPowReference);
                    break;
                case BoundBinaryOperator.EqualEqual:
                    ilProcesser.Emit(OpCodes.Ceq);
                    break;
                case BoundBinaryOperator.NotEqual:
                    ilProcesser.Emit(OpCodes.Ceq);
                    ilProcesser.Emit(OpCodes.Ldc_I4_0);
                    ilProcesser.Emit(OpCodes.Ceq);
                    break;
                case BoundBinaryOperator.LessThan:
                    ilProcesser.Emit(OpCodes.Clt);
                    break;
                case BoundBinaryOperator.GreaterThan:
                    ilProcesser.Emit(OpCodes.Cgt);
                    break;
                case BoundBinaryOperator.LessEqual:
                    ilProcesser.Emit(OpCodes.Cgt);
                    ilProcesser.Emit(OpCodes.Ldc_I4_0);
                    ilProcesser.Emit(OpCodes.Ceq);
                    break;
                case BoundBinaryOperator.GreaterEqual:
                    ilProcesser.Emit(OpCodes.Clt);
                    ilProcesser.Emit(OpCodes.Ldc_I4_0);
                    ilProcesser.Emit(OpCodes.Ceq);
                    break;
                // TODO short-circuit evalutaion
                case BoundBinaryOperator.LogicalAnd:
                case BoundBinaryOperator.BitwiseAnd:
                    ilProcesser.Emit(OpCodes.And);
                    break;
                // TODO short-circuit evalutaion
                case BoundBinaryOperator.LogicalOr:
                case BoundBinaryOperator.BitwiseOr:
                    ilProcesser.Emit(OpCodes.Or);
                    break;
                case BoundBinaryOperator.BitwiseXor:
                    ilProcesser.Emit(OpCodes.Xor);
                    break;

                default: throw new Exception($"Unexpected binary operator {node.Op}");
            }
        }

        private void EmitCallExpression(ILProcessor ilProcesser, BoundCallExpression node)
        {
            if (node.Symbol == BuiltInFunctions.Random || node.Symbol == BuiltInFunctions.RandomFloat)
            {
                if (randomDefiniton == null)
                {
                    randomDefiniton = new FieldDefinition("random", FieldAttributes.Static | FieldAttributes.Private, randomTypeReference);
                    mainClass.Fields.Add(randomDefiniton);

                    var attrs = MethodAttributes.SpecialName | MethodAttributes.Static | MethodAttributes.Private | MethodAttributes.RTSpecialName;
                    staticCtor = new MethodDefinition(".cctor", attrs, builtInTypes[TypeSymbol.Void]);
                    mainClass.Methods.Add(staticCtor);

                    var ctorProcessor = staticCtor.Body.GetILProcessor();
                    ctorProcessor.Emit(OpCodes.Newobj, randomCtorReference);
                    ctorProcessor.Emit(OpCodes.Stsfld, randomDefiniton);
                    ctorProcessor.Emit(OpCodes.Ret);
                }
                ilProcesser.Emit(OpCodes.Ldsfld, randomDefiniton);
            }

            foreach (var arg in node.Arguments)
                EmitExpression(ilProcesser, arg);

            if (node.Symbol == BuiltInFunctions.Print)
                ilProcesser.Emit(OpCodes.Call, consoleWriteReference);
            else if (node.Symbol == BuiltInFunctions.Input)
                ilProcesser.Emit(OpCodes.Call, cosnoleReadLineReference);
            else if (node.Symbol == BuiltInFunctions.Len)
                ilProcesser.Emit(OpCodes.Call, stringGetLengthReference);
            else if (node.Symbol == BuiltInFunctions.Clear)
                ilProcesser.Emit(OpCodes.Call, cosnoleClearReference);
            else if (node.Symbol == BuiltInFunctions.Exit)
                ilProcesser.Emit(OpCodes.Call, environmentExitReference);
            else if (node.Symbol == BuiltInFunctions.Random)
                ilProcesser.Emit(OpCodes.Callvirt, randomNextReference);
            else if (node.Symbol == BuiltInFunctions.RandomFloat)
                ilProcesser.Emit(OpCodes.Callvirt, randomNextDoubleReference);
            else
            {
                var function = functions[node.Symbol];
                ilProcesser.Emit(OpCodes.Call, function);
            }

        }

        private void EmitConversionExpression(ILProcessor ilProcesser, BoundConversionExpression node)
        {
            var from = node.Expression.ResultType.Name;
            var to = node.Type.Name;

            EmitExpression(ilProcesser, node.Expression);

            switch (from, to)
            {

                case ("int", "float"):
                    ilProcesser.Emit(OpCodes.Conv_R8);
                    break;
                case ("float", "int"):
                    ilProcesser.Emit(OpCodes.Conv_I8);
                    break;
                case ("any", "str"):
                    ilProcesser.Emit(OpCodes.Call, convertToStringReference);
                    break;
                case ("int", "str"):
                case ("float", "str"):
                case ("bool", "str"):
                    var type1 = builtInTypes[node.Expression.ResultType];
                    ilProcesser.Emit(OpCodes.Box, type1);
                    ilProcesser.Emit(OpCodes.Call, convertToStringReference);
                    break;
                case ("int", "any"):
                case ("float", "any"):
                case ("bool", "any"):
                    var type2 = builtInTypes[node.Expression.ResultType];
                    ilProcesser.Emit(OpCodes.Box, type2);
                    break;
                case ("str", "any"):
                    break;
                case ("any", "int"):
                case ("any", "float"):
                case ("any", "bool"):
                    var type3 = builtInTypes[node.Type];
                    ilProcesser.Emit(OpCodes.Unbox_Any, type3);
                    break;
                default: throw new Exception($"Unexpected conversion from {from} to {to}");
            }
        }

        private void EmitAssignmentExpression(ILProcessor ilProcesser, BoundAssignmentExpression node)
        {
            if (node.Variable.IsConst)
                return;

            if (node.Variable is GlobalVariableSymbol globalVariable)
            {
                var field = globalVariables[globalVariable];
                EmitExpression(ilProcesser, node.Expression);
                ilProcesser.Emit(OpCodes.Stsfld, field);
            }
            else if (node.Variable is LocalVariableSymbol localVariable)
            {
                var variable = locals[localVariable];
                EmitExpression(ilProcesser, node.Expression);
                ilProcesser.Emit(OpCodes.Dup);
                ilProcesser.Emit(OpCodes.Stloc, variable);
            }
            else if (node.Variable is ParameterSymbol parameter)
            {
                EmitExpression(ilProcesser, node.Expression);
                ilProcesser.Emit(OpCodes.Dup);
                ilProcesser.Emit(OpCodes.Starg, parameter.Index);
            }
            else throw new Exception("Unexpected VariableSymbol");
        }

        private TypeReference ResolveType(string metadataName)
        {
            var definition = ResolveTypeDefinition(metadataName);
            if (definition == null)
                return null;
            return mainAssebly.MainModule.ImportReference(definition);
        }

        private TypeDefinition ResolveTypeDefinition(string metadataName)
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

        private MethodReference ResolveMethod(string type, string name, string returnType, params string[] parameterTypes)
        {
            var definition = ResolveMethodDefinition(type, name, returnType, parameterTypes);
            if (definition == null)
                return null;
            return mainAssebly.MainModule.ImportReference(definition);
        }

        private MethodDefinition ResolveMethodDefinition(string type, string name, string returnType, params string[] parameterTypes)
        {
            var returnTypeDef = ResolveTypeDefinition(returnType);
            var typeDef = ResolveTypeDefinition(type);

            if (returnTypeDef == null || typeDef == null)
            {
                var missingName = typeDef == null ? type : returnType;
                diagnostics.ReportError(ErrorMessage.MissingRequiredType, TextLocation.Undefined, missingName);
                return null;
            }

            var fullName = $"{returnTypeDef.FullName} {typeDef.FullName}::{name}({string.Join(",", parameterTypes.Select(p => ResolveTypeDefinition(p).FullName))})";
            var foundMethods = typeDef.Methods.Where(m => m.FullName == fullName);

            if (foundMethods.Count() == 1)
                return foundMethods.Single();
            else if (foundMethods.Count() == 0)
            {
                var parameterTypeNames = parameterTypes.Select(p => ResolveTypeDefinition(p).FullName);
                diagnostics.ReportError(ErrorMessage.MissingRequiredMethod, TextLocation.Undefined, $"{typeDef.FullName}.{name}({string.Join(", ", parameterTypeNames)})");
                return null;
            }
            else
            {
                var parameterTypeNames = parameterTypes.Select(p => ResolveTypeDefinition(p).FullName);
                var methodDecl = $"{typeDef.FullName}.{name}({string.Join(", ", parameterTypeNames)})";
                var names = foundMethods.Select(t => t.Module.Assembly.Name.Name);
                diagnostics.ReportError(ErrorMessage.AmbiguousRequiredMethod, TextLocation.Undefined, methodDecl, string.Join(", ", names));
                return null;
            }
        }
    }
}