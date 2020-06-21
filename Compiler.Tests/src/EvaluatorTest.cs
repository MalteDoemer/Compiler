using Xunit;
using Compiler.Syntax;
using Compiler.Text;
using Compiler.Binding;
using System.Collections.Generic;

namespace Compiler
{

    public class EvaluatorTest
    {
        [Theory]
        [InlineData("1 + 2", 3)]
        [InlineData("1 + 2 * 3", 7)]
        [InlineData("(1 + 2) * 3", 9)]
        [InlineData("1.5 + 2.9", 4.4)]
        [InlineData("1.5 + 2.9 - 4.4", 0)]
        [InlineData("1.5 + 2", 3.5)]
        [InlineData("\"fett\"", "fett")]
        [InlineData("null", null)]
        [InlineData("true", true)]
        [InlineData("false", false)]
        [InlineData("true && true", true)]
        [InlineData("true && false", false)]
        [InlineData("false || true", true)]
        [InlineData("false || false", false)]
        [InlineData("false || true || 1 == 2", true)]
        [InlineData("1 != 1.0 == false", true)]
        [InlineData("100 < 100000", true)]
        public static void EvaluateCorrecResult(string text, dynamic expectedResult)
        {
            var env = new Dictionary<string, VariableSymbol>();
            var compilation = new Compilation(SyntaxTree.ParseSyntaxTree(new SourceText(text)));
            var res = compilation.Evaluate(env);
            Assert.Equal(expectedResult, res);
            Assert.Equal(0, compilation.Tree.Diagnostics.Errors);
        }
    }
}