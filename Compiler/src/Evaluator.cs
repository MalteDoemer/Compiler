using System;
using Compiler.Diagnostics;
using Compiler.Syntax;

namespace Compiler
{
    public class Evaluator
    {
        private DiagnosticBag Diagnostics { get; }
        private SyntaxNode Root { get; }

        internal Evaluator(SyntaxNode root, DiagnosticBag diagnostics)
        {
            Root = root;
            Diagnostics = diagnostics;
        }

        internal dynamic EvaluateExpression() => EvaluateExpression((ExpressionSyntax)Root);

        private dynamic EvaluateExpression(ExpressionSyntax expr)
        {
            if (Diagnostics.Count > 0) return null;


            if (expr is LiteralExpressionSyntax le)
                return le.Literal.value;
            else if (expr is VariableExpressionSyntax ve)
            {
                Diagnostics.ReportVariableNotDefined(ve);
                return null;
            }
            else if (expr is UnaryExpressionSyntax ue)
            {
                dynamic val = EvaluateExpression(ue.Expression);
                switch (ue.Op.kind)
                {
                    case SyntaxTokenKind.Plus: return val;
                    case SyntaxTokenKind.Minus: return -val;
                    default:
                        Diagnostics.ReportUnknownUnaryOperator(ue);
                        return null;
                }
            }
            else if (expr is BinaryExpressionSyntax be)
            {

                var left = EvaluateExpression(be.Left);
                var right = EvaluateExpression(be.Right);

                switch (be.Op.kind)
                {
                    case SyntaxTokenKind.Plus: return left + right;
                    case SyntaxTokenKind.Minus: return left - right;
                    case SyntaxTokenKind.Star: return left * right;
                    case SyntaxTokenKind.Slash: return left / right;
                    default:
                        Diagnostics.ReportUnknownBinaryOperator(be);
                        return null;
                }
            }
            else throw new Exception("Fett");
        }

        public static void Evaluate(string text)
        {
            var bag = new DiagnosticBag();
            var parser = new Parser(text, bag);
            var root = parser.ParseExpression();
            var evaluator = new Evaluator(root, bag);
            var res = evaluator.EvaluateExpression();

            if (bag.Errors > 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                foreach (var err in bag.GetErrors()) Console.WriteLine('\n' + err.ToString() + '\n');
                Console.ResetColor();
                return;
            }
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(res);
            Console.ResetColor();
        }
    }
}