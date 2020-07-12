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
        public ErrorLevel Level { get; }
        
        public TextLocation Location { get; }
        public TextSpan Span => Location.Span;
        public bool HasPositon => Span != TextSpan.Undefined;

        public Diagnostic(string message, TextLocation location, ErrorLevel level)
        {
            Message = message;
            Location = location;
            Level = level;
        }
    }
}