using System;
using System.CodeDom.Compiler;
using System.IO;
using Compiler.Binding;
using Compiler.Diagnostics;
using Compiler.Syntax;

namespace Compiler.Text
{
    public static class TextWriterExtensions
    {
        private static bool IsConsole(this TextWriter writer)
        {
            if (writer == Console.Out) return !Console.IsOutputRedirected;
            if (writer == Console.Error) return !Console.IsOutputRedirected && !Console.IsErrorRedirected;
            if (writer is IndentedTextWriter iw) return iw.InnerWriter.IsConsole();
            return false;
        }

        private static void SetForeground(this TextWriter writer, ConsoleColor color)
        {
            if (writer.IsConsole())
                Console.ForegroundColor = color;
        }

        private static void ResetColor(this TextWriter writer)
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
                //var src = diagnostic.Location.Text;
                //var prefix = src.ToString(0, diagnostic.Span.Start);
                //var errorText = src.ToString(diagnostic.Span);
                //var postfix = src.ToString(diagnostic.Span.End, src.Length - diagnostic.Span.End);

                var lineStart = diagnostic.Location.StartLine;
                var lineEnd = diagnostic.Location.EndLine;
                var columnStart = diagnostic.Location.StartCharacter + 1;
                var columnEnd = diagnostic.Location.EndCharacter + 1;
                var file = diagnostic.Location.Text.File ?? "<string>";

                writer.WriteLine();
                writer.ColorWrite($"{errType} in {file} line [{lineStart},{lineEnd}] column [{columnStart},{columnEnd}]: {diagnostic.Message}", reportColor);
                writer.WriteLine();

