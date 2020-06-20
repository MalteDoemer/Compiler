using System.Collections.Generic;
using System.Linq;
using Compiler.Binding;
using Compiler.Syntax;

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
            var d = new Diagnostic(ErrorKind.SyntaxError, "Invalid decimal point", pos);
            diagnostics.Add(d);
        }

        internal void ReportUnexpectedToken(SyntaxToken actual, SyntaxTokenKind expected)
        {
            var d = new Diagnostic(ErrorKind.SyntaxError, $"Expected <{expected}> but got <{actual.kind}>", actual.pos);
            diagnostics.Add(d);
        }

        internal void ReportUnexpectedToken(SyntaxToken token)
        {
            var d = new Diagnostic(ErrorKind.SyntaxError, $"Unexpected token <{token.kind}>", token.pos);
            diagnostics.Add(d);
        }

        internal void ReportNeverClosedString(int pos)
        {
            var d = new Diagnostic(ErrorKind.SyntaxError, "Never closed string literal", pos);
            diagnostics.Add(d);
        }

        internal void ReportVariableNotDefined(VariableExpressionSyntax ve)
        {
            var d = new Diagnostic(ErrorKind.RuntimeError, $"The variable <{ve.Name.value}> is not defined>", ve.Pos);
            diagnostics.Add(d);
        }

        internal void ReportUnknownUnaryOperator(BoundUnaryExpression ue)
        {
            var d = new Diagnostic(ErrorKind.RuntimeError, $"Unknown unary operator <{ue.Op}>", ue.Pos);
            diagnostics.Add(d);
        }

        internal void ReportUnknownBinaryOperator(BoundBinaryExpression be)
        {
            var d = new Diagnostic(ErrorKind.RuntimeError, $"Unknown binary operator <{be.Op}>", be.Pos);
            diagnostics.Add(d);
        }

        internal void ReportUnsupportedBinaryOperator(SyntaxToken op, BoundExpression left, BoundExpression right)
        {
            var d = new Diagnostic(ErrorKind.RuntimeError, $"The Binary operator '{op.value}' is unsupported for the operands <{left.ResultType}> and <{right.ResultType}>", op.pos);
            diagnostics.Add(d);
        }

        internal void ReportUnsupportedUnaryOperator(SyntaxToken op, BoundExpression right)
        {
            var d = new Diagnostic(ErrorKind.RuntimeError, $"The Unary operator '{op.value}' is unsupported for the operand <{right.ResultType}>", op.pos);
            diagnostics.Add(d);
        }
    }
}