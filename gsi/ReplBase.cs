using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Reflection;
using System.Text;

namespace gsi
{
    public abstract class ReplBase
    {
        private readonly string prompt;
        private readonly string @continue;
        private readonly ConsoleColor foreground;
        private readonly ConsoleColor background;
        private readonly List<MetaCommand> metaCommands;

        private readonly List<string> submissionHistory;
        private int submissionHistoryIndex;
        private bool editDone;

        protected ReplBase(string prompt = "» ", string @continue = "· ", ConsoleColor foreground = ConsoleColor.Green, ConsoleColor background = ConsoleColor.Black)
        {
            if (prompt.Length != @continue.Length)
                throw new ArgumentException("prompt and contune must have the same length.");

            this.prompt = prompt;
            this.@continue = @continue;
            this.foreground = foreground;
            this.background = background;
            this.submissionHistory = new List<string>();
            this.metaCommands = new List<MetaCommand>();
            InitializeMetaCommands();
        }

        private void InitializeMetaCommands()
        {
            var methods = GetType().GetMethods(BindingFlags.Public |
                                               BindingFlags.NonPublic |
                                               BindingFlags.Static |
                                               BindingFlags.Instance |
                                               BindingFlags.FlattenHierarchy);
            foreach (var method in methods)
            {
                var attribute = method.GetCustomAttribute<MetaCommandAttribute>();
                if (attribute is null)
                    continue;

                var metaCommand = new MetaCommand(attribute.Name, attribute.Description, method);
                metaCommands.Add(metaCommand);
            }
        }

        public void Run()
        {
            while (true)
            {
                var text = EditSubmission();

                if (string.IsNullOrEmpty(text))
                    continue;


                if (!text.Contains(Environment.NewLine) && text.StartsWith('#'))
                    EvaluateMetaCommand(text);
                else
                    EvaluateSubmission(text);

                submissionHistory.Add(text);
                submissionHistoryIndex = 0;
            }
        }

        private string EditSubmission()
        {
            editDone = false;
            var document = new ObservableCollection<string>() { "" };
            var renderer = new SubmissionRenderer(this, document);

            while (!editDone)
            {
                var key = Console.ReadKey(true);
                HandleKey(key, document, renderer);
            }

            return string.Join(Environment.NewLine, document);
        }

        private void HandleKey(ConsoleKeyInfo key, ObservableCollection<string> document, SubmissionRenderer renderer)
        {
            if (key.Modifiers == default(ConsoleModifiers))
            {
                switch (key.Key)
                {
                    case ConsoleKey.Backspace:
                        HandleBackspace(document, renderer);
                        break;
                    case ConsoleKey.Tab:
                        HandleTab(document, renderer);
                        break;
                    case ConsoleKey.Enter:
                        HandleEnter(document, renderer);
                        break;
                    case ConsoleKey.Escape:
                        HandleEscape(document, renderer);
                        break;
                    case ConsoleKey.PageUp:
                        HandlePageUp(document, renderer);
                        break;
                    case ConsoleKey.PageDown:
                        HandlePageDown(document, renderer);
                        break;
                    case ConsoleKey.End:
                        HandleEnd(document, renderer);
                        break;
                    case ConsoleKey.Home:
                        HandleHome(document, renderer);
                        break;
                    case ConsoleKey.LeftArrow:
                        HandleLeftArrow(document, renderer);
                        break;
                    case ConsoleKey.UpArrow:
                        HandleUpArrow(document, renderer);
                        break;
                    case ConsoleKey.RightArrow:
                        HandleRightArrow(document, renderer);
                        break;
                    case ConsoleKey.DownArrow:
                        HandleDownArrow(document, renderer);
                        break;
                    case ConsoleKey.Delete:
                        HandleDelete(document, renderer);
                        break;
                }
            }

            else if (key.Modifiers == ConsoleModifiers.Control)
            {
                switch (key.Key)
                {
                    case ConsoleKey.Enter:
                        HandleCtrlEnter(document, renderer);
                        break;
                }
            }


            if (key.Key != ConsoleKey.Backspace && key.KeyChar >= ' ')
                HandleTyping(document, renderer, key.KeyChar.ToString());
        }

        private void InsertLine(ObservableCollection<string> document, SubmissionRenderer renderer)
        {
            var remainder = document[renderer.CurrentLine].Substring(renderer.CurrentChar);
            document[renderer.CurrentLine] = document[renderer.CurrentLine].Substring(0, renderer.CurrentChar);

            var lineIndex = renderer.CurrentLine + 1;
            document.Insert(lineIndex, remainder);
            renderer.CurrentChar = 0;
            renderer.CurrentLine = lineIndex;
        }

        private void HandleTyping(ObservableCollection<string> document, SubmissionRenderer renderer, string text)
        {
            var lineIndex = renderer.CurrentLine;
            var start = renderer.CurrentChar;
            document[lineIndex] = document[lineIndex].Insert(start, text);
            renderer.CurrentChar += text.Length;
        }

