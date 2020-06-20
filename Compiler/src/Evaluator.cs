using System;
using System.Collections.Generic;
using System.Linq;
using Compiler.Binding;
using Compiler.Diagnostics;
using Compiler.Syntax;
using static System.Math;

namespace Compiler
{
    public class Evaluator
    {
        private DiagnosticBag Diagnostics { get; }
        private BoundNode Root { get; }

        internal Evaluator(BoundNode root, DiagnosticBag diagnostics)
        {
            Root = root;
            Diagnostics = diagnostics;
        }

        internal dynamic EvaluateExpression() => EvaluateExpression((BoundExpression)Root);

        private dynamic EvaluateExpression(BoundExpression expr)
        {
            if (Diagnostics.Count > 0) return null;


            if (expr is BoundLiteralExpression le)
                return le.Value;
            //else if (expr is VariableExpressionSyntax ve)
            //{
            //    Diagnostics.ReportVariableNotDefined(ve);
            //    return null;
            //}
            else if (expr is BoundUnaryExpression ue)
            {
                dynamic val = EvaluateExpression(ue.Right);
                switch (ue.Op)
                {
                    case BoundUnaryOperator.Identety: return val;
                    case BoundUnaryOperator.Negation: return -val;
                    case BoundUnaryOperator.LogicalNot: return !val;
                    default:
                        Diagnostics.ReportUnknownUnaryOperator(ue);
                        return null;
                }
            }
            else if (expr is BoundBinaryExpression be)
            {

                var left = EvaluateExpression(be.Left);
                var right = EvaluateExpression(be.Right);

                switch (be.Op)
                {
                    case BoundBinaryOperator.Addition: return left + right;
                    case BoundBinaryOperator.Subtraction: return left - right;
                    case BoundBinaryOperator.Multiplication: return left * right;
                    case BoundBinaryOperator.Division: return left / right;
                    case BoundBinaryOperator.Power: return Pow(left, right);
                    case BoundBinaryOperator.Root: return Pow(left, 1.0d / right);

                    case BoundBinaryOperator.EqualEqual: return left == right;
                    case BoundBinaryOperator.NotEqual: return left != right;
                    case BoundBinaryOperator.LessThan: return left < right;
                    case BoundBinaryOperator.LessEqual: return left <= right;
                    case BoundBinaryOperator.GreaterThan: return left > right;
                    case BoundBinaryOperator.GreaterEqual: return left >= right;

                    case BoundBinaryOperator.LogicalAnd: return left && right;
                    case BoundBinaryOperator.LogicalOr: return left || right;

                    default:
                        Diagnostics.ReportUnknownBinaryOperator(be);
                        return null;
                }
            }
            else throw new Exception("Fett");
        }

        public static void Evaluate(string text, out DiagnosticBag bag)
        {
            bag = new DiagnosticBag();
            var parser = new Parser(text, bag);
            var binder = new Binder(bag);
            var syntaxExpr = parser.ParseExpression();

            //Console.WriteLine(syntaxExpr);

            var boundExpr = binder.BindExpression(syntaxExpr);
            var evaluator = new Evaluator(boundExpr, bag);
            var res = evaluator.EvaluateExpression();

            if (bag.Errors > 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                foreach (var err in bag.GetErrors()) Console.WriteLine('\n' + err.ToString() + '\n');
                Console.ResetColor();
                return;
            }
            Console.ForegroundColor = ConsoleColor.Green;

            if (res == null) Console.WriteLine("null");
            else Console.WriteLine(res);
            Console.ResetColor();
        }

        public static IEnumerable<SyntaxToken> Tokenize(string text, out DiagnosticBag bag)
        {
            bag = new DiagnosticBag();
            var lexer = new Lexer(text, bag);
            return lexer.Tokenize();
        }

    }
}