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
        public string Message { get; }
        public ErrorKind Kind { get; }
        public ErrorLevel Level { get; }
        
        public TextLocation Location { get; }
        public TextSpan Span => Location.Span;
        public bool HasPositon => Span != TextSpan.Undefined;

        public Diagnostic(ErrorKind kind, string message, TextLocation location, ErrorLevel level)
        {
            Kind = kind;
            Message = message;
            Location = location;
            Level = level;
        }
    }
}