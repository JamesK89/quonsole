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

using Quonsole.Interfaces;
using Quonsole.Exceptions;
using Quonsole.Attributes;
using Godot;

namespace Quonsole.Commands;

[Command]
public class LoopCommand : BaseInternalCommand
{
    public static LoopCommand Instance { get; } = new LoopCommand();

    public override string GetName()
    {
        return "loop";
    }

    public override ExecutionResult ExecuteHelp(IExecutionContext context)
    {
        context.Console.Info("Repeats a command for a given number of times.");

        RaiseHelpEvent(context);

        return ExecutionResult.Done;
    }

    public override ExecutionResult Execute(IExecutionContext context)
    {
        if (context.Arguments?.Count > 2)
            throw new TooManyArgumentsException(GetName(), context.Arguments?.Count ?? 0, 0);

        if (context.Arguments?.Count < 2)
            throw new TooFewArgumentsException(GetName(), context.Arguments?.Count ?? 0, 0);

        var countKey = $"{context.Guid}_loop_count";
        int count = context.Arguments[0].ToInt();

        var command = context.Arguments[1];

        if (context.Data.ContainsKey(countKey))
        {
            count = context.Data[countKey].AsInt32() - 1;
            context.Data[countKey] = Variant.CreateFrom(count);
        }
        else
        {
            context.Data.Add(countKey, Variant.CreateFrom(count));
        }

        if (count > 0)
        {
            context.Execute(command);

            RaiseExecutedEvent(context);
            return ExecutionResult.Continue;
        }
        else
        {
            context.Data.Remove(countKey);
        }

        RaiseExecutedEvent(context);
        return ExecutionResult.Done;
    }
}