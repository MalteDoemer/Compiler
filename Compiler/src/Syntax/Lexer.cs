using System.Collections.Generic;
using Compiler.Diagnostics;

namespace Compiler.Syntax
{
    internal class Lexer
    {
        private readonly DiagnosticBag diagnostics;
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

        public Lexer(string text, DiagnosticBag diagnostics)
        {
            this.text = text;
            this.diagnostics = diagnostics;
            pos = 0;
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

                if (!char.IsDigit(current)) diagnostics.ReportInvalidDecimalPoint(pos);

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
                    diagnostics.ReportNeverClosedString(pos);
                    return new SyntaxToken(SyntaxTokenKind.String, start, text.Substring(start, pos - start));
                }
                else pos++;
            }
            pos++;
            return new SyntaxToken(SyntaxTokenKind.String, start, text.Substring(start, pos - start - 1));
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
                    return new SyntaxToken(pair.Value, (pos += pair.Key.Length) - pair.Key.Length, SyntaxFacts.GetKeywordValue(pair.Key));
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

        public IEnumerable<SyntaxToken> Tokenize()
        {
            SyntaxToken token;
            do
            {
                token = NextToken();
                yield return token;
            } while (token.kind != SyntaxTokenKind.End);
        }
    }
}