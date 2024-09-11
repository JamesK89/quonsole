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

namespace Quonsole.Commands;

[Command]
public class HelpCommand : BaseInternalCommand
{
    public static HelpCommand Instance { get; } = new HelpCommand();

    public override string GetName()
    {
        return "help";
    }

    public override ExecutionResult ExecuteHelp(IExecutionContext context)
    {
        context.Console.Info("Prints out information about the given command or variable.");
        RaiseHelpEvent(context);
        return ExecutionResult.Done;
    }

    public override ExecutionResult Execute(IExecutionContext context)
    {
        var argCount = context.Arguments?.Count ?? 0;

        if (argCount < 1)
            throw new TooFewArgumentsException(GetName(), argCount, 1);

        var exec = context.Console.GetCommandOrVariable(context.Arguments[0]);

        if (exec != null)
        {
            return exec.ExecuteHelp(context);
        }
        else
        {
            var alias = context.Console.GetAlias(context.Arguments[0]);

            if (alias != null)
            {
                context.Console.Info($"{context.Arguments[0]} is an alias for: {alias}");
            }
            else
            {
                throw new CommandNotFoundException(context.Arguments[0]);
            }
        }

        RaiseExecutedEvent(context);

        return ExecutionResult.Done;
    }
}