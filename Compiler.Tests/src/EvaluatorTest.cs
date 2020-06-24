using Xunit;

namespace Compiler.Test
{
    public class EvaluatorTest
    {
        [Theory]
        [InlineData("1", 1L)]
        [InlineData("+1", 1L)]
        [InlineData("-1", -1L)]
        [InlineData("~1", -2L)]
        [InlineData("14 + 12", 26L)]
        [InlineData("12 - 3", 9L)]
        [InlineData("4 * 2", 8L)]
        [InlineData("9 / 3", 3L)]
        [InlineData("9 % 3", 0L)]
        [InlineData("(10)", 10L)]
        [InlineData("12 == 3", false)]
        [InlineData("3 == 3", true)]
        [InlineData("12 != 3", true)]
        [InlineData("3 != 3", false)]
        [InlineData("3 < 4", true)]
        [InlineData("5 < 4", false)]
        [InlineData("4 <= 4", true)]
        [InlineData("4 <= 5", true)]
        [InlineData("5 <= 4", false)]
        [InlineData("4 > 3", true)]
        [InlineData("4 > 5", false)]
        [InlineData("4 >= 4", true)]
        [InlineData("5 >= 4", true)]
        [InlineData("4 >= 5", false)]
        [InlineData("1.5", 1.5)]
        [InlineData("+3.2", 3.2)]
        [InlineData("-1.12", -1.12)]
        [InlineData("14.25 + 12.78", 27.03)]
        [InlineData("12.5 - 3.5", 9.0)]
        [InlineData("4.5 * 2", 9.0)]
        [InlineData("10 / 3.4", 2.9411764705882352941176470588235)]
        [InlineData("4.5 % 2", 0.5)]
        [InlineData("(10.10)", 10.10)]
        [InlineData("12.12 == 3", false)]
        [InlineData("3 == 3.0", true)]
        [InlineData("12 != 3.12", true)]
        [InlineData("3.1 != 3.1", false)]
        [InlineData("3.0 < 4", true)]
        [InlineData("5.8 < 4", false)]
        [InlineData("4.2 <= 4.2", true)]
        [InlineData("4.9 <= 5", true)]
        [InlineData("5.5 <= 4", false)]
        [InlineData("4.1 > 3", true)]
        [InlineData("4.9 > 5", false)]
        [InlineData("4.23 >= 4.23", true)]
        [InlineData("5.5 >= 4", true)]
        [InlineData("4 >= 5.87", false)]
        [InlineData("1 | 2", 3L)]
        [InlineData("1 | 0", 1L)]
        [InlineData("1 & 3", 1L)]
        [InlineData("1 & 0", 0L)]
        [InlineData("1 ^ 0", 1L)]
        [InlineData("0 ^ 1", 1L)]
        [InlineData("1 ^ 3", 2L)]
        [InlineData("false == false", true)]
        [InlineData("true == false", false)]
        [InlineData("false != false", false)]
        [InlineData("true != false", true)]
        [InlineData("true && true", true)]
        [InlineData("false || false", false)]
        [InlineData("false | false", false)]
        [InlineData("false | true", true)]
        [InlineData("true | false", true)]
        [InlineData("true | true", true)]
        [InlineData("false & false", false)]
        [InlineData("false & true", false)]
        [InlineData("true & false", false)]
        [InlineData("true & true", true)]
        [InlineData("false ^ false", false)]
        [InlineData("true ^ false", true)]
        [InlineData("false ^ true", true)]
        [InlineData("true ^ true", false)]
        [InlineData("true", true)]
        [InlineData("false", false)]
        [InlineData("!true", false)]
        [InlineData("!false", true)]
        [InlineData("\"test\"", "test")]
        [InlineData("'test'", "test")]
        [InlineData("'test' == 'test'", true)]
        [InlineData("'test' != 'test'", false)]
        [InlineData("'test' == 'abc'", false)]
        [InlineData("\"test\" != \"abc\"", true)]
        public static void Evaluate_Result(string text, object expceted)
        {
            var compiltaion = Compilation.Compile(text);
            var res = compiltaion.Evaluate();

            Assert.Empty(compiltaion.Diagnostics);
            Assert.Equal(expceted, res);
        }
    }
}