using System;
using System.Collections.Generic;
using Compiler.Binding;
using Compiler.Diagnostics;
using static System.Math;

namespace Compiler
{

    internal class Evaluator
    {
        internal dynamic lastValue;
        private readonly Dictionary<string, VariableSymbol> varaibles;
        private readonly BoundBlockStatement root;

        public Evaluator(BoundBlockStatement root, Dictionary<string, VariableSymbol> varaibles)
        {
            this.root = root;
            this.varaibles = varaibles;
        }

        public void Evaluate()
        {
            var labelToIndex = new Dictionary<BoundLabel, int>();

            for (int i = 0; i < root.Statements.Length; i++)
                if (root.Statements[i] is BoundLabelStatement label)
                    labelToIndex.Add(label.Label, i);

            var index = 0;

            while (index < root.Statements.Length)
            {
                var stmt = root.Statements[index];

                switch (stmt)
                {
                    case BoundExpressionStatement es:
                        lastValue = EvaluateExpression(es.Expression);
                        index++;
                        break;
                    case BoundVariableDecleration vs:
                        EvaluateVariableDecleration(vs);
                        index++;
                        break;
                    case BoundConditionalGotoStatement cgs:
                        bool condition = EvaluateExpression(cgs.Condition);
                        if ((condition && !cgs.JumpIfFalse) || (!condition && cgs.JumpIfFalse))
                            index = labelToIndex[cgs.Label];
                        else index++;
                        break;
                    case BoundGotoStatement gs:
                        index = labelToIndex[gs.Label];
                        break;
                    case BoundPrintStatement ps:
                        EvaluatePrintStatement(ps);
                        index++;
                        break;
                    case BoundLabelStatement _:
                        index++;
                        break;
                    default: throw new Exception($"Unexpected Statement <{stmt}>");
                }
            }
        }

        private dynamic EvaluateExpression(BoundExpression expr)
        {
            switch (expr)
            {
                case BoundLiteralExpression le:
                    return le.Value;
                case BoundVariableExpression ve:
                    return varaibles[ve.Variable.Identifier].Value;
                case BoundUnaryExpression ue:
                    return EvaluateUnaryExpression(ue);
                case BoundBinaryExpression be:
                    return EvaluateBinaryExpression(be);
                case BoundAssignementExpression ae:
                    return EvaluateAssignment(ae);
                case BoundInvalidExpression _:
                    return null;
                default:
                    throw new Exception("Unknown Expression");
            }
        }

        private dynamic EvaluateAssignment(BoundAssignementExpression ae)
        {
            var val = EvaluateExpression(ae.Expression);
            var variable = new VariableSymbol(ae.Variable.Identifier, ae.Variable.Type, val);
            varaibles[variable.Identifier] = variable;
            return val;
        }

        private dynamic EvaluateBinaryExpression(BoundBinaryExpression be)
        {
            var left = EvaluateExpression(be.Left);
            var right = EvaluateExpression(be.Right);

            switch (be.Op)
            {
                case BoundBinaryOperator.Addition: return left + right;
                case BoundBinaryOperator.Subtraction: return left - right;
                case BoundBinaryOperator.Multiplication: return left * right;
                case BoundBinaryOperator.Division: return left / right;
                case BoundBinaryOperator.Modulo: return left % right;
                case BoundBinaryOperator.Power: return Pow(left, right);
                case BoundBinaryOperator.Root: return Pow(left, 1.0d / right);

                case BoundBinaryOperator.BitwiseAnd: return left & right;
                case BoundBinaryOperator.BitwiseOr: return left | right;
                case BoundBinaryOperator.BitwiseXor: return left ^ right;

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

        private dynamic EvaluateUnaryExpression(BoundUnaryExpression ue)
        {
            dynamic val = EvaluateExpression(ue.Right);
            switch (ue.Op)
            {
                case BoundUnaryOperator.Identety: return val;
                case BoundUnaryOperator.Negation: return -val;
                case BoundUnaryOperator.LogicalNot: return !val;
                case BoundUnaryOperator.BitwiseNot: return ~val;
                default:
                    throw new Exception($"Unknown Unary Operator <{ue.Op}>");
            }
        }

        private void EvaluateVariableDecleration(BoundVariableDecleration vs)
        {
            var val = EvaluateExpression(vs.Expression);
            lastValue = val;
            var variable = new VariableSymbol(vs.Variable.Identifier, vs.Variable.Type, val);
            varaibles[variable.Identifier] = variable;
        }

        private void EvaluatePrintStatement(BoundPrintStatement ps)
        {
            Console.WriteLine(EvaluateExpression(ps.Expression));
        }
    }
}