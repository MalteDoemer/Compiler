using System;
using System.CodeDom.Compiler;
using System.IO;
using Compiler.Binding;
using Compiler.Diagnostics;
using Compiler.Lowering;
using Compiler.Symbols;
using Compiler.Syntax;

namespace Compiler.Text
{
    public static class TextWriterExtensions
    {
        internal static bool IsConsole(this TextWriter writer)
        {
            if (writer == Console.Out) return !Console.IsOutputRedirected;
            if (writer == Console.Error) return !Console.IsOutputRedirected && !Console.IsErrorRedirected;
            if (writer is IndentedTextWriter iw) return iw.InnerWriter.IsConsole();
            return false;
        }

        internal static void SetForeground(this TextWriter writer, ConsoleColor color)
        {
            if (writer.IsConsole())
                Console.ForegroundColor = color;
        }

        internal static void ResetColor(this TextWriter writer)
        {
            if (writer.IsConsole())
                Console.ResetColor();
        }

        public static void ColorWrite(this TextWriter writer, string value, ConsoleColor color = ConsoleColor.Gray)
        {
            writer.SetForeground(color);
            writer.Write(value);
            writer.ResetColor();
        }

        public static void ColorWrite(this TextWriter writer, object value, ConsoleColor color = ConsoleColor.Gray)
        {
            writer.SetForeground(color);
            writer.Write(value);
            writer.ResetColor();
        }

        public static void WriteColorizedText(this TextWriter writer, ColorizedText text)
        {
            foreach (var span in text)
            {
                writer.SetForeground(span.Color);
                writer.Write(text.ToString(span.Span));
            }
            writer.ResetColor();
        }

        public static void WriteDiagnostic(this TextWriter writer, Diagnostic diagnostic)
        {
            var reportColor = diagnostic.Level == ErrorLevel.Error ? ConsoleColor.Red : ConsoleColor.Yellow;
            var errType = diagnostic.Level == ErrorLevel.Error ? "Error" : "Warning";

            if (diagnostic.HasPositon)
            {
                var lineStart = diagnostic.Location.StartLine;
                var lineEnd = diagnostic.Location.EndLine;
                var columnStart = diagnostic.Location.StartCharacter + 1;
                var columnEnd = diagnostic.Location.EndCharacter + 1;
                var file = diagnostic.Location.Text.File ?? "<string>";

                writer.WriteLine();
                writer.ColorWrite($"{errType} in {file} line [{lineStart},{lineEnd}] column [{columnStart},{columnEnd}]: {diagnostic.Message}", reportColor);
                writer.WriteLine();

            }
            else writer.ColorWrite($"{errType}: {diagnostic.Message}\n", ConsoleColor.Red);
        }

        public static void WriteDiagnosticReport(this TextWriter writer, DiagnosticReport report)
        {
            foreach (var d in report)
                writer.WriteDiagnostic(d);
        }

        internal static void WriteControlFlowGraph(this IndentedTextWriter writer, ControlFlowGraph graph)
        {
            graph.WriteTo(writer);
        }


        internal static void WriteNumber(this IndentedTextWriter writer, object val) => writer.ColorWrite(val, ConsoleColor.DarkGreen);
        internal static void WriteVariable(this IndentedTextWriter writer, string name) => writer.ColorWrite(name, ConsoleColor.Cyan);
        internal static void WriteFunction(this IndentedTextWriter writer, string name) => writer.ColorWrite(name, ConsoleColor.Yellow);
        internal static void WriteString(this IndentedTextWriter writer, string content) => writer.ColorWrite($"\"{content}\"", ConsoleColor.DarkCyan);
        internal static void WriteMagentaKeyword(this IndentedTextWriter writer, string keyword) => writer.ColorWrite(keyword, ConsoleColor.Magenta);
        internal static void WriteBlueKeyword(this IndentedTextWriter writer, string keyword) => writer.ColorWrite(keyword, ConsoleColor.Blue);
        internal static void WriteSpace(this IndentedTextWriter writer, int len = 1) => writer.Write(new string(' ', len));
    }

    public static class BoundNodePrinter
    {
        internal static void WriteBoundNode(this TextWriter writer, BoundNode node)
        {
            if (writer is IndentedTextWriter indented) WriteBoundNodeInternal(indented, node);
            else
            {
                var indentedWriter = new IndentedTextWriter(writer, " ");
                WriteBoundNodeInternal(indentedWriter, node);
            }
        }

