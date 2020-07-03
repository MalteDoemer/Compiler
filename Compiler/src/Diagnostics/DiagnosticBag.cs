using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Compiler.Binding;
using Compiler.Syntax;
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
        };

        private readonly List<Diagnostic> builder;

        public DiagnosticBag()
        {
            builder = new List<Diagnostic>();
        }

        public void ReportDiagnostic(ErrorMessage message, TextSpan reportSpan, ErrorKind kind, ErrorLevel level, params object[] values)
        {
            var text = String.Format(ErrorFormats[(int)message], values);
            builder.Add(new Diagnostic(kind, text, reportSpan, level));
        }
        public void ReportSyntaxError(ErrorMessage message, TextSpan reportSpan, params object[] values) => ReportDiagnostic(message, reportSpan, ErrorKind.SyntaxError, ErrorLevel.Error, values);
        public void ReportTypeError(ErrorMessage message, TextSpan reportSpan, params object[] values) => ReportDiagnostic(message, reportSpan, ErrorKind.TypeError, ErrorLevel.Error, values);
        public void ReportIdentifierError(ErrorMessage message, TextSpan reportSpan, params object[] values) => ReportDiagnostic(message, reportSpan, ErrorKind.IdentifierError, ErrorLevel.Error, values);

        public IEnumerator<Diagnostic> GetEnumerator()
        {
            return builder.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return builder.GetEnumerator();
        }
    }
}