        private void HandleLeftArrow(ObservableCollection<string> document, SubmissionRenderer renderer)
        {
            if (renderer.CurrentChar > 0)
                renderer.CurrentChar--;
        }

        private void HandleRightArrow(ObservableCollection<string> document, SubmissionRenderer renderer)
        {
            if (renderer.CurrentChar < document[renderer.CurrentLine].Length)
                renderer.CurrentChar++;
        }

        private void HandleUpArrow(ObservableCollection<string> document, SubmissionRenderer renderer)
        {
            if (renderer.CurrentLine > 0)
                renderer.CurrentLine--;
            else
                HandlePageUp(document, renderer);
        }

        private void HandleDownArrow(ObservableCollection<string> document, SubmissionRenderer renderer)
        {
            if (renderer.CurrentLine < document.Count - 1)
                renderer.CurrentLine++;
            else
                HandlePageDown(document, renderer);
        }

        private void HandleBackspace(ObservableCollection<string> document, SubmissionRenderer renderer)
        {
            var start = renderer.CurrentChar;

            if (start == 0)
            {
                if (renderer.CurrentLine == 0)
                    return;

                var currentLine = document[renderer.CurrentLine];
                var previousLine = document[renderer.CurrentLine - 1];
                document.RemoveAt(renderer.CurrentLine);
                renderer.CurrentLine--;
                document[renderer.CurrentLine] = previousLine + currentLine;
                renderer.CurrentChar = previousLine.Length;
            }
            else
            {
                var lineIndex = renderer.CurrentLine;
                var line = document[lineIndex];
                var before = line.Substring(0, start - 1);
                var after = line.Substring(start);
                document[lineIndex] = before + after;
                renderer.CurrentChar--;
            }
        }

        private void HandleDelete(ObservableCollection<string> document, SubmissionRenderer renderer)
        {
            var lineIndex = renderer.CurrentLine;
            var line = document[lineIndex];
            var start = renderer.CurrentChar;
            if (start >= line.Length)
            {
                if (renderer.CurrentLine == document.Count - 1)
                {
                    return;
                }

                var nextLine = document[renderer.CurrentLine + 1];
                document[renderer.CurrentLine] += nextLine;
                document.RemoveAt(renderer.CurrentLine + 1);
                return;
            }

            var before = line.Substring(0, start);
            var after = line.Substring(start + 1);
            document[lineIndex] = before + after;
        }

        private void HandleEscape(ObservableCollection<string> document, SubmissionRenderer renderer)
        {
            document.Clear();
            document.Add(string.Empty);
            renderer.CurrentLine = 0;
            renderer.CurrentChar = 0;
        }

        private void HandleTab(ObservableCollection<string> document, SubmissionRenderer renderer)
        {
            const int TabWidth = 4;
            var start = renderer.CurrentChar;
            var remainingSpaces = TabWidth - start % TabWidth;
            var line = document[renderer.CurrentLine];
            document[renderer.CurrentLine] = line.Insert(start, new string(' ', remainingSpaces));
            renderer.CurrentChar += remainingSpaces;
        }

        private void HandleEnter(ObservableCollection<string> document, SubmissionRenderer renderer)
        {
            var submissionText = string.Join(Environment.NewLine, document);
            if (IsSubmissionComplete(submissionText))
            {
                editDone = true;
                return;
            }

            InsertLine(document, renderer);
        }

        private void HandleCtrlEnter(ObservableCollection<string> document, SubmissionRenderer renderer)
        {
            InsertLine(document, renderer);
        }

        private void HandleEnd(ObservableCollection<string> document, SubmissionRenderer renderer)
        {
            renderer.CurrentChar = document[renderer.CurrentLine].Length;
        }

        private void HandleHome(ObservableCollection<string> document, SubmissionRenderer renderer)
        {
            renderer.CurrentChar = 0;
        }

        private void HandlePageUp(ObservableCollection<string> document, SubmissionRenderer renderer)
        {
            submissionHistoryIndex--;
            if (submissionHistoryIndex < 0)
                submissionHistoryIndex = submissionHistory.Count - 1;
            UpdateDocumentFromHistory(document, renderer);
        }

        private void HandlePageDown(ObservableCollection<string> document, SubmissionRenderer renderer)
        {
            submissionHistoryIndex++;
            if (submissionHistoryIndex > submissionHistory.Count - 1)
                submissionHistoryIndex = 0;
            UpdateDocumentFromHistory(document, renderer);
        }

        private void UpdateDocumentFromHistory(ObservableCollection<string> document, SubmissionRenderer view)
        {
            if (submissionHistory.Count == 0)
                return;

            document.Clear();

            Console.WriteLine(submissionHistoryIndex);
            var historyItem = submissionHistory[submissionHistoryIndex];
            var lines = historyItem.Split(Environment.NewLine);
            foreach (var line in lines)
                document.Add(line);

            view.CurrentLine = document.Count - 1;
            view.CurrentChar = document[view.CurrentLine].Length;
        }

