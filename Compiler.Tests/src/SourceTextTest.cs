using Xunit;

namespace Compiler.Text
{
    public class SourceTextTest
    {
        [Theory]
        [InlineData("", 1)]
        [InlineData("\n", 2)]
        [InlineData(".\r\n", 2)]
        [InlineData("\r\n   \r\n", 3)]
        [InlineData("\r", 2)]
        [InlineData("\r\r\r", 4)]
        public static void SourceTextLineNumbers(string text, int expected)
        {
            var src = new SourceText(text, null);
            Assert.Equal(expected, src.Lines.Length);
        }
    }
}