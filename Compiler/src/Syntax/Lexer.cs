using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
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
        private char current { get => Peak(0); }
        private char ahead { get => Peak(1); }

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

        private char Peak(int off)
        {
            if (pos + off < text.Length) return text[pos + off];
            else return '\0';
        }

        private SyntaxToken LexSpace()
        {
            var start = pos;
            while (char.IsWhiteSpace(current)) pos++;

            var space = text.ToString(start, pos - start);

            return new SyntaxToken(SyntaxTokenKind.Space, new TextLocation(text, start, pos - start), space);
        }

        private SyntaxToken LexNumber()
        {
            int start = pos;
            int num = 0;

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
                int weight = 1;

                if (!char.IsDigit(current)) diagnostics.ReportError(ErrorMessage.InvalidDecimalPoint, new TextLocation(text, pos - 1, 1));

                while (char.IsDigit(current))
                {
                    weight *= 10;
                    fnum += (double)(current - '0') / (double)weight;
                    pos++;
                }
                return new SyntaxToken(SyntaxTokenKind.Float, new TextLocation(text, start, pos - start), fnum);
            }
            else return new SyntaxToken(SyntaxTokenKind.Int, new TextLocation(text, start, pos - start), num);

        }

        private SyntaxToken LexIdentifierOrKeyword()
        {
            int start = pos;
            while (char.IsLetterOrDigit(current) || current == '_') pos++;

            var tokenText = text.ToString(start, pos - start);
            var isKeyword = SyntaxFacts.IsKeyWord(tokenText);

            if (isKeyword != null)
                return new SyntaxToken((SyntaxTokenKind)isKeyword, new TextLocation(text, start, pos - start), SyntaxFacts.GetKeywordValue(tokenText));
            else return new SyntaxToken(SyntaxTokenKind.Identifier, new TextLocation(text, start, pos - start), tokenText);
        }

        private SyntaxToken LexString()
        {
            var builder = new StringBuilder();
            var quoteStart = pos;
            var quote = Advance();
            var done = false;
            var valid = true;

            while (!done)
            {
                switch (current)
                {
                    case '\0':
                    case '\r':
                    case '\n':
                        diagnostics.ReportError(ErrorMessage.NeverClosedStringLiteral, new TextLocation(text, TextSpan.FromBounds(quoteStart, pos)));
                        valid = false;
                        done = true;
                        break;
                    case '\\':
                        switch (ahead)
                        {
                            case '0':
                                builder.Append('\0');
                                pos += 2;
                                break;

                            case 'n':
                                builder.Append('\n');
                                pos += 2;
                                break;

                            case 'r':
                                builder.Append('\r');
                                pos += 2;
                                break;

                            case 't':
                                builder.Append('\t');
                                pos += 2;
                                break;

                            case '"':
                                builder.Append('\"');
                                pos += 2;
                                break;

                            case '\'':
                                builder.Append('\'');
                                pos += 2;
                                break;

                            case '\\':
                                builder.Append('\\');
                                pos += 2;
                                break;

                            default:
                                if (ahead == '\0')
                                {
                                    diagnostics.ReportError(ErrorMessage.NeverClosedStringLiteral, new TextLocation(text, TextSpan.FromBounds(quoteStart, pos)));
                                    valid = false;
                                    done = true;
                                    break;
                                }

                                var escapeStart = pos;
                                var escapeEnd = pos + 2;
                                var character = ahead;

                                diagnostics.ReportError(ErrorMessage.InvalidEscapeSequence, new TextLocation(text, TextSpan.FromBounds(escapeStart, escapeEnd)), character);
                                pos += 2;
                                valid = false;
                                break;
                        }

                        break;
                    default:
                        if (current == quote)
                        {
                            done = true;
                            pos++;
                            break;
                        }

                        builder.Append(current);
                        pos++;
                        break;
                }
            }
            var t = builder.ToString();
            return new SyntaxToken(SyntaxTokenKind.String, new TextLocation(text, quoteStart, pos - quoteStart), t, valid);
        }

        private SyntaxToken? LexSingleChar()
        {
            var kind = SyntaxFacts.IsSingleCharacter(current);
            if (kind != null)
                return new SyntaxToken((SyntaxTokenKind)kind, new TextLocation(text, pos, 1), Advance());
            return null;
        }

        private SyntaxToken? LexDoubleChar()
        {
            var kind = SyntaxFacts.IsDoubleCharacter(current, ahead);
            string value = "" + current + ahead;
            if (kind != null) return new SyntaxToken((SyntaxTokenKind)kind, new TextLocation(text, (pos += 2) - 2, 2), value);
            return null;
        }

        private SyntaxToken LexComment()
        {
            var start = pos;

            while (!(current == '\0' || current == '\n' || current == '\r')) pos++;

            var comment = text.ToString(start, pos - start);

            return new SyntaxToken(SyntaxTokenKind.Comment, new TextLocation(text, start, pos - start), comment);
        }

        private SyntaxToken NextToken()
        {
            var doubleChar = LexDoubleChar();
            if (doubleChar != null) return doubleChar;

            var singleChar = LexSingleChar();
            if (singleChar != null) return singleChar;

            if (current == '\0') return new SyntaxToken(SyntaxTokenKind.EndOfFile, new TextLocation(text, pos, 0), "End");
            else if (current == '"' || current == '\'') return LexString();
            else if (char.IsNumber(current)) return LexNumber();
            else if (char.IsWhiteSpace(current)) return LexSpace();
            else if (current == '#') return LexComment();
            else if (char.IsLetter(current) || current == '_') return LexIdentifierOrKeyword();
            else return new SyntaxToken(SyntaxTokenKind.Invalid, new TextLocation(text, pos, 1), Advance());
        }

        public IEnumerable<SyntaxToken> Tokenize(bool verbose = false)
        {
            SyntaxToken token;

            do
            {
                token = NextToken();
                var shouldYield = verbose ? true : (token.TokenKind != SyntaxTokenKind.Space && token.TokenKind != SyntaxTokenKind.Comment);
                if (shouldYield)
                    yield return token;
            } while (token.TokenKind != SyntaxTokenKind.EndOfFile);
        }
    }
}
