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
        private readonly AssemblyDefinition[] references;
        private readonly Dictionary<TypeSymbol, TypeReference> builtInTypes;
        private readonly TypeReference consoleType;
        private readonly MethodReference consoleWriteLine;

        public IEnumerable<Diagnostic> GetDiagnostics() => diagnostics;

        public Emiter(BoundProgram program, string moduleName, string outputPath, string[] referencePaths)
        {
            this.program = program;
            this.outputPath = outputPath;
            this.diagnostics = new DiagnosticBag();

            mainAssebly = CreateMainAssembly(moduleName);
            references = ResolveReferences(referencePaths);
            builtInTypes = ResolveBuiltIns();
            consoleType = ResolveType("System.Console");
            consoleWriteLine = ResolveMethod(consoleType, "WriteLine", builtInTypes[TypeSymbol.Any]);
        }

        public void Emit()
        {
            if (HasErrors())
                return;

            var voidType = builtInTypes[TypeSymbol.Void];
            var objectType = builtInTypes[TypeSymbol.Any];
            var programType = new TypeDefinition("", "Program", TypeAttributes.Abstract | TypeAttributes.Sealed | TypeAttributes.Public, objectType);
            var mainMethod = new MethodDefinition("Main", MethodAttributes.Static | MethodAttributes.Private, voidType);

            var ilProcesser = mainMethod.Body.GetILProcessor();

            ilProcesser.Emit(OpCodes.Ret);

            programType.Methods.Add(mainMethod);
            mainAssebly.MainModule.Types.Add(programType);
            mainAssebly.EntryPoint = mainMethod;
            mainAssebly.Write(outputPath);
        }

        private Dictionary<TypeSymbol, TypeReference> ResolveBuiltIns()
        {
            var builtInTypes = new List<(TypeSymbol Symbol, string MetadataName)>(){
                (TypeSymbol.Any,    "System.Object"),
                (TypeSymbol.Int,    "System.Int64"),
                (TypeSymbol.Float,  "System.Double"),
                (TypeSymbol.Bool,   "System.Boolean"),
                (TypeSymbol.String, "System.String"),
                (TypeSymbol.Void,   "System.Void"),
            };
            var knownTypes = new Dictionary<TypeSymbol, TypeReference>();

            foreach (var (symbol, metadataName) in builtInTypes)
                knownTypes.Add(symbol, ResolveType(metadataName));

            return knownTypes;
        }

        private TypeReference ResolveType(string metadataName)
        {
            var foundTypes = references.SelectMany(a => a.Modules)
                                           .SelectMany(m => m.Types)
                                           .Where(t => t.FullName == metadataName)
                                           .ToArray();

            if (foundTypes.Length == 1)
            {
                var definition = foundTypes.Single();
                var typeReference = mainAssebly.MainModule.ImportReference(definition);
                return typeReference;
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

        private MethodReference ResolveMethod(TypeReference type, string name,params TypeReference[] parameterTypes)
        {
            var def = type.Resolve();
            return null;
        }

        private AssemblyDefinition CreateMainAssembly(string moduleName)
        {
            var assembylInfo = new AssemblyNameDefinition(moduleName, new Version(1, 0));
            return AssemblyDefinition.CreateAssembly(assembylInfo, moduleName, ModuleKind.Console);
        }

        private AssemblyDefinition[] ResolveReferences(string[] referencePaths)
        {
            var result = new List<AssemblyDefinition>();

            foreach (var reference in referencePaths)
            {
                try
                {
                    var assembly = AssemblyDefinition.ReadAssembly(reference);
                    result.Add(assembly);
                }
                catch (BadImageFormatException)
                {
                    diagnostics.ReportError(ErrorMessage.InvalidReference, TextLocation.Undefined, reference);
                }
            }
            return result.ToArray();
        }

        private bool HasErrors() => diagnostics.Where(d => d.Level == ErrorLevel.Error).Any();
    }
}