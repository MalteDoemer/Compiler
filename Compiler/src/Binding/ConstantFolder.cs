using System;
using Compiler.Symbols;

namespace Compiler.Binding
{
    internal sealed class ConstantFolder
    {
        public static BoundConstant ComputeConstantUnary(BoundUnaryOperator op, BoundExpression operand)
        {
            if (operand.HasConstant)
            {
                var value = operand.Constant.Value;
                object res;

                switch (op)
                {
                    case BoundUnaryOperator.Identety:
                        res = value;
                        break;
                    case BoundUnaryOperator.Negation:
                        if (value is int i1) res = -i1;
                        else if (value is double d1) res = -d1;
                        else throw new Exception("Invalid state");
                        break;
                    case BoundUnaryOperator.LogicalNot:
                        if (value is bool b1) res = !b1;
                        else throw new Exception("Invalid state");
                        break;
                    case BoundUnaryOperator.BitwiseNot:
                        if (value is int i2) res = ~i2;
                        else throw new Exception("Invalid state");
                        break;
                    case BoundUnaryOperator.Invalid:
                        return null;

                    default: throw new Exception($"Unexpected Unary operator ${op}");
                }

                return new BoundConstant(res);
            }

            return null;
        }

        public static BoundConstant ComputeConstantBinary(BoundBinaryOperator op, BoundExpression left, BoundExpression right)
        {
            if (op == BoundBinaryOperator.LogicalAnd)
                if (left.HasConstant && !(bool)left.Constant.Value || right.HasConstant && !(bool)right.Constant.Value)
                    return new BoundConstant(false);

            if (op == BoundBinaryOperator.LogicalOr)
                if (left.HasConstant && (bool)left.Constant.Value || right.HasConstant && (bool)right.Constant.Value)
                    return new BoundConstant(true);

            if (left.HasConstant && right.HasConstant)
            {
                var leftVal = left.Constant.Value;
                var rightVal = right.Constant.Value;

                object res;

                switch (op)
                {
                    case BoundBinaryOperator.Addition:
                        {
                            if (leftVal is int i1 && rightVal is int i2) res = i1 + i2;
                            else if (leftVal is int i3 && rightVal is double d1) res = i3 + d1;
                            else if (leftVal is double d2 && rightVal is int i4) res = d2 + i4;
                            else if (leftVal is double d3 && rightVal is double d4) res = d3 + d4;
                            else if (leftVal is string s1) res = s1 + rightVal;
                            else if (rightVal is string s2) res = leftVal + s2;
                            else throw new Exception("Invalid state");
                            break;
                        }
                    case BoundBinaryOperator.Subtraction:
                        {
                            if (leftVal is int i1 && rightVal is int i2) res = i1 - i2;
                            else if (leftVal is int i3 && rightVal is double d1) res = i3 - d1;
                            else if (leftVal is double d2 && rightVal is int i4) res = d2 - i4;
                            else if (leftVal is double d3 && rightVal is double d4) res = d3 - d4;
                            else throw new Exception("Invalid state");
                            break;
                        }
                    case BoundBinaryOperator.Multiplication:
                        {
                            if (leftVal is int i1 && rightVal is int i2) res = i1 * i2;
                            else if (leftVal is int i3 && rightVal is double d1) res = i3 * d1;
                            else if (leftVal is double d2 && rightVal is int i4) res = d2 * i4;
                            else if (leftVal is double d3 && rightVal is double d4) res = d3 * d4;
                            else throw new Exception("Invalid state");
                            break;
                        }
                    case BoundBinaryOperator.Division:
                        {
                            if (leftVal is int i1 && rightVal is int i2) res = i1 / i2;
                            else if (leftVal is int i3 && rightVal is double d1) res = i3 / d1;
                            else if (leftVal is double d2 && rightVal is int i4) res = d2 / i4;
                            else if (leftVal is double d3 && rightVal is double d4) res = d3 / d4;
                            else throw new Exception("Invalid state");
                            break;
                        }
                    case BoundBinaryOperator.Power:
                        {
                            if (leftVal is int i1 && rightVal is int i2) res = Math.Pow(i1, i2);
                            else if (leftVal is int i3 && rightVal is double d1) res = Math.Pow(i3, d1);
                            else if (leftVal is double d2 && rightVal is int i4) res = Math.Pow(d2, i4);
                            else if (leftVal is double d3 && rightVal is double d4) res = Math.Pow(d3, d4);
                            else throw new Exception("Invalid state");
                            break;
                        }
                    case BoundBinaryOperator.Root:
                        {
                            if (leftVal is int i1 && rightVal is int i2) res = Math.Pow(i1, 1.0 / i2);
                            else if (leftVal is int i3 && rightVal is double d1) res = Math.Pow(i3, 1.0 / d1);
                            else if (leftVal is double d2 && rightVal is int i4) res = Math.Pow(d2, 1.0 / i4);
                            else if (leftVal is double d3 && rightVal is double d4) res = Math.Pow(d3, 1.0 / d4);
                            else throw new Exception("Invalid state");
                            break;
                        }
                    case BoundBinaryOperator.Modulo:
                        {
                            if (leftVal is int i1 && rightVal is int i2) res = i1 % i2;
                            else if (leftVal is int i3 && rightVal is double d1) res = i3 % d1;
                            else if (leftVal is double d2 && rightVal is int i4) res = d2 % i4;
                            else if (leftVal is double d3 && rightVal is double d4) res = d3 % d4;
                            else throw new Exception("Invalid state");
                            break;
                        }
                    case BoundBinaryOperator.EqualEqual:
                        {
                            if (leftVal is int i1 && rightVal is int i2) res = i1 == i2;
                            else if (leftVal is int i3 && rightVal is double d1) res = i3 == d1;
                            else if (leftVal is double d2 && rightVal is int i4) res = d2 == i4;
                            else if (leftVal is double d3 && rightVal is double d4) res = d3 == d4;
                            else if (leftVal is bool b1 && rightVal is bool b2) res = b1 == b2;
                            else if (leftVal is string s1 && rightVal is string s2) res = s1 == s2;
                            else res = leftVal.Equals(rightVal);
                            break;
                        }
                    case BoundBinaryOperator.NotEqual:
                        {
                            if (leftVal is int i1 && rightVal is int i2) res = i1 != i2;
                            else if (leftVal is int i3 && rightVal is double d1) res = i3 != d1;
                            else if (leftVal is double d2 && rightVal is int i4) res = d2 != i4;
                            else if (leftVal is double d3 && rightVal is double d4) res = d3 != d4;
                            else if (leftVal is bool b1 && rightVal is bool b2) res = b1 != b2;
                            else if (leftVal is string s1 && rightVal is string s2) res = s1 != s2;
                            else res = !leftVal.Equals(rightVal);
                            break;
                        }
                    case BoundBinaryOperator.LessThan:
                        {
                            if (leftVal is int i1 && rightVal is int i2) res = i1 < i2;
                            else if (leftVal is int i3 && rightVal is double d1) res = i3 < d1;
                            else if (leftVal is double d2 && rightVal is int i4) res = d2 < i4;
                            else if (leftVal is double d3 && rightVal is double d4) res = d3 < d4;
                            else throw new Exception("Invalid state");
                            break;
                        }
                    case BoundBinaryOperator.GreaterThan:
                        {
                            if (leftVal is int i1 && rightVal is int i2) res = i1 > i2;
                            else if (leftVal is int i3 && rightVal is double d1) res = i3 > d1;
                            else if (leftVal is double d2 && rightVal is int i4) res = d2 > i4;
                            else if (leftVal is double d3 && rightVal is double d4) res = d3 > d4;
                            else throw new Exception("Invalid state");
                            break;
                        }
                    case BoundBinaryOperator.LessEqual:
                        {
                            if (leftVal is int i1 && rightVal is int i2) res = i1 <= i2;
                            else if (leftVal is int i3 && rightVal is double d1) res = i3 <= d1;
                            else if (leftVal is double d2 && rightVal is int i4) res = d2 <= i4;
                            else if (leftVal is double d3 && rightVal is double d4) res = d3 <= d4;
                            else throw new Exception("Invalid state");
                            break;
                        }
                    case BoundBinaryOperator.GreaterEqual:
                        {
                            if (leftVal is int i1 && rightVal is int i2) res = i1 >= i2;
                            else if (leftVal is int i3 && rightVal is double d1) res = i3 >= d1;
                            else if (leftVal is double d2 && rightVal is int i4) res = d2 >= i4;
                            else if (leftVal is double d3 && rightVal is double d4) res = d3 >= d4;
                            else throw new Exception("Invalid state");
                            break;
                        }
                    case BoundBinaryOperator.LogicalAnd:
                        {
                            if (leftVal is bool b1 && rightVal is bool b2) res = b1 && b2;
                            else throw new Exception("Invalid state");
                            break;
                        }
                    case BoundBinaryOperator.LogicalOr:
                        {
                            if (leftVal is bool b1 && rightVal is bool b2) res = b1 || b2;
                            else throw new Exception("Invalid state");
                            break;
                        }
                    case BoundBinaryOperator.BitwiseAnd:
                        {
                            if (leftVal is int i1 && rightVal is int i2) res = i1 & i2;
                            else if (leftVal is bool b1 && rightVal is bool b2) res = b1 & b2;
                            else throw new Exception("Invalid state");
                            break;
                        }
                    case BoundBinaryOperator.BitwiseOr:
                        {
                            if (leftVal is int i1 && rightVal is int i2) res = i1 | i2;
                            else if (leftVal is bool b1 && rightVal is bool b2) res = b1 | b2;
                            else throw new Exception("Invalid state");
                            break;
                        }
                    case BoundBinaryOperator.BitwiseXor:
                        {
                            if (leftVal is int i1 && rightVal is int i2) res = i1 ^ i2;
                            else if (leftVal is bool b1 && rightVal is bool b2) res = b1 ^ b2;
                            else throw new Exception("Invalid state");
                            break;
                        }
                    case BoundBinaryOperator.Invalid:
                        return null;

                    default: throw new Exception($"Unexpected binary operator ${op}");
                }

                return new BoundConstant(res);
            }

            return null;
        }

        public static BoundConstant ComputeConstantConversion(TypeSymbol typeToConvert, BoundExpression expression)
        {
            if (expression.HasConstant)
            {
                var value = expression.Constant.Value;
                object res;

                switch (expression.ResultType.Name, typeToConvert.Name)
                {
                    case ("int", "float"):
                        {
                            var i = (int)value;
                            res = (double)i;
                            break;
                        }
                    case ("float", "int"):
                        {
                            var d = (double)value;
                            res = (int)d;
                            break;
                        }
                    case ("float", "str"):
                    case ("int", "str"):
                    case ("bool", "str"):
                    case ("any", "str"):
                        res = Convert.ToString(value);
                        break;

                    case ("any", "int"):
                        res = Convert.ToInt32(value);
                        break;
                    case ("any", "float"):
                        res = Convert.ToDouble(value);
                        break;
                    case ("any", "bool"):
                        res = Convert.ToBoolean(value);
                        break;
                    case ("int", "any"):
                    case ("float", "any"):
                    case ("bool", "any"):
                    case ("str", "any"):
                        res = value;
                        break;
                    default: throw new Exception($"Unexpected type ${typeToConvert}");
                }

                return new BoundConstant(res);
            }

            return null;
        }
    }
}
