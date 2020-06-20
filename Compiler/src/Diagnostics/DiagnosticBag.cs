using System.Collections.Generic;
using System.Linq;
using Compiler.Binding;
using Compiler.Syntax;
using Compiler.Text;

namespace Compiler.Diagnostics
{
    public class DiagnosticBag
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


        internal void ReportInvalidDecimalPoint(int pos)
        {
            var span = new TextSpan(pos, 1);
            var d = new Diagnostic(ErrorKind.SyntaxError, "Invalid decimal point", span);
            diagnostics.Add(d);
        }

        internal void ReportUnexpectedToken(SyntaxToken actual, SyntaxTokenKind expected)
        {
            var d = new Diagnostic(ErrorKind.SyntaxError, $"Expected <{expected}> but got <{actual.Kind}>", actual.Span);
            diagnostics.Add(d);
        }

        internal void ReportUnexpectedToken(SyntaxToken token)
        {
            var d = new Diagnostic(ErrorKind.SyntaxError, $"Unexpected token <{token.Kind}>", token.Span);
            diagnostics.Add(d);
        }

        internal void ReportNeverClosedString(int start, int end)
        {
            var span = TextSpan.FromEnd(start, end);
            var d = new Diagnostic(ErrorKind.SyntaxError, "Never closed string literal", span);
            diagnostics.Add(d);
        }

        internal void ReportVariableNotDefined(VariableExpressionSyntax ve)
        {
            var d = new Diagnostic(ErrorKind.InvalidIdentifier, $"The variable <{ve.Name.Value}> is not defined>", ve.Span);
            diagnostics.Add(d);
        }

        internal void ReportUnknownUnaryOperator(BoundUnaryExpression ue)
        {
            var d = new Diagnostic(ErrorKind.InvalidOperator, $"Unknown unary operator <{ue.Op}>", ue.Span);
            diagnostics.Add(d);
        }

        internal void ReportUnknownBinaryOperator(BoundBinaryExpression be)
        {
            var d = new Diagnostic(ErrorKind.InvalidOperator, $"Unknown binary operator <{be.Op}>", be.Span);
            diagnostics.Add(d);
        }

        internal void ReportUnsupportedBinaryOperator(BinaryExpressionSyntax expr, BoundExpression left, BoundExpression right)
        {
            var d = new Diagnostic(ErrorKind.InvalidOperator, $"The Binary operator '{expr.Op.Value}' is unsupported for the operands <{left.ResultType}> and <{right.ResultType}>", expr.Span);
            diagnostics.Add(d);
        }

        internal void ReportUnsupportedUnaryOperator(UnaryExpressionSyntax expr, BoundExpression right)
        {
            var d = new Diagnostic(ErrorKind.InvalidOperator, $"The Unary operator '{expr.Op.Value}' is unsupported for the operand <{right.ResultType}>", expr.Span);
            diagnostics.Add(d);
        }
    }
}