using System.Threading;
using System.Collections.Immutable;
using Compiler.Syntax;
using Compiler.Text;
using Compiler.Binding;
using System.Collections.Generic;
using Compiler.Diagnostics;
using System.IO;
using Compiler.Symbols;
using Compiler.Lowering;

namespace Compiler
{

    public sealed class Compilation
    {
        private Dictionary<string, object> variables;
        private bool isScript;

        private Compilation(Compilation previous, SourceText text, Dictionary<string, object> env, bool isScript)
        {
            Previous = previous;
            Text = text;
            variables = env;
            this.isScript = isScript;

            var lexer = new Lexer(text, isScript);
            var tokens = lexer.Tokenize().ToImmutableArray();

            var parser = new Parser(text, tokens, isScript);
            var unit = parser.ParseCompilationUnit();

            var binder = new Binder(previous, isScript);
            Root = binder.BindCompilationUnit(unit);

            var builder = ImmutableArray.CreateBuilder<Diagnostic>();
            builder.AddRange(lexer.GetDiagnostics());
            builder.AddRange(parser.GetDiagnostics());
            builder.AddRange(binder.GetDiagnostics());

            Diagnostics = builder.ToImmutable();
        }

        internal BoundCompilationUnit Root { get; }
        public SourceText Text { get; }
        public ImmutableArray<Diagnostic> Diagnostics { get; }
        public Compilation Previous { get; }

        public void Evaluate()
        {
            if (Root == null) return;

            var statement = GetStatement();
            var evaluator = new Evaluator(statement, variables);
            evaluator.Evaluate();
        }

        public object EvaluateExpression()
        {
           if (Root == null) return null;


            var statement = GetStatement();
            var evaluator = new Evaluator(statement, variables);
            evaluator.Evaluate();
            return evaluator.lastValue;
        }

        private BoundBlockStatement GetStatement()
        {
            var stmt = Root.Statement;
            return Lowerer.Lower(stmt);
        }

        public static Compilation Compile(SourceText text) => new Compilation(null, text, new Dictionary<string, object>(), false);

        public static Compilation CompileScript(SourceText text, Compilation previous = null) 
        {
            var env = previous == null ? new Dictionary<string, object>() : previous.variables;
            return new Compilation(previous, text, env, true);
        }

        public static ImmutableArray<SyntaxToken> Tokenize(SourceText text)
        {
            var lexer = new Lexer(text, true);
            return lexer.Tokenize().ToImmutableArray();
        }

        public static string SyntaxTreeToString(SourceText text)
        {
            var lexer = new Lexer(text, true);
            var tokens = lexer.Tokenize().ToImmutableArray();
            var parser = new Parser(text, tokens, true);
            var root = parser.ParseCompilationUnit();
            return root.ToString();
        }
    }
}
