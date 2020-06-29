using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Compiler.Syntax
{
    public static class SyntaxFacts
    {

        internal const int MaxPrecedence = 6;

        internal static object GetKeywordValue(string keyword)
        {
            switch (keyword)
            {
                case "true": return true;
                case "false": return false;
                default: return keyword;
            }
        }

        internal static bool IsLiteralExpression(SyntaxTokenKind kind)
        {
            switch (kind)
            {
                case SyntaxTokenKind.Float:
                case SyntaxTokenKind.Int:
                case SyntaxTokenKind.String:
                case SyntaxTokenKind.True:
                case SyntaxTokenKind.False:
                    return true;
                default: return false;
            }
        }

        internal static bool IsTypeKeyword(this SyntaxTokenKind kind)
        {
            switch (kind)
            {
                case SyntaxTokenKind.IntKeyword:
                case SyntaxTokenKind.FloatKeyword:
                case SyntaxTokenKind.BoolKeyword:
                case SyntaxTokenKind.StringKeyword:
                case SyntaxTokenKind.VarKeyword:
                    return true;
                default: return false;
            }
        }

        public static SyntaxTokenKind? IsSingleCharacter(char c)
        {
            switch (c)
            {
                case '+': return SyntaxTokenKind.Plus;
                case '-': return SyntaxTokenKind.Minus;
                case '*': return SyntaxTokenKind.Star;
                case '/': return SyntaxTokenKind.Slash;
                case '<': return SyntaxTokenKind.LessThan;
                case '>': return SyntaxTokenKind.GreaterThan;
                case '!': return SyntaxTokenKind.Bang;
                case '(': return SyntaxTokenKind.LParen;
                case ')': return SyntaxTokenKind.RParen;
                case '{': return SyntaxTokenKind.LCurly;
                case '}': return SyntaxTokenKind.RCurly;
                case '=': return SyntaxTokenKind.Equal;
                case '~': return SyntaxTokenKind.Tilde;
                case '%': return SyntaxTokenKind.Percentage;
                case '&': return SyntaxTokenKind.Ampersand;
                case '|': return SyntaxTokenKind.Pipe;
                case '^': return SyntaxTokenKind.Hat;
                case ',': return SyntaxTokenKind.Comma;
                default: return null;
            }
        }

        public static SyntaxTokenKind? IsDoubleCharacter(char c1, char c2)
        {
            switch (c1, c2)
            {
                case ('+', '+'): return SyntaxTokenKind.PlusPlus;
                case ('-', '-'): return SyntaxTokenKind.MinusMinus;
                case ('*', '*'): return SyntaxTokenKind.StarStar;
                case ('/', '/'): return SyntaxTokenKind.SlashSlah;
                case ('+', '='): return SyntaxTokenKind.PlusEqual;
                case ('-', '='): return SyntaxTokenKind.MinusEqual;
                case ('*', '='): return SyntaxTokenKind.StarEqual;
                case ('/', '='): return SyntaxTokenKind.SlashEqual;
                case ('=', '='): return SyntaxTokenKind.EqualEqual;
                case ('!', '='): return SyntaxTokenKind.NotEqual;
                case ('<', '='): return SyntaxTokenKind.LessEqual;
                case ('>', '='): return SyntaxTokenKind.GreaterEqual;
                case ('&', '&'): return SyntaxTokenKind.AmpersandAmpersand;
                case ('|', '|'): return SyntaxTokenKind.PipePipe;
                case ('&', '='): return SyntaxTokenKind.AmpersandEqual;
                case ('|', '='): return SyntaxTokenKind.PipeEqual;
                default: return null;
            }
        }

        internal static bool IsValidExpression(ExpressionSyntax expression)
        {
            if (!expression.IsValid) return false;
            else return true;
        }

        public static SyntaxTokenKind? IsKeyWord(string text)
        {
            switch (text)
            {
                case "true": return SyntaxTokenKind.True;
                case "false": return SyntaxTokenKind.False;
                case "int": return SyntaxTokenKind.IntKeyword;
                case "float": return SyntaxTokenKind.FloatKeyword;
                case "bool": return SyntaxTokenKind.BoolKeyword;
                case "string": return SyntaxTokenKind.StringKeyword;
                case "var": return SyntaxTokenKind.VarKeyword;
                case "if": return SyntaxTokenKind.IfKeyword;
                case "else": return SyntaxTokenKind.ElseKeyword;
                case "while": return SyntaxTokenKind.WhileKeyword;
                case "do": return SyntaxTokenKind.DoKeyword;
                case "for": return SyntaxTokenKind.ForKeyword;
                case "void": return SyntaxTokenKind.VoidKeyword;
                default: return null;
            }
        }

        public static string GetStringRepresentation(SyntaxTokenKind kind)
        {
            switch (kind)
            {
                case SyntaxTokenKind.Plus: return "+";
                case SyntaxTokenKind.Minus: return "-";
                case SyntaxTokenKind.Star: return "*";
                case SyntaxTokenKind.Slash: return "/";
                case SyntaxTokenKind.Equal: return "=";
                case SyntaxTokenKind.Bang: return "!";
                case SyntaxTokenKind.LessThan: return "<";
                case SyntaxTokenKind.GreaterThan: return ">";
                case SyntaxTokenKind.LParen: return "(";
                case SyntaxTokenKind.RParen: return ")";
                case SyntaxTokenKind.LCurly: return "{";
                case SyntaxTokenKind.RCurly: return "}";
                case SyntaxTokenKind.Tilde: return "~";
                case SyntaxTokenKind.Percentage: return "%";
                case SyntaxTokenKind.Pipe: return "|";
                case SyntaxTokenKind.Ampersand: return "&";
                case SyntaxTokenKind.Hat: return "^";
                case SyntaxTokenKind.Comma: return ",";

                case SyntaxTokenKind.StarStar: return "**";
                case SyntaxTokenKind.SlashSlah: return "//";
                case SyntaxTokenKind.EqualEqual: return "==";
                case SyntaxTokenKind.NotEqual: return "!=";
                case SyntaxTokenKind.LessEqual: return "<=";
                case SyntaxTokenKind.GreaterEqual: return ">=";
                case SyntaxTokenKind.AmpersandAmpersand: return "&&";
                case SyntaxTokenKind.PipePipe: return "||";

                case SyntaxTokenKind.PlusPlus: return "++";
                case SyntaxTokenKind.MinusMinus: return "--";
                case SyntaxTokenKind.PlusEqual: return "+=";
                case SyntaxTokenKind.MinusEqual: return "-=";
                case SyntaxTokenKind.StarEqual: return "*=";
                case SyntaxTokenKind.SlashEqual: return "/=";
                case SyntaxTokenKind.AmpersandEqual: return "&=";
                case SyntaxTokenKind.PipeEqual: return "|=";

                case SyntaxTokenKind.True: return "true";
                case SyntaxTokenKind.False: return "false";
                case SyntaxTokenKind.VarKeyword: return "var";
                case SyntaxTokenKind.IntKeyword: return "int";
                case SyntaxTokenKind.FloatKeyword: return "float";
                case SyntaxTokenKind.BoolKeyword: return "bool";
                case SyntaxTokenKind.StringKeyword: return "string";
                case SyntaxTokenKind.IfKeyword: return "if";
                case SyntaxTokenKind.ElseKeyword: return "else";
                case SyntaxTokenKind.WhileKeyword: return "while";
                case SyntaxTokenKind.ForKeyword: return "for";
                default: return null;
            }
        }

        public static int GetBinaryPrecedence(this SyntaxTokenKind kind)
        {
            switch (kind)
            {
                case SyntaxTokenKind.StarStar:
                case SyntaxTokenKind.SlashSlah:
                    return 1;

                case SyntaxTokenKind.Star:
                case SyntaxTokenKind.Slash:
                case SyntaxTokenKind.Percentage:
                    return 2;

                case SyntaxTokenKind.Plus:
                case SyntaxTokenKind.Minus:
                    return 3;

                case SyntaxTokenKind.LessEqual:
                case SyntaxTokenKind.GreaterEqual:
                case SyntaxTokenKind.LessThan:
                case SyntaxTokenKind.GreaterThan:
                case SyntaxTokenKind.EqualEqual:
                case SyntaxTokenKind.NotEqual:
                    return 4;

                case SyntaxTokenKind.Ampersand:
                case SyntaxTokenKind.Pipe:
                case SyntaxTokenKind.Hat:
                    return 5;

                case SyntaxTokenKind.AmpersandAmpersand:
                case SyntaxTokenKind.PipePipe:
                    return 6;

                default: return 0;
            }
        }

        public static IEnumerable<SyntaxTokenKind> GetUnaryOperators()
        {
            var tokens = (SyntaxTokenKind[])Enum.GetValues(typeof(SyntaxTokenKind));

            foreach (var t in tokens)
                if (IsUnaryOperator(t)) yield return t;
        }

        public static IEnumerable<SyntaxTokenKind> GetBinaryOperators()
        {
            var tokens = (SyntaxTokenKind[])Enum.GetValues(typeof(SyntaxTokenKind));

            foreach (var t in tokens)
                if (GetBinaryPrecedence(t) > 0) yield return t;
        }

        public static bool IsUnaryOperator(this SyntaxTokenKind kind)
        {
            switch (kind)
            {
                case SyntaxTokenKind.Minus:
                case SyntaxTokenKind.Plus:
                case SyntaxTokenKind.Bang:
                case SyntaxTokenKind.Tilde:
                    return true;
                default: return false;
            }
        }

        public static bool IsBinaryOperator(this SyntaxTokenKind kind) => GetBinaryPrecedence(kind) > 0;
    }
}