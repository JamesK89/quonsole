﻿/*
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
using Quonsole.Interfaces;
using Quonsole.Exceptions;
using Quonsole.Attributes;

namespace Quonsole.Commands;

[Command]
public class GdCommand : BaseInternalCommand
{
    public static GdCommand Instance { get; } = new GdCommand();

    public override string GetName()
    {
        return "gd";
    }

    public override ExecutionResult ExecuteHelp(IExecutionContext context)
    {
        context.Console.Info("Executes a GdScript expression.");
        RaiseHelpEvent(context);
        return ExecutionResult.Done;
    }

    public override ExecutionResult Execute(IExecutionContext context)
    {
        var argCount = context.Arguments?.Count ?? 0;

        if (argCount > 1)
            throw new TooManyArgumentsException(GetName(), argCount, 1);

        if (argCount < 1)
            throw new TooFewArgumentsException(GetName(), argCount, 1);

        var expr = new Expression();
        var err = expr.Parse(context.Arguments[0]);

        Variant result = default;

        if (err != Error.Ok)
        {
            context.Console.Error($"Parser returned: {err.ToString("G")}");
        }
        else
        {
            result = expr.Execute();
        }

        if (expr.HasExecuteFailed())
        {
            context.Console.Error("Execution failed.");
        }
        else
        {
            context.Console.Info($"Expression returned: {result.AsString()}");
        }

        RaiseExecutedEvent(context);

        return ExecutionResult.Done;
    }
}