using System.Collections.Generic;
using System.Collections.Immutable;
using Compiler.Diagnostics;
using Compiler.Text;

namespace Compiler.Syntax
{
    internal class Lexer
    {
        private readonly DiagnosticBag diagnostics;
        private readonly SourceText text;

        private int pos;
        private char current { get => pos < text.Length ? text[pos] : '\0'; }
        private char ahead { get => pos + 1 < text.Length ? text[pos + 1] : '\0'; }

        public Lexer(SourceText text)
        {
            this.text = text;
            diagnostics = new DiagnosticBag();
        }

        public IEnumerable<Diagnostic> GetDiagnostics() => diagnostics;

        private char Advance()
        {
            var res = current;
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

                if (!char.IsDigit(current)) diagnostics.ReportSyntaxError(ErrorMessage.InvalidDecimalPoint, TextSpan.FromLength(pos - 1, 1));

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
            else return new SyntaxToken(SyntaxTokenKind.Identifier, start, pos - start, tokenText);
        }

        private SyntaxToken LexString()
        {
            int quotePos = pos;
            var quote = Advance();
            int start = pos;

            var done = false;

            while (!done)
            {
                switch (current)
                {
                    case '\0':
                    case '\r':
                    case '\n':
                        diagnostics.ReportSyntaxError(ErrorMessage.NeverClosedStringLiteral, TextSpan.FromBounds(quotePos, pos));
                        var t1 = text.ToString(start, pos - start);
                        return new SyntaxToken(SyntaxTokenKind.String, quotePos, t1.Length, t1, false);
                    default:
                        if (current == quote)
                            done = true;
                        else 
                            pos++;
                        break;
                }
            }
            pos++;
            var t = text.ToString(start, pos - start - 1);
            return new SyntaxToken(SyntaxTokenKind.String, quotePos, text.Length + 2, t);
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
            if (kind != null) return new SyntaxToken((SyntaxTokenKind)kind, (pos += 2) - 2, 2, value);
            return null;
        }

        private SyntaxToken NextToken()
        {
            var doubleChar = LexDoubleChar();
            if (doubleChar != null) return doubleChar;

            var singleChar = LexSingleChar();
            if (singleChar != null) return singleChar;

            if (current == '\0') return new SyntaxToken(SyntaxTokenKind.End, pos, 0, "End");
            else if (current == '"' || current == '\'') return LexString();
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