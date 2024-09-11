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
using System;
using System.Linq;
using Quonsole.Interfaces;
using Quonsole.Attributes;

namespace Quonsole.Variables;

[Variable]
public class WindowModeVariable : BaseInternalVariable
{
    public override string GetName()
    {
        return "wnd_mode";
    }

    public override ExecutionResult ExecuteHelp(IExecutionContext context)
    {
        var values = Enum.GetValues<DisplayServer.WindowMode>().Select(x => $"({x.ToString("d")}) {x.ToString("G")}");
        context.Console.Info($"The window display mode. Possible values: [\n\t{string.Join(", \n\t", values)}\n].");
        return ExecutionResult.Done;
    }

    public override Variant Get()
    {
        var mode = DisplayServer.WindowGetMode();
        return Variant.From(mode.ToString("G"));
    }

    public override void Set(Variant value)
    {
        var mode = (DisplayServer.WindowMode)Enum.Parse(typeof(DisplayServer.WindowMode), value.AsString(), true);
        DisplayServer.WindowSetMode(mode);
        RaiseChangedEvent(value);
    }
}