        protected abstract bool IsSubmissionComplete(string text);
        protected abstract void EvaluateSubmission(string text);
        protected abstract void RenderLine(IReadOnlyList<string> lines, int lineCount);
        protected void ClearHistory() => submissionHistory.Clear();


        private sealed class SubmissionRenderer
        {
            private readonly ObservableCollection<string> submissionDocument;
            private readonly ReplBase parent;
            private readonly int startLine;

            private int currentChar;
            private int currentLine;
            private int linesRendered;

            public SubmissionRenderer(ReplBase parent, ObservableCollection<string> submissionDocument)
            {
                this.parent = parent;
                this.startLine = Console.CursorTop;
                this.submissionDocument = submissionDocument;
                this.submissionDocument.CollectionChanged += OnDocumentChanged;
                Render();
            }

            private void OnDocumentChanged(object sender, NotifyCollectionChangedEventArgs args) => Render();

            private void Render()
            {
                Console.CursorVisible = false;

                var lineCount = 0;

                foreach (var line in submissionDocument)
                {

                    Console.SetCursorPosition(0, startLine + lineCount);
                    Console.ForegroundColor = parent.foreground;
                    Console.BackgroundColor = parent.background;

                    if (lineCount == 0)
                        Console.Write(parent.prompt);
                    else
                        Console.Write(parent.@continue);

                    Console.ResetColor();

                    Console.Write(new string(' ', Console.WindowWidth - parent.prompt.Length));
                    Console.SetCursorPosition(parent.prompt.Length, startLine + lineCount);
                    parent.RenderLine(submissionDocument, lineCount);
                    lineCount++;
                }
                var numberOfBlankLines = linesRendered - lineCount;
                if (numberOfBlankLines > 0)
                {
                    var blankLine = new string(' ', Console.WindowWidth);
                    for (var i = 0; i < numberOfBlankLines; i++)
                    {
                        Console.SetCursorPosition(0, startLine + lineCount + i);
                        Console.WriteLine(blankLine);
                    }
                }

                linesRendered = lineCount;
                UpdateCursorPosition();
                Console.CursorVisible = true;
            }

            private void UpdateCursorPosition()
            {
                Console.CursorTop = startLine + currentLine;
                Console.CursorLeft = parent.prompt.Length + currentChar;
            }

            public int CurrentLine
            {
                get => currentLine;
                set
                {
                    if (currentLine != value && value >= 0)
                    {
                        currentLine = value;
                        currentChar = Math.Min(submissionDocument[currentLine].Length, currentChar);
                        UpdateCursorPosition();
                    }
                }
            }

            public int CurrentChar
            {
                get => currentChar;
                set
                {
                    if (currentChar != value)
                    {
                        currentChar = value;
                        UpdateCursorPosition();
                    }
                }
            }
        }

        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
        protected sealed class MetaCommandAttribute : Attribute
        {
            public MetaCommandAttribute(string name, string description)
            {
                Name = name;
                Description = description;
            }

            public string Name { get; }
            public string Description { get; }
        }

        private sealed class MetaCommand
        {
            public MetaCommand(string name, string description, MethodInfo method)
            {
                Name = name;
                Description = description;
                Method = method;
            }

            public string Name { get; }
            public string Description { get; }
            public MethodInfo Method { get; }
        }


        private void EvaluateMetaCommand(string input)
        {
            {
                // Parse arguments

                var args = new List<string>();
                var inQuotes = false;
                var position = 1;
                var sb = new StringBuilder();
                while (position < input.Length)
                {
                    var c = input[position];
                    var l = position + 1 >= input.Length ? '\0' : input[position + 1];

                    if (char.IsWhiteSpace(c))
                    {
                        if (!inQuotes)
                            CommitPendingArgument();
                        else
                            sb.Append(c);
                    }
                    else if (c == '\"')
                    {
                        if (!inQuotes)
                            inQuotes = true;
                        else if (l == '\"')
                        {
                            sb.Append(c);
                            position++;
                        }
                        else
                            inQuotes = false;
                    }
                    else
                    {
                        sb.Append(c);
                    }

                    position++;
                }

                CommitPendingArgument();

                void CommitPendingArgument()
                {
                    var arg = sb.ToString();
                    if (!string.IsNullOrWhiteSpace(arg))
                        args.Add(arg);
                    sb.Clear();
                }

                var commandName = args.FirstOrDefault();
                if (args.Count > 0)
                    args.RemoveAt(0);

                var command = metaCommands.SingleOrDefault(mc => mc.Name == commandName);
                if (command is null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\nInvalid command {input}.");
                    Console.ResetColor();
                    return;
                }

                var parameters = command.Method.GetParameters();

                if (args.Count != parameters.Length)
                {
                    var parameterNames = string.Join(" ", parameters.Select(p => $"<{p.Name}>"));
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"error: invalid number of arguments");
                    Console.WriteLine($"usage: #{command.Name} {parameterNames}");
                    Console.ResetColor();
                    return;
                }

                var instance = command.Method.IsStatic ? null : this;
                command.Method.Invoke(instance, args.ToArray());
            }
        }
    }
}