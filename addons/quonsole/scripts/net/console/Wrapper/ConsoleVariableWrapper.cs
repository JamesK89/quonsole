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
using Godot.Collections;
using Quonsole.Extensions;
using Quonsole.Interfaces;

public partial class ConsoleVariableWrapper : GodotObject
{
    [Signal]
    public delegate void ChangedEventHandler(ConsoleVariableWrapper variable, Variant value);

    [Signal]
    public delegate void ExecutedEventHandler(ConsoleVariableWrapper variable, string guid, double delta, Dictionary data, Array arguments);

    [Signal]
    public delegate void HelpExecutedEventHandler(ConsoleVariableWrapper variable, string guid, double delta, Dictionary data, Array arguments);

    private IConsoleCore _core;
    private IVariable _variable;

    public ConsoleVariableWrapper(IConsoleCore core, IVariable variable)
    {
        _core = core;

        _variable = variable;

        _variable.Changed += (sender, value) => EmitSignal(SignalName.Changed, this, value);

        _variable.Executed += (sender, args) =>
            EmitSignal(
                SignalName.HelpExecuted,
                this,
                args.Guid,
                args.Delta,
                (Dictionary<string, Variant>)args?.Context?.Data ?? new Dictionary<string, Variant>(),
                args.Arguments.ToGodotArray()
            );

        _variable.HelpExecuted += (sender, args) =>
            EmitSignal(
                SignalName.HelpExecuted,
                this,
                args.Guid,
                args.Delta,
                (Dictionary<string, Variant>)args?.Context?.Data ?? new Dictionary<string, Variant>(),
                args.Arguments.ToGodotArray()
            );
    }

    public string GetName()
    {
        return _variable.GetName();
    }

    public string GetGuid()
    {
        return _variable.Guid;
    }

    public Variant GetValue()
    {
        return _variable.Get();
    }

    public void SetValue(Variant value)
    {
        _variable.Set(value);
    }

    public void ExecuteHelp(string[] args)
    {
        _core.ExecuteHelp(_variable, args);
    }

    public void Execute(string[] args)
    {
        _core.Execute(_variable, args);
    }
}
