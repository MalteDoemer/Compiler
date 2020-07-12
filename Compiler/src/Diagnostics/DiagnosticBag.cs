using System;
using System.Collections;
using System.Collections.Generic;
using Compiler.Text;

namespace Compiler.Diagnostics
{
    public class DiagnosticBag : IEnumerable<Diagnostic>
    {
        public static readonly string[] ErrorFormats =
        {
           "Invalid decimal point.",
           "Never closed string literal.",
           "Never closed curly brackets.",
           "Never closed parenthesis.",
           "Expected token: '{0}'",
           "Unexpected token: '{0}'",
           "Cannot resolve identifier '{0}'.",
           "The types '{0}' and '{1}' are incompatible.",
           "Binary operator '{0}' cannot be applied to '{1}' and '{2}'.",
           "Unary operator '{0}' cannot be applied to '{1}'.",
           "Variable '{0}' is already declared",
           "Function '{0}' requires {1} arguments but recived {2}",
           "Expression cannot be void.",
           "The types '{0}' and '{1}' need a explicit conversion.",
           "Cannot convert '{0}' to '{1}'.",
           "Expression isn't a statement.",
           "Duplicated parameter '{0}'.",
           "Function '{0}' is already declared",
           "Statement isn't a global statement.",
           "Cannot assign a value to the constant '{0}'.",
           "The keyword '{0}' can only be used inside a loop.",
           "The keyword 'return' can only be used inside a function.",
           "Not all paths return a value.",
        };

        private readonly List<Diagnostic> builder;
        private readonly SourceText text;

        public DiagnosticBag(SourceText text)
        {
            builder = new List<Diagnostic>();
            this.text = text;
        }

        public void ReportDiagnostic(ErrorMessage message, TextSpan reportSpan, ErrorLevel level, params object[] values)
        {
            var text = String.Format(ErrorFormats[(int)message], values);
            builder.Add(new Diagnostic(text, new TextLocation(this.text, reportSpan), level));
        }

        public void ReportError(ErrorMessage message, TextSpan span, params object[] values) => ReportDiagnostic(message, span, ErrorLevel.Error, values);
        public void ReportWarning(ErrorMessage message, TextSpan span, params object[] values) => ReportDiagnostic(message, span, ErrorLevel.Warning, values);

        public IEnumerator<Diagnostic> GetEnumerator() => builder.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => builder.GetEnumerator();
    }
}