using System;
using System.IO;
using Compiler.Text;
using Xunit;

namespace Compiler.Test
{
    public class EvaluatorTest
    {
        [Theory]
        [InlineData("1", 1)]
        [InlineData("+1", 1)]
        [InlineData("-1", -1)]
        [InlineData("~1", -2)]
        [InlineData("14 + 12", 26)]
        [InlineData("12 - 3", 9)]
        [InlineData("4 * 2", 8)]
        [InlineData("9 / 3", 3)]
        [InlineData("9 % 3", 0)]
        [InlineData("(10)", 10)]
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
        [InlineData("1.5", 1.5f)]
        [InlineData("+3.2", 3.2f)]
        [InlineData("-1.12", -1.12f)]
        [InlineData("14.25 + 12.78", 27.03f)]
        [InlineData("12.5 - 3.5", 9.0f)]
        [InlineData("4.5 * 2", 9.0f)]
        [InlineData("10 / 3.4", 2.9411764705882352941176470588235d)]
        [InlineData("4.5 % 2", 0.5f)]
        [InlineData("(10.10)", 10.10f)]
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
        [InlineData("1 | 2", 3)]
        [InlineData("1 | 0", 1)]
        [InlineData("1 & 3", 1)]
        [InlineData("1 & 0", 0)]
        [InlineData("1 ^ 0", 1)]
        [InlineData("0 ^ 1", 1)]
        [InlineData("1 ^ 3", 2)]
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
        public static void Test_Correct_Value(string text, object value)
        {
            text = $"print({text})";
            var expected = value.ToString();
            var compilation = Compilation.CompileScript(new SourceText(text, null), Compilation.StandardReferencePaths);

            Assert.Empty(compilation.Diagnostics);

            using (var writer = new StringWriter())
            {
                var original = Console.Out;
                Console.SetOut(writer);
                compilation.Evaluate();
                var res = writer.ToString();
                Assert.Equal(expected, res);
            }
        }
    }
}