using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Text;
using Compiler.Text;

namespace Compiler.Test
{
    public class AnnotatedText
    {
        public AnnotatedText(string text, ImmutableArray<TextSpan> spans)
        {
            Text = text;
            Spans = spans;
        }

        public string Text { get; }
        public ImmutableArray<TextSpan> Spans { get; }

        public static AnnotatedText Parse(string text)
        {
            text = Unindent(text);

            var textBuilder = new StringBuilder();
            var spanBuilder = ImmutableArray.CreateBuilder<TextSpan>();

            var startStack = new Stack<int>();

            var pos = 0;
            foreach (var c in text)
            {
                if (c == '[')
                {
                    startStack.Push(pos);
                }
                else if (c == ']')
                {
                    if (startStack.Count <= 0) throw new ArgumentException("Brackets don't match up");

                    var start = startStack.Pop();
                    var span = TextSpan.FromBounds(start, pos);
                    spanBuilder.Add(span);
                }
                else
                {
                    pos++;
                    textBuilder.Append(c);
                }
            }

            if (startStack.Count != 0) throw new ArgumentException("Brackets don't match up");


            return new AnnotatedText(textBuilder.ToString(), spanBuilder.ToImmutable());
        }

        public static string[] UnindentLines(string text)
        {
            var lines = new List<string>();

            using (var reader = new StringReader(text))
            {
                string line;
                while ((line = reader.ReadLine()) is not null)
                    lines.Add(line);
            }

            var minIndentation = int.MaxValue;
            for (var i = 0; i < lines.Count; i++)
            {
                var line = lines[i];

                if (line.Trim().Length == 0)
                {
                    lines[i] = string.Empty;
                    continue;
                }

                var indentation = line.Length - line.TrimStart().Length;
                minIndentation = Math.Min(minIndentation, indentation);
            }

            for (var i = 0; i < lines.Count; i++)
            {
                if (lines[i].Length == 0)
                    continue;

                lines[i] = lines[i].Substring(minIndentation);
            }

            while (lines.Count > 0 && lines[0].Length == 0)
                lines.RemoveAt(0);

            while (lines.Count > 0 && lines[lines.Count - 1].Length == 0)
                lines.RemoveAt(lines.Count - 1);

            return lines.ToArray();
        }

        public static string Unindent(string text)
        {
            return string.Join(Environment.NewLine, UnindentLines(text));
        }
    }
}