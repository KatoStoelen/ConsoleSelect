# ConsoleSelect

## Usage

The following example:

```csharp
var consoleSelect = new ConsoleSelect();

var options = new[]
{
    new ConsoleSelect.Option<string> { Key = "option1", Text = "Option 1" },
    new ConsoleSelect.Option<string> { Key = "option2", Text = "Option 2" },
    new ConsoleSelect.Option<string> { Key = "option3", Text = "Option 3", Selected = true },
    new ConsoleSelect.Option<string> { Key = "option4", Text = "Option 4" },
    new ConsoleSelect.Option<string> { Key = "option5", Text = "Option 5" }
};

var selectedOption = consoleSelect.PropmtSelection("Select an option:", options);
```

Would render:

```

Select an option:

[ ] Option 1
[ ] Option 2
[X] Option 3
[ ] Option 4
[ ] Option 5
```

By default, `UpArrow` and `DownArrow` is used to move selection up and down. `Enter` is used to confirm the selection.

## Customization

The `ConsoleSelect(ConsoleSelect.Settings settings)` constructor can be used to custimize the following:

|Property|Default|Description|
|---|---|---|
|OptionRenderFormat|`"[{Selected}] {Text}"`|The render format of each option (`{Selected}` and `{Text}` are required placeholders)|
|OptionNotSelectedIndicator|`" "`|The string indicating that an option is **not** selected|
|OptionSelectedIndicator|`"X"`|The string indicating that an option is selected|
|IsTitleEnabled|`true`|Whether or not the title should be drawn|
|MoveSelectionUpKey|`ConsoleKey.UpArrow`|The key used to move the selection up|
|MoveSelectionDownKey|`ConsoleKey.DownArrow`|The key used to move the selection down|
|ConfirmSelectionKey|`ConsoleKey.Enter`|The key used to confirm the selection|
|TextWriter|`Console.Out`|The writer used to output the title and options|
|InputKeyReader|`intercept => Console.ReadKey(intercept)`|A function to read a key from user input|
|CursorVisibilitySetter|`visible => Console.CursorVisible = visible`|A function to set the console cursor visibility|
|CursorPositionSetter|`(left, top) => Console.SetCursorPosition(left, top)`|A function to set the console cursor position|
|CursorLeftGetter|`() => Console.CursorLeft`|A function to get the console cursor *left* position|
|CursorTopGetter|`() => Console.CursorTop`|A function to get the console cursor *top* position|