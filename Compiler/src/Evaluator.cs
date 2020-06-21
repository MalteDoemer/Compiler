using System;
using System.Collections.Generic;
using Compiler.Binding;
using Compiler.Diagnostics;
using Compiler.Syntax;
using static System.Math;

namespace Compiler
{
    internal class Evaluator
    {

        private CompilationUnit Root { get; }
        private DiagnosticBag Diagnostics { get; }
        private Dictionary<string, (TypeSymbol type, dynamic value)> Environment { get; }

        public Evaluator(CompilationUnit root, DiagnosticBag diagnostics, Dictionary<string, (TypeSymbol type, dynamic value)> environment)
        {
            Root = root;
            Diagnostics = diagnostics;
            Environment = environment;
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


        public dynamic Evaluate()
        {
            var binder= new Binder(Diagnostics, Environment);
            var expr = binder.BindExpression((ExpressionSyntax)Root.Nodes[0]);
            return EvaluateExpression(expr);
        }
    }
}