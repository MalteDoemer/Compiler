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
        private Compilation(Compilation previous, SourceText text, Dictionary<string, VariableSymbol> env)
        {
            Previous = previous;
            Text = text;
            Env = env;
            var lexer = new Lexer(text);
            var tokens = lexer.Tokenize().ToImmutableArray();
            var parser = new Parser(text, tokens);
            var unit = parser.ParseCompilationUnit();
            var binder = new Binder(previous);
            Root = binder.BindCompilationUnit(unit);

            var builder = ImmutableArray.CreateBuilder<Diagnostic>();
            builder.AddRange(lexer.GetDiagnostics());
            builder.AddRange(parser.GetDiagnostics());
            builder.AddRange(binder.GetDiagnostics());

            Diagnostics = builder.ToImmutable();
        }

        internal BoundCompilationUnit Root { get; }
        public Compilation Previous { get; }
        public SourceText Text { get; }
        public Dictionary<string, VariableSymbol> Env { get; }
        public ImmutableArray<Diagnostic> Diagnostics { get; }

        public dynamic Evaluate()
        {
            if (Diagnostics.Length > 0) return null;
            var evaluator = new Evaluator(Root.Statement, Env);
            evaluator.Evaluate();
            return evaluator.lastValue;
        }

        public Compilation ContinueWith(SourceText text) => new Compilation(this, text, Env);
        public Compilation ContinueWith(string text) => ContinueWith(new SourceText(text));
        public static Compilation Compile(string text) => Compile(new SourceText(text));
        public static Compilation Compile(SourceText text) => new Compilation(null, text, new Dictionary<string, VariableSymbol>());


    }
}
