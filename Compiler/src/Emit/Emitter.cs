using System;
using System.Linq;
using System.Collections.Generic;
using Mono.Cecil;
using Compiler.Binding;
using Compiler.Diagnostics;
using Compiler.Symbols;
using Compiler.Text;
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
        private readonly TypeReference consoleType;
        private readonly MethodReference consoleWriteLine;
        private readonly MethodReference consoleReadLine;

        public IEnumerable<Diagnostic> GetDiagnostics() => diagnostics;

        public Emiter(BoundProgram program, string moduleName, string[] referencePaths)
        {
            this.program = program;
            this.diagnostics = new DiagnosticBag();
            this.functions = new Dictionary<FunctionSymbol, MethodDefinition>();
            this.builtInTypes = new Dictionary<TypeSymbol, TypeReference>();

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
            consoleWriteLine = ResolveMethod("System.Console", "WriteLine", "System.Void", "System.String");
            consoleReadLine = ResolveMethod("System.Console", "ReadLine", "System.String");
        }

        public void Emit(string outputPath)
        {
            foreach (var func in program.Functions.Keys)
                EmitFunctionDecleration(func);

            foreach (var func in program.Functions)
                EmitFunctionBody(func.Key, func.Value);

            mainAssebly.MainModule.Types.Add(mainClass);
            mainAssebly.EntryPoint = functions[program.MainFunction];
            mainAssebly.Write(outputPath);
        }

        private void EmitFunctionDecleration(FunctionSymbol symbol)
        {
            const MethodAttributes attrs = MethodAttributes.Static | MethodAttributes.Private;
            var returnType = builtInTypes[symbol.ReturnType];
            var function = new MethodDefinition(symbol.Name, attrs, returnType);
            functions.Add(symbol, function);
            mainClass.Methods.Add(function);
        }

        private void EmitFunctionBody(FunctionSymbol symbol, BoundBlockStatement body)
        {
            var function = functions[symbol];
            var ilProcesser = function.Body.GetILProcessor();

            foreach (var statement in body.Statements)
                EmitStatement(ilProcesser, statement);

            ilProcesser.Emit(OpCodes.Ret);
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
            throw new NotImplementedException();
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
            throw new NotImplementedException();
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
            else if (node.ResultType == TypeSymbol.String)
            {
                var val = (string)node.Value;
                ilProcesser.Emit(OpCodes.Ldstr, val);
            }
            else throw new Exception("Unexpected literal type");
        }

        private void EmitVariableExpression(ILProcessor ilProcesser, BoundVariableExpression node)
        {
            throw new NotImplementedException();
        }

        private void EmitUnaryExpression(ILProcessor ilProcesser, BoundUnaryExpression node)
        {
            throw new NotImplementedException();
        }

        private void EmitBinaryExpression(ILProcessor ilProcesser, BoundBinaryExpression node)
        {
            throw new NotImplementedException();
        }

        private void EmitCallExpression(ILProcessor ilProcesser, BoundCallExpression node)
        {
            foreach (var arg in node.Arguments)
                EmitExpression(ilProcesser, arg);

            if (node.Symbol == BuiltInFunctions.Print)
                ilProcesser.Emit(OpCodes.Call, consoleWriteLine);
            else if (node.Symbol == BuiltInFunctions.Input)
                ilProcesser.Emit(OpCodes.Call, consoleReadLine);
            else if (node.Symbol == BuiltInFunctions.Len)
                throw new NotImplementedException();
            else if (node.Symbol == BuiltInFunctions.Random)
                throw new NotImplementedException();
            else if (node.Symbol == BuiltInFunctions.RandomFloat)
                throw new NotImplementedException();
            else if (node.Symbol == BuiltInFunctions.Clear)
                throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        private void EmitAssignmentExpression(ILProcessor ilProcesser, BoundAssignmentExpression node)
        {
            throw new NotImplementedException();
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