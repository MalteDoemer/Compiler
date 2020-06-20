using System;
using System.Collections;

namespace Compiler.Diagnostics
{
    public class Diagnostic
    {
        public ErrorKind Kind { get; }
        public string Message { get; }
        public int Position { get; }
        public ErrorLevel Level { get; }
        public bool HasPositon => Position >= 0;

        public Diagnostic(ErrorKind kind, string message, int pos = -1, ErrorLevel level = ErrorLevel.Error)
        {
            Kind = kind;
            Message = message;
            Position = pos;
            Level = level;
        }

        public override string ToString()
        {
            if (HasPositon) return $"{Kind} at {Position}\n{Message}";
            else return $"{Kind}: {Message}";
        }

    }
}