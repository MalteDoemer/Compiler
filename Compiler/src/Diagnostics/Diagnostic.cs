using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Compiler.Diagnostics
{

    internal enum ErrorLevel
    {
        Warning,
        Error,
    }

    internal enum ErrorKind
    {
        SyntaxError,
    }

    internal class Diagnostic
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

    internal class DiagnosticBag
    {
        private readonly List<Diagnostic> diagnostics;

        public int Count => diagnostics.Count;
        public int Errors => diagnostics.Where(d => d.Level == ErrorLevel.Error).Count();
        public int Warnings => diagnostics.Where(d => d.Level == ErrorLevel.Warning).Count();

        public DiagnosticBag()
        {
            diagnostics = new List<Diagnostic>();
        }

        public Diagnostic[] ToArray() => diagnostics.ToArray();
        public IEnumerable<Diagnostic> GetDiagnostics() => diagnostics;
        public IEnumerable<Diagnostic> GetErrors() => diagnostics.Where(d => d.Level == ErrorLevel.Error);
        public IEnumerable<Diagnostic> GetWarnings() => diagnostics.Where(d => d.Level == ErrorLevel.Warning);


        public void ReportInvalidDecimalPoint(int pos)
        {
            var d = new Diagnostic(ErrorKind.SyntaxError, "Invalid decimal point", pos);
            diagnostics.Add(d);
        }

        public void ReportNeverClosedString(int pos)
        {
            var d = new Diagnostic(ErrorKind.SyntaxError, "Never closed string literal", pos);
            diagnostics.Add(d);
        }

    }
}