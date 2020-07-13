using System;
using System.Collections.Generic;
using Compiler.Binding;
using Compiler.Symbols;

using static System.Math;

namespace Compiler
{

    internal sealed class Evaluator
    {
        private readonly Dictionary<string, object> globals;
        private readonly Stack<Dictionary<string, object>> stackFrames;
        private readonly BoundProgram program;
        private readonly Random random;

        private dynamic lastValue;


        public Evaluator(BoundProgram program, Dictionary<string, object> globals)
        {
            this.program = program;
            this.globals = globals;
            this.stackFrames = new Stack<Dictionary<string, object>>();
            this.random = new Random();
        }

        public dynamic Evaluate()
        {
            if (program.MainFunction != null)
            {
                var locals = new Dictionary<string, object>();
                stackFrames.Push(locals);
                var body = program.GetFunctionBody(program.MainFunction);
                EvaluateBlock(body);
                stackFrames.Pop();
            }
            return lastValue;
        }

        private dynamic EvaluateBlock(BoundBlockStatement block)
        {
            var labelToIndex = new Dictionary<BoundLabel, int>();

            for (int i = 0; i < block.Statements.Length; i++)
                if (block.Statements[i] is BoundLabelStatement label)
                    labelToIndex.Add(label.Label, i);

            var instPtr = 0;

            while (instPtr < block.Statements.Length)
            {
                var stmt = block.Statements[instPtr];

                switch (stmt)
                {
                    case BoundExpressionStatement es:
                        lastValue = EvaluateExpression(es.Expression);
                        instPtr++;
                        break;
                    case BoundVariableDeclarationStatement vs:
                        EvaluateVariableDeclaration(vs);
                        instPtr++;
                        break;
                    case BoundConditionalGotoStatement cgs:
                        bool condition = EvaluateExpression(cgs.Condition);
                        if ((condition && !cgs.JumpIfFalse) || (!condition && cgs.JumpIfFalse))
                            instPtr = labelToIndex[cgs.Label];
                        else instPtr++;
                        break;
                    case BoundGotoStatement gs:
                        instPtr = labelToIndex[gs.Label];
                        break;
                    case BoundReturnStatement rs:
                        object res = null;
                        if (rs.Expression != null)
                            res = EvaluateExpression(rs.Expression);
                        lastValue = res;
                        return res;
                    case BoundLabelStatement _:
                        instPtr++;
                        break;
                    default: throw new Exception($"Unexpected Statement <{stmt}>");
                }
            }

            return null;
        }

        private dynamic EvaluateExpression(BoundExpression expr)
        {
            switch (expr)
            {
                case BoundLiteralExpression le:
                    return le.Value;
                case BoundVariableExpression ve:
                    return EvaluateVariableExpression(ve);
                case BoundUnaryExpression ue:
                    return EvaluateUnaryExpression(ue);
                case BoundBinaryExpression be:
                    return EvaluateBinaryExpression(be);
                case BoundAssignmentExpression ae:
                    return EvaluateAssignment(ae);
                case BoundCallExpression boundCall:
                    return EvaluateFunctionCall(boundCall);
                case BoundConversionExpression boundConversion:
                    return EvaluateConversion(boundConversion);
                default:
                    throw new Exception("Unknown Expression");
            }
        }

        private dynamic EvaluateVariableExpression(BoundVariableExpression expr)
        {
            if (expr.Variable is LocalVariableSymbol local)
                return stackFrames.Peek()[local.Name];
            else if (expr.Variable is ParameterSymbol param)
                return stackFrames.Peek()[param.Name];
            else if (expr.Variable is GlobalVariableSymbol global)
                return globals[global.Name];
            else throw new Exception("Impossible to reach");
        }

        private dynamic EvaluateAssignment(BoundAssignmentExpression expr)
        {
            var val = EvaluateExpression(expr.Expression);

            if (expr.Variable is LocalVariableSymbol local)
                stackFrames.Peek()[local.Name] = val;
            else if (expr.Variable is ParameterSymbol param)
                stackFrames.Peek()[param.Name] = val;
            else if (expr.Variable is GlobalVariableSymbol global)
                globals[global.Name] = val;
            else throw new Exception("Impossible state");

            return val;
        }

