/*
MIT License

Copyright (c) 2024 James Kelly

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using Godot;
using Quonsole.Attributes;
using Quonsole.Interfaces;

namespace Quonsole.Variables;

[Variable]
public class WindowWidthVariable : BaseInternalVariable
{
    public override string GetName()
    {
        return "wnd_width";
    }

    public override ExecutionResult ExecuteHelp(IExecutionContext context)
    {
        context.Console.Info("The window width");
        return ExecutionResult.Done;
    }

    public override Variant Get()
    {
        var width = DisplayServer.WindowGetSize().X;
        return Variant.From(width);
    }

    public override void Set(Variant value)
    {
        var width = value.AsInt32();
        var size = DisplayServer.WindowGetSize();
        DisplayServer.WindowSetSize(new Vector2I(width, size.Y));
        RaiseChangedEvent(value);
    }
}
