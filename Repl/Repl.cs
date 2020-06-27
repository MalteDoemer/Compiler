using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;

namespace Editor
{
    public class ConsoleEditor
    {
        private int cursorX;
        private int cursorY;
        private List<string> document;

        private void Start()
        {
            Console.CursorSize = 100;
            while (true)
            {
                HandleInput();
            }
        }

        private void HandleInput()
        {
            var key = Console.ReadKey(true);

            switch (key.Key)
            {
                case ConsoleKey.Escape:
                    HandleExit(); break;
            }
        }

        private void HandleExit() => Environment.Exit(0);

        public static void NotMain(string[] args)
        {
            try
            {
                var repl = new ConsoleEditor();
                repl.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.ReadLine();
            }
        }
    }

    internal sealed class ConsoleWriter
    {
        private ObservableCollection<string> document;

        public ConsoleWriter(ObservableCollection<string> document)
        {
            this.document = document;
            this.document.CollectionChanged += DocumentChanged;
        }

        private void DocumentChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
        {
            RenderDocument();
        }

        private void RenderDocument()
        {
            Console.CursorVisible = false;

            for (int y = 0; y < document.Count; y++)
            {
                if (y >= Console.BufferHeight) break;
            }
        }
    }
}