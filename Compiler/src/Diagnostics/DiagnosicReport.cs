using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Compiler.Diagnostics
{
    public class DiagnosticReport : IEnumerable<Diagnostic>
    {
        public DiagnosticReport(IEnumerable<Diagnostic> diagnostics)
        {
            if (diagnostics is ImmutableArray<Diagnostic> immutable)
                Diagnostics = immutable;
            else
                Diagnostics = diagnostics.ToImmutableArray();
        }

        public ImmutableArray<Diagnostic> Diagnostics { get; }

        public IEnumerable<Diagnostic> Errors { get => Diagnostics.Where(d => d.Level == ErrorLevel.Error); }
        public IEnumerable<Diagnostic> Warnings { get => Diagnostics.Where(d => d.Level == ErrorLevel.Warning); }

        public bool HasErrors { get => Errors.Any(); }
        public bool HasWarnings { get => Warnings.Any(); }

        public Diagnostic this[int i] { get => Diagnostics[i]; }
        public int Length => Diagnostics.Length;


        public IEnumerator<Diagnostic> GetEnumerator() { foreach (var d in Diagnostics) yield return d; }
        IEnumerator IEnumerable.GetEnumerator() { foreach (var d in Diagnostics) yield return d; }
    }
}