        private static void WriteBoundNodeInternal(this IndentedTextWriter writer, BoundNode node)
        {
            switch (node.Kind)
            {
                case BoundNodeKind.BoundProgram:
                    WriteBoundProgram(writer, (BoundProgram)node); break;
                case BoundNodeKind.BoundLiteralExpression:
                    WriteBoundLiteralExpression(writer, (BoundLiteralExpression)node); break;
                case BoundNodeKind.BoundVariableExpression:
                    WriteBoundVariableExpression(writer, (BoundVariableExpression)node); break;
                case BoundNodeKind.BoundUnaryExpression:
                    WriteBoundUnaryExpression(writer, (BoundUnaryExpression)node); break;
                case BoundNodeKind.BoundBinaryExpression:
                    WriteBoundBinaryExpression(writer, (BoundBinaryExpression)node); break;
                case BoundNodeKind.BoundCallExpression:
                    WriteBoundCallExpression(writer, (BoundCallExpression)node); break;
                case BoundNodeKind.BoundConversionExpression:
                    WriteBoundConversionExpression(writer, (BoundConversionExpression)node); break;
                case BoundNodeKind.BoundAssignmentExpression:
                    WriteBoundAssignmentExpression(writer, (BoundAssignmentExpression)node); break;
                case BoundNodeKind.BoundBlockStatement:
                    WriteBoundBlockStatement(writer, (BoundBlockStatement)node); break;
                case BoundNodeKind.BoundExpressionStatement:
                    WriteBoundExpressionStatement(writer, (BoundExpressionStatement)node); break;
                case BoundNodeKind.BoundVariableDeclarationStatement:
                    WriteBoundVariableDeclarationStatement(writer, (BoundVariableDeclarationStatement)node); break;
                case BoundNodeKind.BoundConditionalGotoStatement:
                    WriteBoundConditionalGotoStatement(writer, (BoundConditionalGotoStatement)node); break;
                case BoundNodeKind.BoundGotoStatement:
                    WriteBoundGotoStatement(writer, (BoundGotoStatement)node); break;
                case BoundNodeKind.BoundLabelStatement:
                    WriteBoundLabelStatement(writer, (BoundLabelStatement)node); break;
                case BoundNodeKind.BoundReturnStatement:
                    WriteBoundReturnStatement(writer, (BoundReturnStatement)node); break;
                case BoundNodeKind.BoundStatementExpression:
                    WriteBoundStatementExpression(writer, (BoundStatementExpression)node); break;
                case BoundNodeKind.BoundNopStatement:
                    writer.WriteBlueKeyword("nop"); break;
                default: throw new Exception("Unexpected kind");
            }
        }

        private static void WriteBoundProgram(this IndentedTextWriter writer, BoundProgram node)
        {
            foreach (var pair in node.Functions)
            {
                writer.WriteFunctionSymbol(pair.Key);
                writer.WriteLine();
                writer.WriteBoundBlockStatement(pair.Value);
                writer.WriteLine();
                writer.WriteLine();
            }
        }

        private static void WriteFunctionSymbol(this IndentedTextWriter writer, FunctionSymbol symbol)
        {
            writer.WriteBlueKeyword("func");
            writer.WriteSpace();
            writer.WriteFunction(symbol.Name);
            writer.ColorWrite("(");

            for (int i = 0; i < symbol.Parameters.Length; i++)
            {
                var parameter = symbol.Parameters[i];

                writer.WriteVariable(parameter.Name);
                writer.ColorWrite(":");
                writer.WriteSpace();
                writer.WriteBlueKeyword(parameter.Type.Name);

                if (i != symbol.Parameters.Length - 1)
                    writer.ColorWrite(", ");
            }

            writer.ColorWrite(")");
        }

        private static void WriteBoundLiteralExpression(this IndentedTextWriter writer, BoundLiteralExpression node)
        {
            switch (node.Value)
            {
                case int i:
                case float f:
                    writer.WriteNumber(node.Value); break;
                case bool b:
                    writer.WriteBlueKeyword(b.ToString().ToLower()); break;
                case string s:
                    writer.WriteStringWithoutEscapeSequences(s);
                    break;
            }
        }

        private static void WriteStringWithoutEscapeSequences(this IndentedTextWriter writer, string s)
        {
            s = s.Replace("\\", "\\\\");
            s = s.Replace("\0", "\\0");
            s = s.Replace("\n", "\\n");
            s = s.Replace("\r", "\\r");
            s = s.Replace("\t", "\\t");
            s = s.Replace("\"", "\\\"");
            s = s.Replace("\'", "\\'");
            writer.WriteString(s);
        }

        private static void WriteBoundVariableExpression(this IndentedTextWriter writer, BoundVariableExpression node)
        {
            writer.WriteVariable(node.Variable!.Name);
        }

        private static void WriteBoundUnaryExpression(this IndentedTextWriter writer, BoundUnaryExpression node)
        {
            writer.ColorWrite("(");
            writer.Write(node.Op.GetText());
            writer.WriteBoundNode(node.Expression);
            writer.ColorWrite(")");
        }

