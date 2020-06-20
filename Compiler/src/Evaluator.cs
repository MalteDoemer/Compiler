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
                        throw new Exception($"Unknown Unary Operator <{ue.Op}>");
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
                        throw new Exception($"Unknown binary operator <{be.Op}>");
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
                foreach (var err in bag.GetErrors())
                {
                    if (err.HasPositon)
                    {
                        var errText = text.Substring(err.Span.Start, err.Span.Lenght);
                        var prefix = text.Substring(0, err.Span.Start);
                        var postfix = text.Substring(err.Span.End, text.Length - err.Span.End);

                        Console.WriteLine('\n');

                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write(err.Kind);
                        Console.Write(" at position ");
                        Console.Write(err.Span.Start);
                        Console.Write(":");
                        Console.WriteLine('\n');

                        Console.ResetColor();
                        Console.Write("\t");
                        Console.Write(prefix);

                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write(errText);
                        
                        Console.ResetColor();
                        Console.Write(postfix);

                        Console.WriteLine('\n');
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(err.Message);
                        Console.WriteLine('\n');

                    }
                    else 
                    {

                    }
                }
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

        public static string GetExpressionAsString(string text, out DiagnosticBag bag)
        {
            bag = new DiagnosticBag();
            var parser = new Parser(text, bag);
            return parser.ParseExpression().ToString();
        }

    }
}