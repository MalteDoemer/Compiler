using Compiler;
using Compiler.Syntax;
using Compiler.Text;
using Xunit;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Compiler.Test
{
    public class ParserTest
    {
        [Theory]
        [MemberData(nameof(GetBinaryOperatorData))]
        public static void Parser_Honors_Binary_Precedence(SyntaxTokenKind op1, SyntaxTokenKind op2)
        {
            var op1Text = op1.GetStringRepresentation();
            var op2Text = op2.GetStringRepresentation();

            var op1Precedence = op1.GetBinaryPrecedence();
            var op2Precedence = op2.GetBinaryPrecedence();

            var text = $"a {op1Text} b {op2Text} c";
            var tree = SyntaxTree.ParseSyntaxTree(new SourceText(text, null), true);

            var member = Assert.Single(tree.Root.Members);
            var statement = Assert.IsType<GlobalStatementSynatx>(member).Statement;
            var expression = Assert.IsType<ExpressionStatementSyntax>(statement).Expression;

            Assert.IsType<BinaryExpressionSyntax>(expression);

            if (op1Precedence <= op2Precedence)
            {
                //     op2
                //    /   \
                //   op1   c
                //  /   \
                // a     b

                using (var asserter = new SyntaxTreeAsserter(expression))
                {
                    asserter.AssertNode(SyntaxNodeKind.BinaryExpressionSyntax);
                    asserter.AssertNode(SyntaxNodeKind.BinaryExpressionSyntax);
                    asserter.AssertNode(SyntaxNodeKind.VariableExpressionSyntax);
                    asserter.AssertToken(SyntaxTokenKind.Identifier, "a");
                    asserter.AssertToken(op1, op1Text);
                    asserter.AssertNode(SyntaxNodeKind.VariableExpressionSyntax);
                    asserter.AssertToken(SyntaxTokenKind.Identifier, "b");
                    asserter.AssertToken(op2, op2Text);
                    asserter.AssertNode(SyntaxNodeKind.VariableExpressionSyntax);
                    asserter.AssertToken(SyntaxTokenKind.Identifier, "c");
                }
            }
            else
            {
                //   op1
                //  /   \
                // a    op2
                //     /   \
                //    b     c

                using (var asserter = new SyntaxTreeAsserter(expression))
                {
                    asserter.AssertNode(SyntaxNodeKind.BinaryExpressionSyntax);
                    asserter.AssertNode(SyntaxNodeKind.VariableExpressionSyntax);
                    asserter.AssertToken(SyntaxTokenKind.Identifier, "a");
                    asserter.AssertToken(op1, op1Text);
                    asserter.AssertNode(SyntaxNodeKind.BinaryExpressionSyntax);
                    asserter.AssertNode(SyntaxNodeKind.VariableExpressionSyntax);
                    asserter.AssertToken(SyntaxTokenKind.Identifier, "b");
                    asserter.AssertToken(op2, op2Text);
                    asserter.AssertNode(SyntaxNodeKind.VariableExpressionSyntax);
                    asserter.AssertToken(SyntaxTokenKind.Identifier, "c");
                }
            }
        }

        public static IEnumerable<object[]> GetBinaryOperatorData()
        {
            foreach (var op1 in GetBinaryOperators())
                foreach (var op2 in GetBinaryOperators())
                    yield return new object[] { op1, op2 }; 
        }

        public static IEnumerable<SyntaxTokenKind> GetBinaryOperators()
        {
            return ((SyntaxTokenKind[])Enum.GetValues(typeof(SyntaxTokenKind))).Where(k => k.IsBinaryOperator());
        }
    }
}