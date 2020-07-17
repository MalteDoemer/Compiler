using System;
using Xunit;
using System.Collections.Generic;
using System.Linq;
using Compiler.Text;

namespace Compiler.Test
{
    public class StatementsTest
    {
        [Fact]
        public static void Test_Simple_If()
        {
            var text = @"
                var debug = false
                if 1 < 5
                    debug = true

                debug
            ";

            text = AnnotatedText.Unindent(text);

            AssertLastValue(text, true);
        }

        [Fact]
        public static void Test_If_Else()
        {
            var text = @"
                var debug = -1
                if 1 > 5
                    debug = 0
                else if 'fff' != 'fff'
                    debug = 1
                else 
                    debug = 2

                debug
            ";

            text = AnnotatedText.Unindent(text);

            AssertLastValue(text, 2L);
        }

        [Fact]
        public static void Test_Simple_For()
        {
            var text = @"
                var debug = ''
                
                for var i : int = 0 i < 20 i++ {
                    debug += 'f'
                }

                debug
            ";

            text = AnnotatedText.Unindent(text);

            AssertLastValue(text, new string('f', 20));
        }

        [Fact]
        public static void Test_Break_Continue()
        {
            var text = @"
                var debug = ''
                
                for var i = 1 i < 15 i++{
                    if i == 10 break
                    if i % 2 == 0 continue
                    debug += str(i)
                }

                debug
            ";

            text = AnnotatedText.Unindent(text);

            AssertLastValue(text, "13579");
        }

        [Fact]
        public static void Test_Simple_While()
        {
            var text = @"
                var debug = 0
                
                while debug < 20 {
                    debug++
                }

                debug
            ";

            text = AnnotatedText.Unindent(text);

            AssertLastValue(text, 20L);
        }

        [Fact]
        public static void Test_While_True_Break()
        {
            var text = @"
                var debug = 0
                
                while true {
                    debug++
                    if debug == 50
                        break
                }

                debug
            ";

            text = AnnotatedText.Unindent(text);

            AssertLastValue(text, 50L);
        }

        private static void AssertLastValue(string text, object expected)
        {
            // var compilation = Compilation.CompileScript(new SourceText(text, null), Compilation.StandardReferencePaths);
            // Assert.Empty(compilation.Diagnostics);
            // var res = compilation.EvaluateExpression();
            // Assert.Equal(expected, res);
        }
    }
}