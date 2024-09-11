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
using FA = Godot.FileAccess;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Quonsole.Interfaces;
using Quonsole.Exceptions;
using Quonsole.Parser;

namespace Quonsole.Core;

public class ExecutionContext : IExecutionContext
{
    public const string ExecutionContextGuidKey = "exec_ctx_guid";

    private record class ResolvedCommandItem(IExecutable executable, string command, IReadOnlyList<string> arguments);
    public record class ExecutionQueueItem(Guid guid, IExecutable executable, IReadOnlyList<string> arguments);
    public record class CommandQueueItem(string[] command, bool invert);

    private static CommandParser _parser = new CommandParser();

    public event ExecutionContextEventHandler Started;
    public event ExecutionContextEventHandler Stopped;

    public IConsoleCore Console { get; private set; }
    public bool Invert { get; private set; }
    public bool Invertible { get; private set; }
    public bool Persist { get; private set; }
    public double Delta { get; private set; }
    public bool DetailedStackTrace { get; private set; }
    public Guid Guid { get; set; } = Guid.NewGuid();
    public IExecutionData Data { get; private set; }
    public IStringTransformer Transformer { get; private set; }
    public IExecutable Current { get; private set; }
    public IReadOnlyList<string> Arguments { get; private set; }
    public IReadOnlyList<string> NonTransformedArguments { get; private set; }
    public Stack<ExecutionQueueItem> ExecutionQueue { get; private set; } = new Stack<ExecutionQueueItem>();
    public Stack<CommandQueueItem> CommandQueue { get; private set; } = new Stack<CommandQueueItem>();
    public bool IsExecuting { get; private set; }
    public bool IsFinished { get; private set; }

    public ExecutionContext(IConsoleCore console, bool invert, bool persist)
    {
        Console = console;
        Transformer = console.Transformer;
        Invert = invert;
        Invertible = false;
        Persist = persist;
        IsFinished = false;
        DetailedStackTrace = console.GetVariable("dbg_stacktrace")?.AsBool() ?? false;
    }

    private bool AdvanceExecutionQueue()
    {
        while (ExecutionQueue.Count > 0 || CommandQueue.Count > 0)
        {
            if (Current == null && ExecutionQueue.Count > 0)
            {
                var q = ExecutionQueue.Pop();

                Guid = q.guid;
                Current = q.executable;
                Arguments = q.arguments.Select(arg => Transform(arg)).ToArray();
                NonTransformedArguments = q.arguments;
                Data = new ExecutionData();

                return true;
            }
            else if (AdvanceCommandQueue())
            {
                continue;
            }

            break;
        }

        return false;
    }

    private bool AdvanceCommandQueue()
    {
        while (CommandQueue.Count > 0)
        {
            var cmd = CommandQueue.Pop();
            ProcessCommand(cmd.command, cmd.invert);
            return true;
        }

        return false;
    }

    private void AddToExecutionQueue(ResolvedCommandItem item)
    {
        ExecutionQueue.Push(new ExecutionQueueItem(Guid.NewGuid(), item.executable, item.arguments));
    }

    private void AddToCommandQueue(string cmd, bool invert = false, bool defer = false)
    {
        if (!string.IsNullOrWhiteSpace(cmd))
        {
            var commands = _parser.Parse(cmd);

            for (int i = commands.Count - 1; i >= 0; i--)
            {
                var command = commands[i];

                if (command.Length < 1)
                {
                    continue;
                }

                if (defer)
                {
                    CommandQueue.Push(new CommandQueueItem(command, invert));
                }
                else
                {
                    ProcessCommand(command, invert);
                }
            }
        }
    }

    public ExecutionResult Step(double delta)
    {
        var result = ExecutionResult.Done;

        if (ExecutionQueue.Count > 0 && Current == null)
        {
            Started?.Invoke(this);
        }

        while (result == ExecutionResult.Done)
        {
            if (Current == null && !AdvanceExecutionQueue())
            {
                Stopped?.Invoke(this);
                IsExecuting = false;
                IsFinished = true;
                result = ExecutionResult.Done;
                break;
            }

            IsExecuting = true;

            try
            {
                Delta = delta;

                if (Data.ContainsKey(ExecutionContextGuidKey))
                {
                    Data.Remove(ExecutionContextGuidKey);
                }

                Data.Add(ExecutionContextGuidKey, Variant.CreateFrom(Guid.ToString()));

                result = Current?.Execute(this) ?? ExecutionResult.Done;
            }
            catch (Exception ex)
            {
                Console.Error(DetailedStackTrace ? ex.ToString() : ex.Message);
            }

            if (result == ExecutionResult.Done)
            {
                Guid = Guid.Empty;
                Current = null;
                Arguments = null;
                Data = null;
            }
        }

        return result;
    }

