using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Compiler.Binding;
using Compiler.Diagnostics;
using Compiler.Syntax;
using Compiler.Text;
using static System.Math;

namespace Compiler
{
    public class Evaluator
    {
        private SourceText Text;
        private BoundNode Root { get; }
        private DiagnosticBag Diagnostics { get; }
        private Dictionary<string, (TypeSymbol type, dynamic value)> Environment { get; }

        private Evaluator(SourceText src, BoundNode root, DiagnosticBag diagnostics, Dictionary<string, (TypeSymbol type, dynamic value)> environement)
        {
            Text = src;
            Root = root;
            Diagnostics = diagnostics;
            Environment = environement;
        }

        private dynamic EvaluateExpression(BoundExpression expr)
        {
            if (Diagnostics.Count > 0) return null;

            if (expr is BoundLiteralExpression le) return le.Value;
            else if (expr is BoundVariableExpression ve)
            {
                if (!Environment.TryGetValue(ve.Identifier, out (TypeSymbol Type, dynamic Value) value))
                {
                    Diagnostics.ReportVariableNotDefined(ve);
                    return null;
                }

                return value.Value;
            }
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
            else if (expr is BoundAssignementExpression ae)
            {
                var value = Environment[ae.Identifier];
                value.value = EvaluateExpression(ae.Expression);
                Environment[ae.Identifier] = value;
                return value.value;
            }
            else if (expr is BoundInvalidExpression) return null;
            else throw new Exception("Unknown Expression");
        }

        public dynamic Evaluate() => EvaluateExpression((BoundExpression)Root);

        public static Evaluator ConstructEvaluator(SourceText text, Dictionary<string, (TypeSymbol type, dynamic value)> env, DiagnosticBag bag)
        {
            var parser = new Parser(text, bag);
            var binder = new Binder(bag, env);
            var syntaxExpr = parser.Parse();
            var root = binder.BindExpression((ExpressionSyntax)syntaxExpr);
            var evaluator = new Evaluator(text, root, bag, env);
            return evaluator;
        }

        public static IEnumerable<SyntaxToken> Tokenize(string text, out DiagnosticBag bag)
        {
            bag = new DiagnosticBag();
            var lexer = new Lexer(new SourceText(text), bag);
            return lexer.Tokenize();
        }

        public static string GetExpressionAsString(string text, out DiagnosticBag bag)
        {
            bag = new DiagnosticBag();
            var parser = new Parser(new SourceText(text), bag);
            return parser.Parse().ToString();
        }

    }
}