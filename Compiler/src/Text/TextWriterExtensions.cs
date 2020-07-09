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

    }
}