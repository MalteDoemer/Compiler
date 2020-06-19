using System.Collections.Generic;

namespace Compiler
{
    internal enum SyntaxTokenKind
    {
        End,
        Invalid,
        Error,
        Number,
        String,
        Identifier,

        Plus,
        Minus,
        Star,
        Slash,
        StarStar,
        SlashSlah,

        True,
        False,
        Null
    }

    internal class SyntaxToken
    {
        public readonly SyntaxTokenKind kind;
        public readonly int pos;
        public readonly dynamic value;

        public SyntaxToken(SyntaxTokenKind kind, int pos, dynamic value)
        {
            this.kind = kind;
            this.pos = pos;
            this.value = value;
        }

        public override string ToString() => $"{kind} at {pos} : {value}";
    }

    internal static class SyntaxFacts
    {
        public static readonly Dictionary<string, SyntaxTokenKind> SingleCharacters = new Dictionary<string, SyntaxTokenKind>()
        {
            {"+", SyntaxTokenKind.Plus},
            {"-", SyntaxTokenKind.Minus},
            {"*", SyntaxTokenKind.Star},
            {"/", SyntaxTokenKind.Slash},
        };

        public static readonly Dictionary<string, SyntaxTokenKind> DoubleCharacters = new Dictionary<string, SyntaxTokenKind>()
        {
            {"**", SyntaxTokenKind.StarStar},
            {"//", SyntaxTokenKind.SlashSlah},
        };

        public static readonly Dictionary<string, SyntaxTokenKind> Keywords = new Dictionary<string, SyntaxTokenKind>()
        {
            {"true", SyntaxTokenKind.True},
            {"false", SyntaxTokenKind.False},
            {"null", SyntaxTokenKind.Null},
        };
    
        public static dynamic GetKeywordValue(string keyword)
        {
            switch(keyword)
            {
                case "true": return true;
                case "false": return false;
                case "null": return null;
                default: return keyword;
            }
        }

    }

    internal class Lexer
    {

        private readonly List<string> diagnostics;
        private readonly string text;
        private int pos;
        private char current
        {
            get
            {
                if (pos < text.Length) return text[pos];
                else return '\0';
            }
        }

        public Lexer(string text)
        {
            this.text = text;
            diagnostics = new List<string>();
        }

        private char Advance()
        {
            char res = current;
            pos++;
            return res;
        }

        private string Peak(int len)
        {
            if (pos + len < text.Length) return text.Substring(pos, len);
            else return "\0";
        }

        private SyntaxToken LexSpace()
        {
            while (char.IsWhiteSpace(current)) pos++;
            return NextToken();
        }

        private SyntaxToken LexNumber()
        {
            int start = pos;
            long num = 0;

            while (char.IsDigit(current))
            {
                num *= 10;
                num += current - '0';
                pos++;
            }

            if (current == '.')
            {
                pos++;
                double fnum = num;
                long weight = 1;

                if (!char.IsDigit(current)) diagnostics.Add($"SyntaxError at {pos}\nInvalid decimal point.");

                while (char.IsDigit(current))
                {
                    weight *= 10;
                    fnum += (double)(current - '0') / (double)weight;
                    pos++;
                }
                return new SyntaxToken(SyntaxTokenKind.Number, start, fnum);
            }
            else return new SyntaxToken(SyntaxTokenKind.Number, start, num);

        }

        private SyntaxToken LexIdentifier()
        {
            int start = pos;
            while (char.IsLetter(current)) pos++;
            return new SyntaxToken(SyntaxTokenKind.Identifier, pos, text.Substring(start, pos - start));
        }

        private SyntaxToken LexString()
        {
            pos++;
            int start = pos;
            while (current != '"')
            {
                if (current == '\0')
                {
                    diagnostics.Add($"SyntaxError at {pos}\nNeverClosedString");
                    break;
                }
                else pos++;
            }
            return new SyntaxToken(SyntaxTokenKind.String, start, text.Substring(start, pos - start));
        }

        private SyntaxToken LexSingleChar()
        {
            foreach (var pair in SyntaxFacts.SingleCharacters)
                if (current == pair.Key[0]) return new SyntaxToken(pair.Value, pos++, pair.Key);
            return null;
        }

        private SyntaxToken LexDoubleChar()
        {
            foreach (var pair in SyntaxFacts.DoubleCharacters)
                if (pos + 1 < text.Length && text[pos] == pair.Key[0] && text[pos + 1] == pair.Key[1])
                    return new SyntaxToken(pair.Value, (pos += 2) - 2, pair.Key);
            return null;
        }

        private SyntaxToken LexKeyWord()
        {
            foreach (var pair in SyntaxFacts.Keywords)
                if (Peak(pair.Key.Length) == pair.Key)
                    return new SyntaxToken(pair.Value, (pos += pair.Key.Length) - pair.Key.Length, pair.Key);
            return null;
        }

        public SyntaxToken NextToken()
        {
            var keyword = LexKeyWord();
            if (keyword != null) return keyword;

            var doubleChar = LexDoubleChar();
            if (doubleChar != null) return doubleChar;

            var singleChar = LexSingleChar();
            if (singleChar != null) return singleChar;

            if (current == '\0') return new SyntaxToken(SyntaxTokenKind.End, pos, "End");
            else if (current == '"') return LexString();
            else if (char.IsNumber(current)) return LexNumber();
            else if (char.IsWhiteSpace(current)) return LexSpace();
            else if (char.IsLetter(current)) return LexIdentifier();
            else return new SyntaxToken(SyntaxTokenKind.Invalid, pos, Advance());
        }
    }
}