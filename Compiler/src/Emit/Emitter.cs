using System;
using System.Linq;
using System.Collections.Generic;
using Mono.Cecil;
using Compiler.Binding;
using Compiler.Diagnostics;
using Compiler.Symbols;
using Compiler.Text;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

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
        private readonly Dictionary<TypeSymbol, MethodReference> toStringReferences;

        private readonly TypeReference consoleType;
        private readonly MethodReference consoleWriteLineReference;
        private readonly MethodReference cosnoleReadLineReference;
        private readonly MethodReference cosnoleClearReference;
        private readonly MethodReference stringConcatReference;
        private readonly MethodReference mathPowReference;

        private readonly Dictionary<GlobalVariableSymbol, FieldDefinition> globalVariables;
        private readonly Dictionary<LocalVariableSymbol, VariableDefinition> locals;

        public IEnumerable<Diagnostic> GetDiagnostics() => diagnostics;

        public Emiter(BoundProgram program, string moduleName, string[] referencePaths)
        {
            this.program = program;
            this.diagnostics = new DiagnosticBag();
            this.functions = new Dictionary<FunctionSymbol, MethodDefinition>();
            this.builtInTypes = new Dictionary<TypeSymbol, TypeReference>();
            this.globalVariables = new Dictionary<GlobalVariableSymbol, FieldDefinition>();
            this.locals = new Dictionary<LocalVariableSymbol, VariableDefinition>();
            this.toStringReferences = new Dictionary<TypeSymbol, MethodReference>();

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
            builtInTypes.Add(TypeSymbol.Int, ResolveType("System.Int64"));
            builtInTypes.Add(TypeSymbol.Float, ResolveType("System.Double"));
            builtInTypes.Add(TypeSymbol.Bool, ResolveType("System.Boolean"));
            builtInTypes.Add(TypeSymbol.String, ResolveType("System.String"));
            builtInTypes.Add(TypeSymbol.Void, ResolveType("System.Void"));
            mainClass = new TypeDefinition("", "Program", TypeAttributes.Abstract | TypeAttributes.Sealed | TypeAttributes.Public, builtInTypes[TypeSymbol.Any]);
            consoleType = ResolveType("System.Console");
            if (consoleType == null)
                return;
            consoleWriteLineReference = ResolveMethod("System.Console", "WriteLine", "System.Void", "System.Object");
            cosnoleReadLineReference = ResolveMethod("System.Console", "ReadLine", "System.String");
            cosnoleClearReference = ResolveMethod("System.Console", "Clear", "System.Void");
            stringConcatReference = ResolveMethod("System.String", "Concat", "System.String", "System.String", "System.String");
            mathPowReference = ResolveMethod("System.Math", "Pow", "System.Double", "System.Double", "System.Double");

            toStringReferences.Add(TypeSymbol.Any, ResolveMethod("System.Object", "ToString", "System.String"));
            toStringReferences.Add(TypeSymbol.Int, ResolveMethod("System.Int64", "ToString", "System.String"));
            toStringReferences.Add(TypeSymbol.Float, ResolveMethod("System.Double", "ToString", "System.String"));
            toStringReferences.Add(TypeSymbol.Bool, ResolveMethod("System.Boolean", "ToString", "System.String"));
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
            const FieldAttributes attrs = FieldAttributes.Static | FieldAttributes.Private;
            var type = builtInTypes[variable.Type];
            var field = new FieldDefinition(variable.Name, attrs, type);
            globalVariables.Add(variable, field);
            mainClass.Fields.Add(field);
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
            var ilProcesser = function.Body.GetILProcessor();

            foreach (var statement in body.Statements)
                EmitStatement(ilProcesser, statement);
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
                default: throw new Exception("Unexpected kind");
            }
        }

        private void EmitVariableDeclarationStatement(ILProcessor ilProcesser, BoundVariableDeclarationStatement node)
        {
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

        private void EmitConditionalGotoStatement(ILProcessor ilProcesser, BoundConditionalGotoStatement node)
        {
            throw new NotImplementedException();
        }

        private void EmitGotoStatement(ILProcessor ilProcesser, BoundGotoStatement node)
        {
            throw new NotImplementedException();
        }

        private void EmitLabelStatement(ILProcessor ilProcesser, BoundLabelStatement node)
        {
            throw new NotImplementedException();
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
            switch (node.Kind)
            {
                case BoundNodeKind.BoundLiteralExpression:
                    EmitLiteralExpression(ilProcesser, (BoundLiteralExpression)node);
                    break;
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

        private void EmitLiteralExpression(ILProcessor ilProcesser, BoundLiteralExpression node)
        {

            if (node.ResultType == TypeSymbol.Bool)
            {
                var val = (bool)node.Value;
                var opCode = val ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0;
                ilProcesser.Emit(opCode);
            }
            else if (node.ResultType == TypeSymbol.Int)
            {
                var val = (long)node.Value;
                ilProcesser.Emit(OpCodes.Ldc_I8, val);
            }
            else if (node.ResultType == TypeSymbol.Float)
            {
                var val = (double)node.Value;
                ilProcesser.Emit(OpCodes.Ldc_R8, val);
            }
            else if (node.ResultType == TypeSymbol.String)
            {
                var val = (string)node.Value;
                ilProcesser.Emit(OpCodes.Ldstr, val);
            }
            else throw new Exception("Unexpected literal type");
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
            throw new NotImplementedException();
        }

        private void EmitBinaryExpression(ILProcessor ilProcesser, BoundBinaryExpression node)
        {
            var leftType = node.Left.ResultType;
            var rightType = node.Right.ResultType;

            EmitExpression(ilProcesser, node.Left);
            EmitExpression(ilProcesser, node.Right);

            switch (node.Op)
            {
                case BoundBinaryOperator.Addition:
                    if (leftType == TypeSymbol.String && rightType == TypeSymbol.String) ilProcesser.Emit(OpCodes.Call, stringConcatReference);
                    else ilProcesser.Emit(OpCodes.Add);
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

                default: throw new Exception("Unexpected binary operator");
            }
        }

        private void EmitCallExpression(ILProcessor ilProcesser, BoundCallExpression node)
        {
            foreach (var arg in node.Arguments)
                EmitExpression(ilProcesser, arg);

            if (node.Symbol == BuiltInFunctions.Print)
                ilProcesser.Emit(OpCodes.Call, consoleWriteLineReference);
            else if (node.Symbol == BuiltInFunctions.Input)
                ilProcesser.Emit(OpCodes.Call, cosnoleReadLineReference);
            else if (node.Symbol == BuiltInFunctions.Len)
                throw new NotImplementedException();
            else if (node.Symbol == BuiltInFunctions.Random)
                throw new NotImplementedException();
            else if (node.Symbol == BuiltInFunctions.RandomFloat)
                throw new NotImplementedException();
            else if (node.Symbol == BuiltInFunctions.Clear)
                ilProcesser.Emit(OpCodes.Call, cosnoleClearReference);
            else if (node.Symbol == BuiltInFunctions.Exit)
                throw new NotImplementedException();
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
                case ("int", "str"):
                case ("float", "str"):
                case ("bool", "str"):
                case ("any", "str"):
                    var local = new VariableDefinition(builtInTypes[TypeSymbol.String]);
                    ilProcesser.Body.Variables.Add(local);
                    ilProcesser.Emit(OpCodes.Stloc, local);
                    ilProcesser.Emit(OpCodes.Ldloca, local);
                    var toStringReference = toStringReferences[node.Expression.ResultType];
                    ilProcesser.Emit(OpCodes.Call, toStringReference);
                    break;
                case ("int", "any"):
                case ("float", "any"):
                case ("bool", "any"):
                    var type1 = builtInTypes[node.Expression.ResultType];
                    ilProcesser.Emit(OpCodes.Box, type1);
                    break;
                case ("str", "any"):
                    break;
                case ("any", "int"):
                case ("any", "float"):
                case ("any", "bool"):
                    var type2 = builtInTypes[node.Type];
                    ilProcesser.Emit(OpCodes.Unbox_Any, type2);
                    break;



                default: throw new Exception("Unexpected type");
            }
        }

        private void EmitAssignmentExpression(ILProcessor ilProcesser, BoundAssignmentExpression node)
        {
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