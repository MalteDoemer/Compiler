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
           "Cannot assing to read-only variable '{0}'",
           "The keyword '{0}' can only be used inside a loop.",
           "The keyword 'return' can only be used inside a function.",
           "Not all paths return a value.",
           "The reference '{0}' is not a valid .NET assembly.",
           "The required type {0} cannot be resolved in the given references.",
           "The required type {0} was found in multiple references: {1}",
           "The required method {0} cannot be resolved in the given references.",
           "The required method {0} was found in multiple references: {1}",
           "Invalid escape sequence: '{0}'"
        };

        private readonly List<Diagnostic> builder;
        
        public DiagnosticBag()
        {
            builder = new List<Diagnostic>();
        }

        public void ReportDiagnostic(ErrorMessage message, TextLocation location, ErrorLevel level, params object[] values)
        {
            var text = String.Format(ErrorFormats[(int)message], values);
            builder.Add(new Diagnostic(text, location, level));
        }

        public void ReportError(ErrorMessage message, TextLocation location, params object[] values) => ReportDiagnostic(message, location, ErrorLevel.Error, values);
        public void ReportWarning(ErrorMessage message, TextLocation location, params object[] values) => ReportDiagnostic(message, location, ErrorLevel.Warning, values);

        public IEnumerator<Diagnostic> GetEnumerator() => builder.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => builder.GetEnumerator();
    }
}