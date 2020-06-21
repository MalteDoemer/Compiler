using System.Collections.Generic;
using Compiler.Diagnostics;
using Compiler.Text;

namespace Compiler.Syntax
{
    internal class Lexer
    {
        private readonly DiagnosticBag diagnostics;
        private readonly SourceText text;
        private int pos;
        private char current
        {
            get
            {
                if (pos < text.Length) return text[pos];
                else return '\0';
            }
        }
        private char ahead
        {
            get
            {
                if (pos + 1 < text.Length) return text[pos + 1];
                else return '\0';
            }
        }


        public Lexer(SourceText text, DiagnosticBag diagnostics)
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
            if (pos + len <= text.Length) return text.ToString(pos, len);
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
                return new SyntaxToken(SyntaxTokenKind.Float, start, pos - start, fnum);
            }
            else return new SyntaxToken(SyntaxTokenKind.Int, start, pos - start, num);

        }

        private SyntaxToken LexIdentifierOrKeyword()
        {
            int start = pos;
            while (char.IsLetterOrDigit(current) || current == '_') pos++;

            var tokenText = text.ToString(start, pos - start);
            var isKeyword = SyntaxFacts.IsKeyWord(tokenText);

            if (isKeyword != null)
                return new SyntaxToken((SyntaxTokenKind)isKeyword, start, pos - start, SyntaxFacts.GetKeywordValue(tokenText));
            else return new SyntaxToken(SyntaxTokenKind.Identifier, start, pos - start,  tokenText);
        }

        private SyntaxToken LexString()
        {
            pos++;
            int start = pos;
            while (current != '"')
            {
                if (current == '\0')
                {
                    diagnostics.ReportNeverClosedString(start, pos);
                    var t1 = text.ToString(start, pos - start);
                    return new SyntaxToken(SyntaxTokenKind.String, start, t1.Length, t1);
                }
                else pos++;
            }
            pos++;
            var t = text.ToString(start, pos - start - 1);
            return new SyntaxToken(SyntaxTokenKind.String, start, text.Length, t);
        }

        private SyntaxToken LexSingleChar()
        {
            var kind = SyntaxFacts.IsSingleCharacter(current);
            if (kind != null) return new SyntaxToken((SyntaxTokenKind)kind, pos, 1, Advance());
            return null;
        }

        private SyntaxToken LexDoubleChar()
        {
            var kind = SyntaxFacts.IsDoubleCharacter(current, ahead);
            string value = "" + current + ahead;
            if (kind != null) return new SyntaxToken((SyntaxTokenKind)kind, (pos+=2) -2, 2, value);
            return null;
        }

        public SyntaxToken NextToken()
        {
            var doubleChar = LexDoubleChar();
            if (doubleChar != null) return doubleChar;

            var singleChar = LexSingleChar();
            if (singleChar != null) return singleChar;

            if (current == '\0') return new SyntaxToken(SyntaxTokenKind.End, pos, 0, "End");
            else if (current == '"') return LexString();
            else if (char.IsNumber(current)) return LexNumber();
            else if (char.IsWhiteSpace(current)) return LexSpace();
            else if (char.IsLetter(current) || current == '_') return LexIdentifierOrKeyword();
            else return new SyntaxToken(SyntaxTokenKind.Invalid, pos, 1, Advance());
        }

        public IEnumerable<SyntaxToken> Tokenize()
        {
            SyntaxToken token;
            do
            {
                token = NextToken();
                yield return token;
            } while (token.Kind != SyntaxTokenKind.End);
        }
    }
}