        private static void WriteBoundBinaryExpression(this IndentedTextWriter writer, BoundBinaryExpression node)
        {
            writer.ColorWrite("(");
            writer.WriteBoundNode(node.Left);
            writer.WriteSpace();
            writer.ColorWrite(node.Op.GetText());
            writer.WriteSpace();
            writer.WriteBoundNode(node.Right);
            writer.ColorWrite(")");
        }

        private static void WriteBoundCallExpression(this IndentedTextWriter writer, BoundCallExpression node)
        {
            writer.WriteFunction(node.Symbol!.Name);
            writer.Write('(');

            for (var i = 0; i < node.Arguments.Length; i++)
            {
                writer.WriteBoundNode(node.Arguments[i]);
                if (i < node.Arguments.Length - 1)
                    writer.Write(", ");
            }

            writer.Write(')');
        }

        private static void WriteBoundConversionExpression(this IndentedTextWriter writer, BoundConversionExpression node)
        {
            writer.WriteBlueKeyword(node.ResultType.Name);
            writer.ColorWrite('(');
            writer.WriteBoundNode(node.Expression);
            writer.ColorWrite(')');
        }

        private static void WriteBoundAssignmentExpression(this IndentedTextWriter writer, BoundAssignmentExpression node)
        {
            writer.WriteVariable(node.Variable!.Name);
            writer.WriteSpace();
            writer.ColorWrite("=");
            writer.WriteSpace();
            writer.WriteBoundNode(node.Expression);
        }

        private static void WriteBoundBlockStatement(this IndentedTextWriter writer, BoundBlockStatement node)
        {

            writer.ColorWrite("{");
            writer.Indent += 4;
            foreach (var n in node.Statements)
            {
                writer.WriteLine();
                writer.WriteBoundNode(n);
            }
            writer.Indent -= 4;
            writer.WriteLine();
            writer.ColorWrite("}");
        }

        private static void WriteBoundExpressionStatement(this IndentedTextWriter writer, BoundExpressionStatement node)
        {
            writer.WriteBoundNode(node.Expression);
        }

        private static void WriteBoundVariableDeclarationStatement(this IndentedTextWriter writer, BoundVariableDeclarationStatement node)
        {
            writer.WriteBlueKeyword("var");
            writer.WriteSpace();
            writer.WriteVariable(node.Variable.Name);
            writer.WriteSpace();
            writer.ColorWrite("=");
            writer.WriteSpace();
            writer.WriteBoundNode(node.Expression);
        }

        private static void WriteBoundConditionalGotoStatement(this IndentedTextWriter writer, BoundConditionalGotoStatement node)
        {
            writer.WriteMagentaKeyword("goto");
            writer.WriteSpace();
            writer.ColorWrite(node.Label.Identifier);
            writer.WriteSpace();

            if (node.JumpIfFalse)
                writer.WriteMagentaKeyword("unless");
            else
                writer.WriteMagentaKeyword("if");

            writer.WriteSpace();
            writer.WriteBoundNode(node.Condition);
        }

        private static void WriteBoundGotoStatement(this IndentedTextWriter writer, BoundGotoStatement node)
        {
            writer.WriteMagentaKeyword("goto");
            writer.WriteSpace();
            writer.ColorWrite(node.Label.Identifier);
        }

        private static void WriteBoundLabelStatement(this IndentedTextWriter writer, BoundLabelStatement node)
        {
            if (writer is IndentedTextWriter iw)
            {
                iw.Indent -= 4;
                iw.ColorWrite(node.Label.Identifier + ":", ConsoleColor.DarkGray);
                iw.Indent += 4;
            }
            else
            {
                var iw2 = new IndentedTextWriter(writer);
                iw2.Indent -= 4;
                iw2.ColorWrite(node.Label.Identifier + ":", ConsoleColor.DarkGray);
                iw2.Indent += 4;
            }
        }

        private static void WriteBoundReturnStatement(this IndentedTextWriter writer, BoundReturnStatement node)
        {
            writer.WriteMagentaKeyword("return");
            writer.WriteSpace();
            if (node.Expression is null)
                writer.WriteBlueKeyword("void");
            else writer.WriteBoundNode(node.Expression);
        }

        private static void WriteBoundStatementExpression(IndentedTextWriter writer, BoundStatementExpression node)
        {
            var statements = Lowerer.Flatten(FunctionSymbol.Invalid, node.Statement).Statements;
            writer.Indent += 4;
            foreach (var stmt in statements)
            {
                writer.WriteBoundNode(stmt);
                writer.WriteLine();
            }
            writer.Indent -= 4;
        }
    }
}