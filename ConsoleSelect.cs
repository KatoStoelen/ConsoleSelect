using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ConsoleStuff
{
    public class ConsoleSelect
    {
        private readonly Settings _settings;

        public ConsoleSelect()
            : this(new Settings())
        {
        }

        public ConsoleSelect(Settings settings)
        {
            _settings = settings;
        }

        public TKey PromptSelection<TKey>(string title, IEnumerable<Option<TKey>> options)
        {
            var selectableOptions = options.Select(option => new SelectableOption<TKey>(option)).ToList();
            if (!selectableOptions.Any())
                throw new ArgumentException("At least one option must be provided", nameof(options));

            if (selectableOptions.All(option => !option.Selected))
                selectableOptions.First().Selected = true;

            var selectStartCursorPosition = new CursorPosition(
                _settings.CursorLeftGetter.Invoke(), _settings.CursorTopGetter.Invoke());

            if (_settings.IsTitleEnabled)
                WriteTitle(title);

            DrawOptions(selectableOptions);

            var selectEndCursorPosition = new CursorPosition(
                _settings.CursorLeftGetter.Invoke(),
                _settings.CursorTopGetter.Invoke());

            _settings.CursorPositionSetter.Invoke(
                selectStartCursorPosition.Left, selectStartCursorPosition.Top);

            try
            {
                DrawSelection(selectableOptions, 0);
                HandleUserSelection(selectableOptions);
            }
            finally
            {
                _settings.CursorPositionSetter.Invoke(
                    selectEndCursorPosition.Left, selectEndCursorPosition.Top);
            }

            _settings.TextWriter.WriteLine();

            return selectableOptions.Single(option => option.Selected).Key;
        }

        private void WriteTitle(string title)
        {
            _settings.TextWriter.WriteLine();
            _settings.TextWriter.WriteLine(title);
            _settings.TextWriter.WriteLine();
        }

        private void DrawOptions<TKey>(IEnumerable<SelectableOption<TKey>> selectableOptions)
        {
            foreach (var option in selectableOptions)
            {
                var formattedOptionText = GetFormattedOptionText(option.Text, option.Selected);
                var selectionLeftPosition = _settings.OptionRenderFormat.IndexOf("{Selected}", StringComparison.Ordinal);

                option.SelectionPosition = new CursorPosition(
                    selectionLeftPosition, _settings.CursorTopGetter.Invoke());

                _settings.TextWriter.WriteLine(formattedOptionText);
            }
        }

        private string GetFormattedOptionText(string text, bool selected)
        {
            if (_settings.OptionRenderFormat.IndexOf("{Selected}", StringComparison.Ordinal) == -1)
                throw new InvalidOperationException($"{nameof(_settings.OptionRenderFormat)} is missing the {{Selected}} placeholder");
            if (_settings.OptionRenderFormat.IndexOf("{Text}", StringComparison.Ordinal) == -1)
                throw new InvalidOperationException($"{nameof(_settings.OptionRenderFormat)} is missing the {{Text}} placeholder");

            return
                _settings.OptionRenderFormat
                    .Replace("{Selected}", selected ? _settings.OptionSelectedIndicator : _settings.OptionNotSelectedIndicator)
                    .Replace("{Text}", text);
        }

        private void HandleUserSelection<TKey>(IReadOnlyList<SelectableOption<TKey>> selectableOptions)
        {
            _settings.CursorVisibilitySetter.Invoke(false);

            try
            {
                ConsoleKeyInfo readKey;
                do
                {
                    readKey = _settings.InputKeyReader.Invoke(true);

                    if (readKey.Key == _settings.MoveSelectionUpKey)
                    {
                        MoveSelection(selectableOptions, -1);
                        DrawSelection(selectableOptions, -1);
                    }
                    else if (readKey.Key == _settings.MoveSelectionDownKey)
                    {
                        MoveSelection(selectableOptions, 1);
                        DrawSelection(selectableOptions, 1);
                    }
                } while (readKey.Key != _settings.ConfirmSelectionKey);
            }
            finally
            {
                _settings.CursorVisibilitySetter.Invoke(true);
            }
        }

        private static void MoveSelection<TKey>(
            IReadOnlyList<SelectableOption<TKey>> options,
            int directionVector)
        {
            for (var i = 0; i < options.Count; i++)
            {
                var option = options[i];

                if (!option.Selected) continue;

                option.Selected = false;

                if (i == 0 && directionVector < 0)
                {
                    options[options.Count - 1].Selected = true;
                }
                else if (i == options.Count - 1 && directionVector > 0)
                {
                    options[0].Selected = true;
                }
                else
                {
                    options[i + directionVector].Selected = true;
                }

                break;
            }
        }

        private void DrawSelection<TKey>(
            IReadOnlyList<SelectableOption<TKey>> options,
            int directionVector)
        {
            for (var i = 0; i < options.Count; i++)
            {
                var option = options[i];

                if (!option.Selected) continue;

                CursorPosition previouslySelectedOptionPosition = null;

                if (i == 0 && directionVector > 0)
                {
                    previouslySelectedOptionPosition = options[options.Count - 1].SelectionPosition;
                }
                else if (i == options.Count - 1 && directionVector < 0)
                {
                    previouslySelectedOptionPosition = options[0].SelectionPosition;
                }
                else if (directionVector != 0)
                {
                    previouslySelectedOptionPosition = options[i - directionVector].SelectionPosition;
                }

                if (previouslySelectedOptionPosition != null)
                {
                    _settings.CursorPositionSetter.Invoke(
                        previouslySelectedOptionPosition.Left, previouslySelectedOptionPosition.Top);
                    _settings.TextWriter.Write(_settings.OptionNotSelectedIndicator);
                }

                _settings.CursorPositionSetter.Invoke(option.SelectionPosition.Left, option.SelectionPosition.Top);
                _settings.TextWriter.Write(_settings.OptionSelectedIndicator);

                break;
            }
        }

        public class Option<TKey>
        {
            public Option() { }

            public Option(TKey key, string text)
                : this(key, text, false)
            {
            }

            public Option(TKey key, string text, bool selected)
            {
                Key = key;
                Text = text;
                Selected = selected;
            }

            public TKey Key { get; set; }
            public string Text { get; set; }
            public bool Selected { get; set; }
        }

        // Probably overkill to make all this stuff configurable
        public class Settings
        {
            public string OptionRenderFormat { get; set; } = "[{Selected}] {Text}";
            public string OptionNotSelectedIndicator { get; set; } = " ";
            public string OptionSelectedIndicator { get; set; } = "X";

            public bool IsTitleEnabled { get; set; } = true;

            public ConsoleKey MoveSelectionUpKey { get; set; } = ConsoleKey.UpArrow;
            public ConsoleKey MoveSelectionDownKey { get; set; } = ConsoleKey.DownArrow;
            public ConsoleKey ConfirmSelectionKey { get; set; } = ConsoleKey.Enter;

            public TextWriter TextWriter { get; set; } = Console.Out;

            public Func<bool, ConsoleKeyInfo> InputKeyReader { get; set; } = Console.ReadKey;
            public Action<bool> CursorVisibilitySetter { get; set; } = visible => Console.CursorVisible = visible;
            public Action<int, int> CursorPositionSetter { get; set; } = Console.SetCursorPosition;
            public Func<int> CursorLeftGetter { get; set; } = () => Console.CursorLeft;
            public Func<int> CursorTopGetter { get; set; } = () => Console.CursorTop;
        }

        private class SelectableOption<TKey>
        {
            public SelectableOption(Option<TKey> option)
            {
                Key = option.Key;
                Text = option.Text;
                Selected = option.Selected;
            }

            public TKey Key { get; }
            public string Text { get; }
            public bool Selected { get; set; }
            public CursorPosition SelectionPosition { get; set; }
        }

        private class CursorPosition
        {
            public CursorPosition(int left, int top)
            {
                Left = left;
                Top = top;
            }

            public int Left { get; }
            public int Top { get; }
        }
    }
}
