using System.Collections.Immutable;
using Compiler.Syntax;
using Compiler.Text;
using Compiler.Binding;
using System.Collections.Generic;
using Compiler.Diagnostics;
using Compiler.Lowering;

namespace Compiler
{

    public sealed class Compilation
    {
        private readonly BoundProgram program;
        private readonly Compilation previous;

        private Dictionary<string, object> globals;
        private bool isScript;

        private Compilation(Compilation previous, SourceText text, Dictionary<string, object> globals, bool isScript)
        {
            this.globals = globals;
            this.isScript = isScript;
            this.Text = text;
            this.previous = previous;

            var lexer = new Lexer(text, isScript);
            var tokens = lexer.Tokenize().ToImmutableArray();

            var parser = new Parser(text, tokens, isScript);
            var unit = parser.ParseCompilationUnit();

            var previousProgram = previous == null ? null : previous.program;
            program = Binder.BindProgram(previousProgram, isScript, unit);

            var builder = ImmutableArray.CreateBuilder<Diagnostic>();
            builder.AddRange(lexer.GetDiagnostics());
            builder.AddRange(parser.GetDiagnostics());
            builder.AddRange(program.Diagnostics);

            Diagnostics = builder.ToImmutable();
        }

        public SourceText Text { get; }
        public ImmutableArray<Diagnostic> Diagnostics { get; }

        public void Evaluate()
        {
            if (Diagnostics.Length > 0) return;

            var statement = GetStatement();
            var evaluator = new Evaluator(statement, variables);
            evaluator.Evaluate();
        }

        public object EvaluateExpression()
        {
           if (Diagnostics.Length > 0) return null;


            var statement = GetStatement();
            var evaluator = new Evaluator(statement, variables);
            evaluator.Evaluate();
            return evaluator.lastValue;
        }

       
        public static Compilation Compile(SourceText text) => new Compilation(null, text, new Dictionary<string, object>(), false);

        public static Compilation CompileScript(SourceText text, Compilation previous = null) 
        {
            var env = previous == null ? new Dictionary<string, object>() : previous.globals;
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
