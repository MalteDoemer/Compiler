using System;
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
            var span = new TextSpan(pos -1, 1);
            var d = new Diagnostic(ErrorKind.SyntaxError, "Invalid decimal point.", span);
            diagnostics.Add(d);
        }

        internal void ReportUnexpectedToken(SyntaxTokenKind actual, SyntaxTokenKind expected, TextSpan span)
        {
            var d = new Diagnostic(ErrorKind.SyntaxError, $"Expected <{expected}> but got <{actual}>.", span);
            diagnostics.Add(d);
        }

        internal void ReportUnexpectedToken(SyntaxTokenKind kind, TextSpan span)
        {
            var d = new Diagnostic(ErrorKind.SyntaxError, $"Unexpected token <{kind}>.", span);
            diagnostics.Add(d);
        }

        internal void ReportNeverClosedString(int start, int end)
        {
            var span = TextSpan.FromBounds(start, end);
            var d = new Diagnostic(ErrorKind.SyntaxError, "Never closed string literal.", span);
            diagnostics.Add(d);
        }

        internal void ReportVariableNotDeclared(string name, TextSpan span)
        {
            var d = new Diagnostic(ErrorKind.IdentifierNotFound, $"The variable \"{name}\" is not defined.", span);
            diagnostics.Add(d);
        }

        internal void ReportUnsupportedBinaryOperator(string op, TypeSymbol left, TypeSymbol right, TextSpan span)
        {
            var d = new Diagnostic(ErrorKind.TypeError, $"The Binary operator '{op}' is unsupported for the operands <{left}> and <{right}>.", span);
            diagnostics.Add(d);
        }

        internal void ReportUnsupportedUnaryOperator(string op, TypeSymbol right, TextSpan span)
        {
            var d = new Diagnostic(ErrorKind.TypeError, $"The Unary operator '{op}' is unsupported for the operand <{right}>.", span);
            diagnostics.Add(d);
        }

        internal void ReportWrongType(TypeSymbol expected, TypeSymbol porvided, TextSpan span)
        {
            var d = new Diagnostic(ErrorKind.TypeError, $"The types <{expected}> and <{porvided}> don't match.", span);
            diagnostics.Add(d);
        }
    }
}