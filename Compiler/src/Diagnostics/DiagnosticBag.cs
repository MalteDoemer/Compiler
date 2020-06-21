using System;
using System.Collections.Generic;
using System.Linq;
using Compiler.Binding;
using Compiler.Syntax;
using Compiler.Text;

namespace Compiler.Diagnostics
{
    public enum SpecificErroKind
    {
        InvalidDecimalPoint,
        UnexpectedToken2,
        UnexpectedToken1,
        NeverClosedString,
        VariableNotDeclared,
        UnsupportedBinaryOperator,
        UnsupportedUnaryOperator,
        NeverClosedCurlyBrackets,
        WrongType,
        VariableAlreadyDeclared,
    }

    public class DiagnosticBag
    {
        public static readonly string[] Formats = {
           "Invalid decimal point.",
           "Expected <{0}> but got <{1}>.",
           "Unexpected token <{0}>.",
           "Never closed string literal.",
           "The variable '{0}' is not declared.",
           "The Binary operator '{0}' is unsupported for the operands <{1}> and <{2}>.",
           "The Unary operator '{0}' is unsupported for the operand <{1}>.",
           "Never closed curly brackets.",
           "The types <{0}> and <{1}> don't match.",
           "The variable '{0}' is already declared.",
       };



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
            var span = new TextSpan(pos - 1, 1);
            var text = string.Format(Formats[(int)SpecificErroKind.InvalidDecimalPoint]);
            diagnostics.Add(new Diagnostic(ErrorKind.SyntaxError, text, span));
        }

        internal void ReportUnexpectedToken(SyntaxTokenKind actual, SyntaxTokenKind expected, TextSpan span)
        {
            var text = string.Format(Formats[(int)SpecificErroKind.UnexpectedToken2], expected, actual);
            diagnostics.Add(new Diagnostic(ErrorKind.SyntaxError, text, span));
        }

        internal void ReportUnexpectedToken(SyntaxTokenKind kind, TextSpan span)
        {
            var text = string.Format(Formats[(int)SpecificErroKind.UnexpectedToken1], kind);
            diagnostics.Add(new Diagnostic(ErrorKind.SyntaxError, text, span));
        }

        internal void ReportNeverClosedString(int start, int end)
        {
            var span = TextSpan.FromBounds(start, end);
            var text = string.Format(Formats[(int)SpecificErroKind.NeverClosedString]);
            diagnostics.Add(new Diagnostic(ErrorKind.SyntaxError, text, span));
        }

        internal void ReportVariableNotDeclared(string name, TextSpan span)
        {
            var text = string.Format(Formats[(int)SpecificErroKind.VariableNotDeclared], name);
            diagnostics.Add(new Diagnostic(ErrorKind.SyntaxError, text, span));
        }

        internal void ReportUnsupportedBinaryOperator(dynamic op, TypeSymbol left, TypeSymbol right, TextSpan span)
        {
           var text = string.Format(Formats[(int)SpecificErroKind.UnsupportedBinaryOperator], op.ToString(), left, right);
            diagnostics.Add(new Diagnostic(ErrorKind.SyntaxError, text, span));
        }

        internal void ReportUnsupportedUnaryOperator(dynamic op, TypeSymbol right, TextSpan span)
        {
            var text = string.Format(Formats[(int)SpecificErroKind.UnsupportedUnaryOperator], op.ToString(), right);
            diagnostics.Add(new Diagnostic(ErrorKind.SyntaxError, text, span));
        }

        internal void ReportNeverClosedCurlyBrackets(TextSpan span)
        {
            var text = string.Format(Formats[(int)SpecificErroKind.NeverClosedCurlyBrackets]);
            diagnostics.Add(new Diagnostic(ErrorKind.SyntaxError, text, span));
        }

        internal void ReportWrongType(TypeSymbol expected, TypeSymbol provided, TextSpan span)
        {
            var text = string.Format(Formats[(int)SpecificErroKind.WrongType], expected, provided);
            diagnostics.Add(new Diagnostic(ErrorKind.SyntaxError, text, span));
        }

        internal void ReportVariableAlreadyDeclared(string identifier, TextSpan span)
        {
            var text = string.Format(Formats[(int)SpecificErroKind.VariableAlreadyDeclared], identifier);
            diagnostics.Add(new Diagnostic(ErrorKind.SyntaxError, text, span));
        }
    }
}