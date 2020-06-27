using System.Threading;
using System.Collections.Immutable;
using Compiler.Syntax;
using Compiler.Text;
using Compiler.Binding;
using System.Collections.Generic;
using Compiler.Diagnostics;
using System.IO;

namespace Compiler
{

    public sealed class Compilation
    {
        private Compilation(Compilation previous, SourceText text, Dictionary<string, VariableSymbol> env)
        {
            Previous = previous;
            Text = text;
            Variables = env;
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
        public Dictionary<string, VariableSymbol> Variables { get; }
        public ImmutableArray<Diagnostic> Diagnostics { get; }

        public void Evaluate()
        {
            if (Diagnostics.Length > 0 || Root == null) return;

            
            var statement = GetStatement();
            var evaluator = new Evaluator(statement, Variables);
            evaluator.Evaluate();
        }

        public object EvaluateExpression()
        {
           if (Diagnostics.Length > 0 || Root == null) return null;


            var statement = GetStatement();
            var evaluator = new Evaluator(statement, Variables);
            evaluator.Evaluate();
            return evaluator.lastValue;
        }

        private BoundBlockStatement GetStatement()
        {
            var stmt = Root.Statement;
            return Lowering.Lowerer.Lower(stmt);
        }



        public Compilation ContinueWith(SourceText text) => new Compilation(this, text, Variables);

        public static Compilation Compile(SourceText text) => new Compilation(null, text, new Dictionary<string, VariableSymbol>());

        public static ImmutableArray<SyntaxToken> Tokenize(SourceText text)
        {
            var lexer = new Lexer(text);
            return lexer.Tokenize().ToImmutableArray();
        }

        public static string SyntaxTreeToString(SourceText text)
        {
            var lexer = new Lexer(text);
            var tokens = lexer.Tokenize().ToImmutableArray();
            var parser = new Parser(text, tokens);
            var root = parser.ParseCompilationUnit();
            return root.ToString();
        }
    }
}
