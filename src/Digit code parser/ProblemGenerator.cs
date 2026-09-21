using System.Diagnostics;
using PeterO.Numbers;
using static Lunariens_Mental_Math_Trainer.ASTBuilder;

namespace Lunariens_Mental_Math_Trainer
{
    internal static class ProblemGenerator
    {
        private static string OperationToString(UnaryOp op)
        {
            return "-";
        }
        private static string OperationToString(BinaryOp op)
        {
            switch (op)
            {
                case BinaryOp.Add:
                    return "+";
                case BinaryOp.Subtract:
                    return "-";
                case BinaryOp.Multiply:
                    return "*";
                case BinaryOp.Divide:
                    return "/";
                case BinaryOp.Power:
                    return "^";
                case BinaryOp.Root:
                    return "√";
                default:
                    throw new FormatException("Error: Something really wrong happened while translating BinaryOp operations into characters.");
            }
        }
        private static EDecimal ExecuteOperation(BinaryOp op, EDecimal left, EDecimal right)
        {
            EContext ctx = new(10000, ERounding.Floor, -10000, 10000, true);
            switch (op)
            {
                case BinaryOp.Add:
                    return left + right;
                case BinaryOp.Subtract:
                    return left - right;
                case BinaryOp.Multiply:
                    return left * right;
                case BinaryOp.Divide:
                    return left.Divide(right, ctx);
                case BinaryOp.Power:
                    return left.Pow(right);
                case BinaryOp.Root:
                    return left.Pow(EDecimal.One / right);
                default:
                    throw new FormatException("Error: Something really wrong happened while translating BinaryOp operations into characters.");
            }
        }

        internal static EInteger RandomEInt(EInteger bottom, EInteger top)
        {
            if (bottom > top)
            {
                throw new ArgumentException("Bottom must be less than or equal to top.");
            }
            if (bottom == top)
                return bottom;
            Random randomness = new();
            EInteger rangeSize = top - bottom;

            EContext ctx = new(1000, ERounding.Floor, -10000, 10000, false);
            EFloat rangeLog2 = EFloat.FromEInteger(rangeSize).Log(ctx).Divide(EFloat.FromInt32(2).Log(ctx), ctx);
            EInteger byteCount = rangeLog2.RoundToPrecision(ctx).ToEInteger();
            byte[] buf = new byte[byteCount.ToInt32Unchecked() + 5];

            randomness.NextBytes(buf);
            EInteger result = (EInteger.FromBytes(buf, 0, buf.Length, false).Abs() % rangeSize) + bottom;
            return result;
        }

        internal static (string, EDecimal) GenerateProblem(Expression expression)
        {
            string problem;
            EDecimal result;
            switch (expression)
            {
                case Expression.Unary unary:
                    {
                        (string left, EDecimal val) = GenerateProblem(unary.Operand);
                        problem = OperationToString(unary.Op) + left;
                        result = -val;
                        break;
                    }
                case Expression.Binary binary:
                    {
                        (string left, EDecimal valLeft) = GenerateProblem(binary.Left);
                        string op = OperationToString(binary.Op);
                        (string right, EDecimal valRight) = GenerateProblem(binary.Right);

                        problem = left.ToString() + op + right.ToString();
                        result = ExecuteOperation(binary.Op, valLeft, valRight);
                        if (binary.Precision != null)
                        {
                            result = result.RoundToExponent(-binary.Precision, ERounding.Floor);
                        }
                        break;
                    }
                case Expression.Number number:
                    {
                        if (number.literal)
                        {
                            problem = number.Value.ToString();
                            result = (EDecimal)number.Value;
                            break;
                        }
                        EInteger bottom = EInteger.Ten.Pow(number.Value - 1);
                        EInteger top = EInteger.Ten.Pow(number.Value) - 1;
                        EInteger num = RandomEInt(bottom, top);

                        problem = num.ToString();
                        result = (EDecimal)num;
                        break;
                    }
                case Expression.Parentheses parens:
                    {
                        (string inner, EDecimal val) = GenerateProblem(parens.Expression);
                        problem = $"({inner})";
                        result = val;
                        break;
                    }
                case Expression.Range range:
                    {
                        EInteger bottom = range.Start;
                        EInteger top = range.End;
                        EInteger num = RandomEInt(bottom, top);

                        problem = num.ToString();
                        result = (EDecimal)num;
                        break;
                    }
                default:
                    throw new UnreachableException("Error: unreachable reached");

            }
            return (problem, result);
        }
    }
}