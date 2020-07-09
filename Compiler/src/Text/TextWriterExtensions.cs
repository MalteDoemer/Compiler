using System;
using System.CodeDom.Compiler;
using System.IO;
using Compiler.Diagnostics;

namespace Compiler.Text
{
    public static class TextWriterExtensions
    {
        private static bool IsConsole(this TextWriter writer)
        {
            if (writer == Console.Out) return !Console.IsOutputRedirected;
            if (writer == Console.Error) return !Console.IsOutputRedirected && !Console.IsErrorRedirected;
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
            if (diagnostic.HasPositon)
            {
                var src = diagnostic.Location.Text;
                var prefix = src.ToString(0, diagnostic.Span.Start);
                var errorText = src.ToString(diagnostic.Span);
                var postfix = src.ToString(diagnostic.Span.End, src.Length - diagnostic.Span.End);

                var linenum = diagnostic.Location.StartLine;
                var charOff = diagnostic.Location.StartCharacter;


                writer.ColorWrite($"\n\n{diagnostic.Kind} in line {linenum} at character {charOff}\n\n");
                writer.ColorWrite(prefix);
                writer.ColorWrite(errorText, ConsoleColor.Red);
                writer.ColorWrite(postfix);
                writer.ColorWrite($"\n\n{diagnostic.Message}\n\n");
            }
            else writer.ColorWrite($"\n\n{diagnostic.Kind}: {diagnostic.Message}\n\n", ConsoleColor.Red);
        }

        public static void WriteDiagnosticReport(this TextWriter writer, DiagnosticReport report)
        {
            foreach (var d in report)
                writer.WriteDiagnostic(d);
        }
    }
}