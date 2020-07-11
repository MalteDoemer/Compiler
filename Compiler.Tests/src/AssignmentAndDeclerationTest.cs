﻿using Xunit;
using System;
using Compiler;
using Compiler.Text;

namespace Compiler.Test
{
    public class VariableTests
    {
        [Theory]
        [InlineData("i++", "i", 0L, 1L)]
        [InlineData("hello += 2", "hello", 0.5, 2.5)]
        [InlineData("i *= 1.0 / 2.0", "i", 40L, 20.0)]
        [InlineData("i--", "i", 0L, -1L)]
        [InlineData("i*=2", "i", 0L, 0L)]
        [InlineData("i |= 3", "i", 4L, 4L | 3L)]
        public void Variable_Has_Correct_Value(string text, string variable, object initialVal, object expected)
        {
            var line1 = $"var {variable} = {initialVal}";
            var line2 = text;
            var line3 = variable;

            var comp1 = Compilation.CompileScript(new SourceText(line1, null), null);
            var comp2 = Compilation.CompileScript(new SourceText(line2, null), comp1);
            var comp3 = Compilation.CompileScript(new SourceText(line3, null), comp2);

            Assert.Empty(comp1.Diagnostics);
            Assert.Empty(comp2.Diagnostics);
            Assert.Empty(comp3.Diagnostics);

            comp1.Evaluate();
            comp2.Evaluate();

            var res = comp3.EvaluateExpression();
            Assert.Equal(expected, res);
        }
    }
}