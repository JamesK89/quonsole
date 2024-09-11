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

using System;
using System.Collections.Generic;
using System.Text;
using Quonsole.Interfaces;
using Quonsole.Exceptions;
using Quonsole.Attributes;

namespace Quonsole.Commands;

[Command]
public class ListCmdCommand : BaseInternalCommand
{
    public static ListCmdCommand Instance { get; } = new ListCmdCommand();

    public override string GetName()
    {
        return "listcmd";
    }

    public override ExecutionResult ExecuteHelp(IExecutionContext context)
    {
        context.Console.Info("Prints out a list of commands.");
        RaiseHelpEvent(context);
        return ExecutionResult.Done;
    }

    public override ExecutionResult Execute(IExecutionContext context)
    {
        var argCount = context.Arguments?.Count ?? 0;

        if (argCount > 1)
            throw new TooManyArgumentsException(GetName(), argCount, 1);

        bool filter = argCount > 0 && !string.IsNullOrWhiteSpace(context.Arguments[0]);

        HashSet<string> added = new HashSet<string>();
        List<string> items = new List<string>();

        foreach (IExecutable cmd in context.Console.Commands)
        {
            string sCmd = cmd.GetName().ToLowerInvariant();
            string sanitized = sCmd.StartsWith('+') || sCmd.StartsWith('-') ? sCmd.Substring(1) : sCmd;

            if (added.Contains(sanitized))
                continue;

            if (!filter || (filter && sCmd.Contains(context.Arguments[0], StringComparison.OrdinalIgnoreCase)))
            {
                items.Add(sCmd);
                added.Add(sanitized);
            }
        }

        StringBuilder sb = new StringBuilder();

        items.Sort();
        items.ForEach((var) => sb.AppendLine(var));

        context.Console.Info(sb.ToString());

        RaiseExecutedEvent(context);

        return ExecutionResult.Done;
    }
}