        private dynamic EvaluateBinaryExpression(BoundBinaryExpression expr)
        {
            var left = EvaluateExpression(expr.Left);
            var right = EvaluateExpression(expr.Right);

            switch (expr.Op)
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
                    throw new Exception($"Unknown binary operator <{expr.Op}>");
            }
        }

        private dynamic EvaluateUnaryExpression(BoundUnaryExpression expr)
        {
            dynamic val = EvaluateExpression(expr.Right);
            switch (expr.Op)
            {
                case BoundUnaryOperator.Identety: return val;
                case BoundUnaryOperator.Negation: return -val;
                case BoundUnaryOperator.LogicalNot: return !val;
                case BoundUnaryOperator.BitwiseNot: return ~val;
                default:
                    throw new Exception($"Unknown Unary Operator <{expr.Op}>");
            }
        }

        private dynamic EvaluateConversion(BoundConversionExpression expr)
        {
            object val = EvaluateExpression(expr.Expression);

            if (expr.Type == TypeSymbol.Bool)
                return Convert.ToBoolean(val);
            else if (expr.Type == TypeSymbol.Int)
                return Convert.ToInt64(val);
            else if (expr.Type == TypeSymbol.Float)
                return Convert.ToDouble(val);
            else if (expr.Type == TypeSymbol.String)
                return Convert.ToString(val);
            else if (expr.Type == TypeSymbol.Any)
            {
                switch (val)
                {
                    case long l: return l;
                    case double d: return d;
                    case bool b: return b;
                    case string s: return s;
                    default: return val;
                }
            }
            else throw new Exception($"Unexpected Conversion Type <{expr.Type}>");
        }

        private void EvaluateVariableDeclaration(BoundVariableDeclarationStatement vs)
        {
            var val = EvaluateExpression(vs.Expression);
            if (vs.Variable is LocalVariableSymbol local)
                stackFrames.Peek()[local.Name] = val;
            else if (vs.Variable is GlobalVariableSymbol global)
                globals[global.Name] = val;
        }

        private dynamic EvaluateFunctionCall(BoundCallExpression expr)
        {
            if (expr.Symbol == BuiltInFunctions.Input)
            {
                return Console.ReadLine();
            }
            else if (expr.Symbol == BuiltInFunctions.Print)
            {
                var message = EvaluateExpression(expr.Arguments[0]);
                Console.WriteLine(message);
                return null;
            }
            else if (expr.Symbol == BuiltInFunctions.Len)
            {
                var str = (string)EvaluateExpression(expr.Arguments[0]);
                return str.Length;
            }
            else if (expr.Symbol == BuiltInFunctions.Clear)
            {
                Console.Clear();
                return null;
            }
            else if (expr.Symbol == BuiltInFunctions.Exit)
            {
                var exitCode = EvaluateExpression(expr.Arguments[0]);
                Environment.Exit((int)exitCode);
                return null;
            }
            else if (expr.Symbol == BuiltInFunctions.Random)
            {
                long lowerBound = EvaluateExpression(expr.Arguments[0]);
                long upperBound = EvaluateExpression(expr.Arguments[1]);
                return random.Next((int)lowerBound, (int)upperBound);
            }
            else if (expr.Symbol == BuiltInFunctions.RandomFloat)
            {
                return random.NextDouble();
            }
            else
            {
                var locals = new Dictionary<string, object>(expr.Arguments.Length);

                for (var i = 0; i < expr.Arguments.Length; i++)
                {
                    var val = EvaluateExpression(expr.Arguments[i]);
                    locals[expr.Symbol.Parameters[i].Name] = val;
                }
                var body = program.GetFunctionBody(expr.Symbol);
                stackFrames.Push(locals);
                var res = EvaluateBlock(body);
                stackFrames.Pop();
                return res;
            }
        }
    }
}