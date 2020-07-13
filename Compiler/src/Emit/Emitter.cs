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
            var voidType = builtInTypes[TypeSymbol.Void];
            var objectType = builtInTypes[TypeSymbol.Any];



            var mainMethod = new MethodDefinition("Main", MethodAttributes.Static | MethodAttributes.Private, voidType);


            var ilProcesser = mainMethod.Body.GetILProcessor();
            ilProcesser.Emit(OpCodes.Ldstr, "gayy");
            ilProcesser.Emit(OpCodes.Call, consoleWriteLine);
            ilProcesser.Emit(OpCodes.Ret);

            mainClass.Methods.Add(mainMethod);
            mainAssebly.MainModule.Types.Add(mainClass);
            mainAssebly.EntryPoint = mainMethod;
            mainAssebly.Write(outputPath);
        }

        private void EmitFunctionDecleration(FunctionSymbol symbol)
        {
            const MethodAttributes attrs = MethodAttributes.Static | MethodAttributes.Private;
            var returnType = builtInTypes[symbol.ReturnType];
            var function = new MethodDefinition(symbol.Name, attrs, returnType);
            functions.Add(symbol, function);
        }

        private void EmitFunctionBody(FunctionSymbol symbol)
        {
            var function = functions[symbol];
            var body = program.GetFunctionBody(symbol);
            var ilProcesser = function.Body.GetILProcessor();

            foreach (var statement in body.Statements)
                EmitStatement(ilProcesser, statement);
        }

        private void EmitStatement(ILProcessor ilProcesser, BoundStatement statement)
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