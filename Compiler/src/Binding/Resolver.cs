using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Compiler.Diagnostics;
using Compiler.Symbols;
using Compiler.Text;
using Mono.Cecil;

namespace Compiler.Binding
{
    internal sealed class BoundTypeResolver : IDiagnostable
    {
        private readonly DiagnosticBag diagnostics;

        public ImmutableArray<AssemblyDefinition> References { get; }
        public Dictionary<TypeSymbol, TypeReference> Types { get; }
        public AssemblyDefinition MainAssembly { get; }

        public IEnumerable<Diagnostic> GetDiagnostics() => diagnostics;

        public BoundTypeResolver(string moduleName, string[] referencePaths)
        {
            Types = new Dictionary<TypeSymbol, TypeReference>();
            diagnostics = new DiagnosticBag();

            var assembylInfo = new AssemblyNameDefinition(moduleName, new Version(1, 0));
            MainAssembly = AssemblyDefinition.CreateAssembly(assembylInfo, moduleName, ModuleKind.Console);

            var refBuilder = ImmutableArray.CreateBuilder<AssemblyDefinition>();
            foreach (var reference in referencePaths)
            {
                try
                {
                    var assembly = AssemblyDefinition.ReadAssembly(reference);
                    refBuilder.Add(assembly);
                }
                catch (BadImageFormatException)
                {
                    diagnostics.ReportError(ErrorMessage.InvalidReference, TextLocation.Undefined, reference);
                }
            }
            References = refBuilder.ToImmutable();
        }

        public bool ResolveType(TypeSymbol symbol)
        {
            if (Types.ContainsKey(symbol))
                return true;

            var type = ResolveTypeReference(symbol);

            if (type is null)
                return false;

            Types.Add(symbol, type);
            return true;
        }

        private TypeReference? ResolveTypeReference(TypeSymbol symbol)
        {
            if (symbol is ArrayTypeSymbol array)
            {
                var baseType = ResolveTypeReference(array.BaseType);
                return new ArrayType(baseType, array.Rank);
            }
            else if (symbol is PrimitiveTypeSymbol primitive)
            {
                switch (primitive.Name)
                {
                    case "int":
                        return ResolveTypeReference("System.Int32");
                    case "float":
                        return ResolveTypeReference("System.Double");
                    case "bool":
                        return ResolveTypeReference("System.Boolean");
                    case "str":
                        return ResolveTypeReference("System.String");
                    case "obj":
                        return ResolveTypeReference("System.Object");
                    case "void":
                        return ResolveTypeReference("System.Void");
                    default: return null;
                }
            }
            else return null;
        }

        private TypeReference? ResolveTypeReference(string metadataName)
        {
            var def = ResolveTypeDefinition(metadataName);
            if (def is null)
                return null;

            return MainAssembly.MainModule.ImportReference(def);
        }

        private TypeDefinition? ResolveTypeDefinition(string metadataName)
        {
            return References.SelectMany(a => a.Modules)
                             .SelectMany(m => m.Types)
                             .Where(t => t.FullName == metadataName)
                             .SingleOrDefault();
        }
    }
}
