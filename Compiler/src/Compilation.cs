using System.Threading;
using System.Collections.Immutable;
using Compiler.Syntax;
using Compiler.Text;
using Compiler.Binding;
using System.Collections.Generic;
using Compiler.Diagnostics;

namespace Compiler
{

    public sealed class Compilation
    {
        private readonly BoundCompilationUnit root;

        private Compilation(SourceText text)
        {
            var lexer = new Lexer(text);
            var tokens = lexer.Tokenize().ToImmutableArray();
            var parser = new Parser(text, tokens);
            var unit = parser.ParseCompilationUnit();
            var binder = new Binder();
            root = binder.BindCompilationUnit(unit);

            var builder = ImmutableArray.CreateBuilder<Diagnostic>();
            builder.AddRange(lexer.GetDiagnostics());
            builder.AddRange(parser.GetDiagnostics());
            builder.AddRange(binder.GetDiagnostics());

            Diagnostics = builder.ToImmutable();
        }

        public ImmutableArray<Diagnostic> Diagnostics { get; }

        public dynamic Evaluate()
        {
            var env = new Dictionary<string, VariableSymbol>();
            var evaluator = new Evaluator(root.Statement, env);
            evaluator.Evaluate();
            return evaluator.lastValue;
        }

        public static Compilation Compile(string text) => Compile(new SourceText(text));

        public static Compilation Compile(SourceText text)
        {
            return new Compilation(text);
        }

    }

    // public sealed class Compilation
    // {
    //     private BoundGlobalScope globalScope;
    //     public Compilation Previous { get; }

    //     private Compilation(Compilation previous, SourceText text)
    //     {
    //         Previous = previous;
    //     }


    //     // internal BoundGlobalScope GlobalScope
    //     // {
    //     //     get
    //     //     {
    //     //         if (globalScope == null)
    //     //         {
    //     //             var scope = Binder.BindGlobalScope(Previous?.GlobalScope, Tree.Root);
    //     //             Interlocked.CompareExchange(ref globalScope, scope, null); // Dammm son
    //     //         }
    //     //         return globalScope;
    //     //     }
    //     // }


    //     public static Compilation Compile(SourceText text)
    //     {

    //     }
    // }
}
