using System;
using System.Linq;
using System.Collections.Generic;
using Mono.Cecil;
using Compiler.Binding;
using Compiler.Diagnostics;
using Compiler.Symbols;
using Compiler.Text;
using Mono.Cecil.Cil;

// C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.7.2\mscorlib.dll

namespace Compiler.Emit
{
    internal sealed class Emiter : IDiagnostable
    {
        private readonly BoundProgram program;
        private readonly string outputPath;

        private readonly DiagnosticBag diagnostics;
        private readonly AssemblyDefinition mainAssebly;
        private readonly List<AssemblyDefinition> references;
        private readonly Dictionary<TypeSymbol, TypeReference> builtInTypes;
        private readonly TypeReference consoleType;
        private readonly MethodReference consoleWriteLine;
        private readonly MethodReference consoleReadLine;

        public IEnumerable<Diagnostic> GetDiagnostics() => diagnostics;

        public Emiter(BoundProgram program, string moduleName, string outputPath, string[] referencePaths)
        {
            this.program = program;
            this.outputPath = outputPath;
            this.diagnostics = new DiagnosticBag();

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


            builtInTypes = new Dictionary<TypeSymbol, TypeReference>();
            builtInTypes.Add(TypeSymbol.Any, ResolveType("System.Object"));
            builtInTypes.Add(TypeSymbol.Int, ResolveType("System.Int64"));
            builtInTypes.Add(TypeSymbol.Float, ResolveType("System.Double"));
            builtInTypes.Add(TypeSymbol.Bool, ResolveType("System.Boolean"));
            builtInTypes.Add(TypeSymbol.String, ResolveType("System.String"));
            builtInTypes.Add(TypeSymbol.Void, ResolveType("System.Void"));
            consoleType = ResolveType("System.Console");
            if (consoleType == null)
                return;
            consoleWriteLine = ResolveMethod("System.Console", "WriteLine", "System.Void", "System.String");
            consoleReadLine = ResolveMethod("System.Console", "ReadLine", "System.String");
        }

        public void Emit()
        {
            var voidType = builtInTypes[TypeSymbol.Void];
            var objectType = builtInTypes[TypeSymbol.Any];
            var programType = new TypeDefinition("", "Program", TypeAttributes.Abstract | TypeAttributes.Sealed | TypeAttributes.Public, objectType);
            var mainMethod = new MethodDefinition("Main", MethodAttributes.Static | MethodAttributes.Private, voidType);

            var ilProcesser = mainMethod.Body.GetILProcessor();
            ilProcesser.Emit(OpCodes.Ldstr, "gayy");
            ilProcesser.Emit(OpCodes.Call, consoleWriteLine);
            ilProcesser.Emit(OpCodes.Ret);

            programType.Methods.Add(mainMethod);
            mainAssebly.MainModule.Types.Add(programType);
            mainAssebly.EntryPoint = mainMethod;
            mainAssebly.Write(outputPath);
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