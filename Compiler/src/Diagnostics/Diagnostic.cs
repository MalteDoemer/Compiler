using System;
using System.Collections.Generic;
using Compiler.Text;

namespace Compiler.Diagnostics
{
    public interface IDiagnostable
    {
        IEnumerable<Diagnostic> GetDiagnostics();
    }

    public class Diagnostic
    {
        public TextSpan Span { get; }
        public string Message { get; }
        public ErrorKind Kind { get; }
        public ErrorLevel Level { get; }
        public bool HasPositon => Span != TextSpan.Invalid;

        public Diagnostic(ErrorKind kind, string message, TextSpan span, ErrorLevel level = ErrorLevel.Error)
        {
            Kind = kind;
            Message = message;
            Span = span;
            Level = level;
        }
    }
}