    private string Transform(string input)
    {
        return Transformer?.Transform(input) ?? input;
    }

    private ResolvedCommandItem ResolveCommand(IEnumerable<string> command, bool invert)
    {
        ResolvedCommandItem result = default;

        var cmd = command.ToArray();

        string cmdName = string.Empty;

        string[] cmdArgs = new string[] { };

        if (cmd.Length > 0)
        {
            cmdName = Transform(cmd[0]);


            if (cmdName.StartsWith('+') || cmdName.StartsWith('-'))
            {
                if (invert)
                {
                    cmdName = cmdName[0] switch
                    {
                        '+' => $"-{cmdName.Substring(1)}",
                        '-' => $"+{cmdName.Substring(1)}",
                        _ => cmdName
                    };
                }
                Invertible = true;
            }
            else if (cmdName.EndsWith('+') || cmdName.EndsWith('-'))
            {
                if (invert)
                {
                    cmdName = cmdName[^1] switch
                    {
                        '+' => $"-{cmdName.Substring(0, cmdName.Length - 1)}",
                        '-' => $"+{cmdName.Substring(0, cmdName.Length - 1)}",
                        _ => cmd[0]
                    };
                }
                else
                {
                    cmdName = cmdName[^1] switch
                    {
                        '+' => $"+{cmdName.Substring(0, cmdName.Length - 1)}",
                        '-' => $"-{cmdName.Substring(0, cmdName.Length - 1)}",
                        _ => cmdName
                    };
                }
            }

            cmdArgs = cmd.Skip(1).ToArray();

            IExecutable exec = Console.GetCommandOrVariable(cmdName);

            if (exec != null)
            {
                result = new ResolvedCommandItem(exec, cmdName, cmdArgs);
            }
        }

        if (result?.executable == null)
        {
            result = new ResolvedCommandItem(null, cmdName, cmdArgs);
        }

        return result;
    }

    private void ProcessCommand(IEnumerable<string> command, bool invert)
    {
        var resolvedCmd = ResolveCommand(command, invert);

        if (string.IsNullOrWhiteSpace(resolvedCmd.command))
        {
            return;
        }

        if (resolvedCmd.executable == null)
        {
            var aliasCommand = Console.GetAlias(resolvedCmd.command);

            if (string.IsNullOrEmpty(aliasCommand))
            {
                if (resolvedCmd.command.StartsWith('+') || resolvedCmd.command.StartsWith('-'))
                {
                    aliasCommand = resolvedCmd.command.Substring(1);
                    Invertible = true;
                }
                else if (resolvedCmd.command.EndsWith('+') || resolvedCmd.command.EndsWith('-'))
                {
                    aliasCommand = resolvedCmd.command.Substring(0, resolvedCmd.command.Length - 1);
                }
            }

            if (!string.IsNullOrEmpty(aliasCommand))
            {
                AddToCommandQueue(aliasCommand, invert, true);
            }
            else
            {
                throw new CommandNotFoundException(resolvedCmd.command);
            }
        }
        else
        {
            AddToExecutionQueue(resolvedCmd);
        }
    }

    public void Execute(IExecutable executable, string[] args)
    {
        if (executable != null)
        {
            ExecutionQueue.Push(new ExecutionQueueItem(Guid.NewGuid(), executable, args));
        }
    }

    public void ExecuteHelp(IExecutable executable, string[] args)
    {
        if (executable != null)
        {
            var help = Console.GetCommand("help");
            var argsList = new List<string>
            {
                executable.GetName()
            };
            argsList.AddRange(args);
            ExecutionQueue.Push(new ExecutionQueueItem(Guid.NewGuid(), help, argsList.ToArray()));
        }
    }

    public void Execute(string cmd)
    {
        AddToCommandQueue(cmd, Invert, true);
    }

    public void ExecuteFile(string file)
    {
        if (!FA.FileExists(file))
        {
            string fileAppended = file;

            if (!file.EndsWith(".cfg", StringComparison.OrdinalIgnoreCase))
            {
                fileAppended = $"{file}.cfg";
            }

            if (!FA.FileExists(fileAppended))
            {
                throw new FileNotFoundException($"File '{file}' could not be found");
            }
            else
            {
                file = fileAppended;
            }
        }

        if (!file.EndsWith(".cfg", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Can only execute .cfg files");
        }

        var openedFile = FA.Open(file, FA.ModeFlags.Read);

        string input = openedFile.GetAsText();

        var lines = new List<string>();

        if (!string.IsNullOrWhiteSpace(input))
        {
            using var textReader = new StringReader(input);

            string line = null;

            while ((line = textReader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                lines.Add(line);
            }
        }

        if (lines.Count > 0)
        {
            for (int i = lines.Count - 1; i >= 0; i--)
            {
                AddToCommandQueue(lines[i], Invert, true);
            }
        }
    }
}