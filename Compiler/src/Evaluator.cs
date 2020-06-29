using System;
using System.Collections.Generic;
using Compiler.Binding;
using Compiler.Symbols;

using static System.Math;

namespace Compiler
{

    internal class Evaluator
    {
        private readonly Dictionary<string, object> varaibles;
        private readonly BoundBlockStatement root;
        internal dynamic lastValue;
        private Random random;

        public Evaluator(BoundBlockStatement root, Dictionary<string, object> varaibles)
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
                    return varaibles[ve.Variable.Name];
                case BoundUnaryExpression ue:
                    return EvaluateUnaryExpression(ue);
                case BoundBinaryExpression be:
                    return EvaluateBinaryExpression(be);
                case BoundAssignementExpression ae:
                    return EvaluateAssignment(ae);
                case BoundCallExpression boundCall:
                    return EvaluateFunctionCall(boundCall);
                case BoundInvalidExpression _:
                    return null;
                default:
                    throw new Exception("Unknown Expression");
            }
        }

        private dynamic EvaluateAssignment(BoundAssignementExpression ae)
        {
            var val = EvaluateExpression(ae.Expression);
            varaibles[ae.Variable.Name] = val;
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

        private dynamic EvaluateFunctionCall(BoundCallExpression bc)
        {
            if (bc.Symbol == BuiltInFunctions.Input)
            {
                return Console.ReadLine();
            }
            else if (bc.Symbol == BuiltInFunctions.Print)
            {
                var message = EvaluateExpression(bc.Arguments[0]);
                Console.WriteLine(message);
                return null;
            }
            else if (bc.Symbol == BuiltInFunctions.Clear)
            {
                Console.Clear();
                return null;
            }
            else if (bc.Symbol == BuiltInFunctions.Random)
            {
                if (random == null)
                    random = new Random();
                var lowerBound = EvaluateExpression(bc.Arguments[0]);
                var upperBound = EvaluateExpression(bc.Arguments[1]);
                return random.Next(lowerBound, upperBound);
            }
            else if (bc.Symbol == BuiltInFunctions.RandomFloat)
            {
                if (random == null)
                    random = new Random();
                return random.NextDouble();
            }
            else throw new Exception($"Unexpected function <{bc.Symbol.Name}>");
        }

        private void EvaluateVariableDecleration(BoundVariableDecleration vs)
        {
            var val = EvaluateExpression(vs.Expression);
            lastValue = val;
            varaibles[vs.Variable.Name] = val;
        }
    }
}