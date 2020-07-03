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

        private SyntaxToken LexStringContent(SyntaxToken quote)
        {
            int start = pos;
            var done = false;

            while (!done)
            {
                switch (current)
                {
                    case '\0':
                    case '\r':
                    case '\n':
                        diagnostics.ReportSyntaxError(ErrorMessage.NeverClosedStringLiteral, TextSpan.FromBounds(quote.Span.Start, pos));
                        var t1 = text.ToString(start, pos - start);
                        return new SyntaxToken(SyntaxTokenKind.String, start, t1.Length, t1, false);
                    default:
                        if (current == (char)quote.Value) done = true;
                        else pos++;
                        break;
                }
            }
            var t = text.ToString(start, pos - start);
            return new SyntaxToken(SyntaxTokenKind.String, start, t.Length, t);
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

        private IEnumerable<SyntaxToken> NextToken()
        {
            var doubleChar = LexDoubleChar();
            if (doubleChar != null) yield return doubleChar;

            var singleChar = LexSingleChar();
            if (singleChar != null) yield return singleChar;

            if (current == '\0') yield return new SyntaxToken(SyntaxTokenKind.End, pos, 0, "End");
            else if (current == '"' || current == '\'')
            {
                var quoteKind = current == '"' ? SyntaxTokenKind.DoubleQuote : SyntaxTokenKind.SingleQuote;
                var startQuote = new SyntaxToken(quoteKind, pos, 1, Advance());
                yield return startQuote;

                var str = LexStringContent(startQuote);
                yield return str;

                if (str.IsValid)
                    yield return new SyntaxToken(quoteKind, pos, 1, Advance());
            }
            else if (char.IsNumber(current)) yield return LexNumber();
            else if (char.IsWhiteSpace(current)) yield return LexSpace();
            else if (current == '#') yield return LexComment();
            else if (char.IsLetter(current) || current == '_') yield return LexIdentifierOrKeyword();
            else yield return new SyntaxToken(SyntaxTokenKind.Invalid, pos, 1, Advance());
        }

        public IEnumerable<SyntaxToken> Tokenize(bool verbose = false)
        {
            bool end = false;

            while (!end)
            {
                foreach (var token in NextToken())
                {
                    if (token.Kind == SyntaxTokenKind.End)
                        end = true;

                    var shouldYield = verbose ? true : (token.Kind != SyntaxTokenKind.Space &&
                                                        token.Kind != SyntaxTokenKind.Comment &&
                                                        token.Kind != SyntaxTokenKind.SingleQuote &&
                                                        token.Kind != SyntaxTokenKind.DoubleQuote);

                    if (shouldYield)
                        yield return token;
                }
            }
        }
    }
}