                // writer.ColorWrite($"\n\nError in {file} line {linenum} column {charOff}\n\n");
                // writer.ColorWrite(prefix);
                // writer.ColorWrite(errorText, ConsoleColor.Red);
                // writer.ColorWrite(postfix);
                // writer.ColorWrite($"\n\n{diagnostic.Message}\n\n");

            }
            else writer.ColorWrite($"{errType}: {diagnostic.Message}\n", ConsoleColor.Red);
        }

        public static void WriteDiagnosticReport(this TextWriter writer, DiagnosticReport report)
        {
            foreach (var d in report)
                writer.WriteDiagnostic(d);
        }

        internal static void WriteControlFlowGraph(this TextWriter writer, ControlFlowGraph graph)
        {
            graph.WriteTo(writer);
        }

        internal static void WriteBoundNode(this TextWriter writer, BoundNode node)
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
                case BoundNodeKind.BoundIfStatement:
                    WriteBoundIfStatement(writer, (BoundIfStatement)node); break;
                case BoundNodeKind.BoundForStatement:
                    WriteBoundForStatement(writer, (BoundForStatement)node); break;
                case BoundNodeKind.BoundWhileStatement:
                    WriteBoundWhileStatement(writer, (BoundWhileStatement)node); break;
                case BoundNodeKind.BoundDoWhileStatement:
                    WriteBoundDoWhileStatement(writer, (BoundDoWhileStatement)node); break;
                case BoundNodeKind.BoundConditionalGotoStatement:
                    WriteBoundConditionalGotoStatement(writer, (BoundConditionalGotoStatement)node); break;
                case BoundNodeKind.BoundGotoStatement:
                    WriteBoundGotoStatement(writer, (BoundGotoStatement)node); break;
                case BoundNodeKind.BoundLabelStatement:
                    WriteBoundLabelStatement(writer, (BoundLabelStatement)node); break;
                case BoundNodeKind.BoundInvalidExpression:
                    WriteBoundInvalidExpression(writer, (BoundInvalidExpression)node); break;
                case BoundNodeKind.BoundReturnStatement:
                    WriteBoundReturnStatement(writer, (BoundReturnStatement)node); break;
                default: throw new Exception("Unexpected kind");
            }
        }

        private static void WriteBoundProgram(this TextWriter writer, BoundProgram node)
        {
            throw new NotImplementedException();
            //writer.WriteBoundNode(node.GlobalStatements);
            //writer.WriteLine();
        }

        private static void WriteBoundLiteralExpression(this TextWriter writer, BoundLiteralExpression node)
        {
            switch (node.Value)
            {
                case int i:
                case float f:
                    writer.WriteNumber(node.Value); break;
                case bool b:
                    writer.WriteBlueKeyword(b.ToString().ToLower()); break;
                case string s:
                    writer.WriteString(s); break;
            }
        }

        private static void WriteBoundVariableExpression(this TextWriter writer, BoundVariableExpression node)
        {
            writer.WriteVariable(node.Variable.Name);
        }

        private static void WriteBoundUnaryExpression(this TextWriter writer, BoundUnaryExpression node)
        {
            writer.ColorWrite("(");
            writer.Write(node.Op.GetText());
            writer.WriteBoundNode(node.Expression);
            writer.ColorWrite(")");
        }

        private static void WriteBoundBinaryExpression(this TextWriter writer, BoundBinaryExpression node)
        {
            writer.ColorWrite("(");
            writer.WriteBoundNode(node.Left);
            writer.WriteSpace();
            writer.ColorWrite(node.Op.GetText());
            writer.WriteSpace();
            writer.WriteBoundNode(node.Right);
            writer.ColorWrite(")");
        }

        private static void WriteBoundCallExpression(this TextWriter writer, BoundCallExpression node)
        {
            writer.WriteFunction(node.Symbol.Name);
            writer.Write('(');

            for (var i = 0; i < node.Arguments.Length; i++)
            {
                writer.WriteBoundNode(node.Arguments[i]);
                if (i < node.Arguments.Length - 1)
                    writer.Write(", ");
            }

            writer.Write(')');
        }

        private static void WriteBoundConversionExpression(this TextWriter writer, BoundConversionExpression node)
        {
            writer.WriteBlueKeyword(node.Type.Name);
            writer.ColorWrite('(');
            writer.WriteBoundNode(node.Expression);
            writer.ColorWrite(')');
        }

        private static void WriteBoundAssignmentExpression(this TextWriter writer, BoundAssignmentExpression node)
        {
            writer.WriteVariable(node.Variable.Name);
            writer.WriteSpace();
            writer.ColorWrite("=");
            writer.WriteSpace();
            writer.WriteBoundNode(node.Expression);
        }

        private static void WriteBoundBlockStatement(this TextWriter writer, BoundBlockStatement node)
        {
            if (writer is IndentedTextWriter indentedTextWriter1)
            {
                indentedTextWriter1.ColorWrite("{");
                indentedTextWriter1.Indent += 4;
                foreach (var n in node.Statements)
                {
                    indentedTextWriter1.WriteLine();
                    indentedTextWriter1.WriteBoundNode(n);
                }
                indentedTextWriter1.Indent -= 4;
                indentedTextWriter1.WriteLine();
                indentedTextWriter1.ColorWrite("}");
            }
            else
            {
                var indentedTextWriter2 = new IndentedTextWriter(writer, " ");
                indentedTextWriter2.ColorWrite("{");
                indentedTextWriter2.Indent += 4;
                foreach (var n in node.Statements)
                {
                    indentedTextWriter2.WriteLine();
                    indentedTextWriter2.WriteBoundNode(n);
                }
                indentedTextWriter2.Indent -= 4;
                indentedTextWriter2.WriteLine();
                indentedTextWriter2.ColorWrite("}");
            }
        }

        private static void WriteBoundExpressionStatement(this TextWriter writer, BoundExpressionStatement node)
        {
            writer.WriteBoundNode(node.Expression);
        }

        private static void WriteBoundVariableDeclarationStatement(this TextWriter writer, BoundVariableDeclarationStatement node)
        {
            writer.WriteBlueKeyword("var");
            writer.WriteSpace();
            writer.WriteVariable(node.Variable.Name);
            writer.WriteSpace();
            writer.ColorWrite("=");
            writer.WriteSpace();
            writer.WriteBoundNode(node.Expression);
        }

        private static void WriteBoundIfStatement(this TextWriter writer, BoundIfStatement node)
        {
            throw new NotImplementedException();
        }

        private static void WriteBoundForStatement(this TextWriter writer, BoundForStatement node)
        {
            throw new NotImplementedException();
        }

        private static void WriteBoundWhileStatement(this TextWriter writer, BoundWhileStatement node)
        {
            throw new NotImplementedException();
        }

        private static void WriteBoundDoWhileStatement(this TextWriter writer, BoundDoWhileStatement node)
        {
            throw new NotImplementedException();
        }

        private static void WriteBoundConditionalGotoStatement(this TextWriter writer, BoundConditionalGotoStatement node)
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

        private static void WriteBoundGotoStatement(this TextWriter writer, BoundGotoStatement node)
        {
            writer.WriteMagentaKeyword("goto");
            writer.WriteSpace();
            writer.ColorWrite(node.Label.Identifier);
        }

        private static void WriteBoundLabelStatement(this TextWriter writer, BoundLabelStatement node)
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

        private static void WriteBoundInvalidExpression(this TextWriter writer, BoundInvalidExpression node)
        {
            throw new NotImplementedException();
        }

        private static void WriteBoundReturnStatement(this TextWriter writer, BoundReturnStatement node)
        {
            writer.WriteMagentaKeyword("return");
            writer.WriteSpace();
            if (node.Expression == null)
                writer.WriteBlueKeyword("void");
            else writer.WriteBoundNode(node.Expression);
        }

        private static void WriteNumber(this TextWriter writer, object val) => writer.ColorWrite(val, ConsoleColor.DarkGreen);
        private static void WriteVariable(this TextWriter writer, string name) => writer.ColorWrite(name, ConsoleColor.Cyan);
        private static void WriteFunction(this TextWriter writer, string name) => writer.ColorWrite(name, ConsoleColor.Yellow);
        private static void WriteString(this TextWriter writer, string content) => writer.ColorWrite($"\"{content}\"", ConsoleColor.DarkCyan);
        private static void WriteMagentaKeyword(this TextWriter writer, string keyword) => writer.ColorWrite(keyword, ConsoleColor.Magenta);
        private static void WriteBlueKeyword(this TextWriter writer, string keyword) => writer.ColorWrite(keyword, ConsoleColor.Blue);
        private static void WriteSpace(this TextWriter writer, int len = 1) => writer.Write(new string(' ', len));
    }
} 