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

        public Evaluator(BoundStatement root, Dictionary<string, VariableSymbol> varaibles)
        {
            Root = root;
            Varaibles = varaibles;
        }

        private Dictionary<string, VariableSymbol> Varaibles { get; }
        private BoundStatement Root { get; }

        private dynamic EvaluateExpression(BoundExpression expr)
        {
            if (expr is BoundLiteralExpression le) return le.Value;
            else if (expr is BoundVariableExpression ve)
            {
                return Varaibles[ve.Variable.Identifier].Value;
            }
            else if (expr is BoundUnaryExpression ue)
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
            else if (expr is BoundAssignementExpression ae)
            {
                var val = EvaluateExpression(ae.Expression);
                var variable = new VariableSymbol(ae.Variable.Identifier, ae.Variable.Type, val);
                Varaibles[variable.Identifier] = variable;
                return val;
            }
            else if (expr is BoundInvalidExpression) return null;
            else throw new Exception("Unknown Expression");
        }

        private void EvaluateStatement(BoundStatement stmt)
        {
            if (stmt is BoundExpressionStatement es)
            {
                lastValue = EvaluateExpression(es.Expression);
                return;
            }
            else if (stmt is BoundBlockStatement bs)
            {
                foreach (var s in bs.Statements)
                    EvaluateStatement(s);
                return;
            }
            else if (stmt is BoundIfStatement ifs)
            {
                bool condition = EvaluateExpression(ifs.Condition);
                if (condition)
                    EvaluateStatement(ifs.ThenStatement);
                else if (ifs.ElseStatement != null) EvaluateStatement(ifs.ElseStatement);
            }
            else if (stmt is BoundVariableDeclerationStatement vs)
            {
                var val = EvaluateExpression(vs.Expression);
                lastValue = val;
                var variable = new VariableSymbol(vs.Variable.Identifier, vs.Variable.Type, val);
                Varaibles[variable.Identifier] = variable;
            }
            else if (stmt is BoundWhileStatement ws)
            {
                while (EvaluateExpression(ws.Condition))
                    EvaluateStatement(ws.Body);
            }
            else if (stmt is BoundPrintStatement ps)
            {
                Console.WriteLine(EvaluateExpression(ps.Expression));
            }
            else if (stmt is BoundForStatement fs)
            {
                EvaluateStatement(fs.VariableDecleration);
                while (EvaluateExpression(fs.Condition))
                {
                    EvaluateStatement(fs.Body);
                    EvaluateExpression(fs.Increment);
                }
            }
        }

        public void Evaluate() => EvaluateStatement(Root);

    }
}