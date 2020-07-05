using System.Collections.Generic;
using System.Collections.Immutable;
using Compiler.Diagnostics;
using Compiler.Text;

namespace Compiler.Syntax
{
    internal class Lexer : IDiagnostable
    {
        private readonly DiagnosticBag diagnostics;
        private readonly SourceText text;
        private readonly bool isScript;
        private int pos;
        private char current { get => pos < text.Length ? text[pos] : '\0'; }
        private char ahead { get => pos + 1 < text.Length ? text[pos + 1] : '\0'; }

        public Lexer(SourceText text, bool isScript)
        {
            this.text = text;
            this.isScript = isScript;
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
            var start = pos;
            while (char.IsWhiteSpace(current)) pos++;

            var space = text.ToString(start, pos - start);

            return new SyntaxToken(SyntaxTokenKind.Space, start, pos - start, space);
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
            var quoteStart = pos;
            var quote = Advance();

            int textStart = pos;
            var done = false;

            while (!done)
            {
                switch (current)
                {
                    case '\0':
                    case '\r':
                    case '\n':
                        diagnostics.ReportSyntaxError(ErrorMessage.NeverClosedStringLiteral, TextSpan.FromBounds(quoteStart, pos));
                        var t1 = text.ToString(textStart, pos - textStart);
                        return new SyntaxToken(SyntaxTokenKind.String, quoteStart, pos - quoteStart, t1, false);
                    default:
                        if (current == quote) done = true;

                        pos++;
                        break;
                }
            }
            var t = text.ToString(textStart, pos - textStart - 1);
            return new SyntaxToken(SyntaxTokenKind.String, quoteStart, pos - quoteStart, t);
        }

        private SyntaxToken LexSingleChar()
        {
            var kind = SyntaxFacts.IsSingleCharacter(current);
            if (kind != null)
            {

                return new SyntaxToken((SyntaxTokenKind)kind, pos, 1, Advance());
            }
            return null;
        }

        private SyntaxToken LexDoubleChar()
        {
            var kind = SyntaxFacts.IsDoubleCharacter(current, ahead);
            string value = "" + current + ahead;
            if (kind != null) return new SyntaxToken((SyntaxTokenKind)kind, (pos += 2) - 2, 2, value);
            return null;
        }

        private SyntaxToken LexComment()
        {
            var start = pos;

            while (!(current == '\0' || current == '\n' || current == '\r')) pos++;

            var comment = text.ToString(start, pos - start);

            return new SyntaxToken(SyntaxTokenKind.Comment, start, pos - start, comment);
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
            else if (current == '#') return LexComment();
            else if (char.IsLetter(current) || current == '_') return LexIdentifierOrKeyword();
            else return new SyntaxToken(SyntaxTokenKind.Invalid, pos, 1, Advance());
        }

        public IEnumerable<SyntaxToken> Tokenize(bool verbose = false)
        {
            SyntaxToken token;

            do
            {
                token = NextToken();
                var shouldYield = verbose ? true : (token.Kind != SyntaxTokenKind.Space &&
                                                    token.Kind != SyntaxTokenKind.Comment &&
                                                    token.Kind != SyntaxTokenKind.SingleQuote &&
                                                    token.Kind != SyntaxTokenKind.DoubleQuote);
                if (shouldYield)
                    yield return token;
            } while (token.Kind != SyntaxTokenKind.End);
        }
    }
}
