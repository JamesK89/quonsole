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
using Quonsole.Repository;
using Quonsole.Transformer;
using Quonsole.Variables;
using System;
using System.Collections.Generic;

namespace Quonsole.Core;

public class ConsoleCore : IConsoleCore
{
	public event ConsoleEventHandler OnClear;

	public event ConsoleMessageEventHandler OnInfo;
	public event ConsoleMessageEventHandler OnError;
	public event ConsoleMessageEventHandler OnWarning;
	public event ConsoleMessageEventHandler OnDebug;

	private readonly List<IExecutionContext> _executionContexts = new List<IExecutionContext>();
	private readonly Dictionary<Guid, IExecutionContext> _executionContextGuidMap = new Dictionary<Guid, IExecutionContext>();

	private DebugStackTraceVariable _debugStackTrace = null;

	private readonly ICommandRepository _commandRepository = new CommandRepository();

	public IEnumerable<IExecutable> Commands => _commandRepository.Commands;

	public IEnumerable<IVariable> Variables => _commandRepository.Variables;

	public IReadOnlyDictionary<string, string> Aliases => _commandRepository.Aliases;

	public IStringTransformer Transformer { get; private set; }

	public ConsoleCore()
	{
		Transformer = new StringTransformer(_commandRepository);
		_debugStackTrace = _commandRepository.GetVariable("dbg_stacktrace") as DebugStackTraceVariable;
	}

	private bool DetailedStackTrace
	{
		get => _debugStackTrace?.Enabled ?? false;
	}

	public void Clear()
	{
		OnClear?.Invoke(this);
	}

	public Guid Execute(IExecutable executable, string[] args, bool persist = false)
	{
		var result = Guid.Empty;

		try
		{
			var exec = CreateExecutionContext(persist, false);
			result = exec.Guid;
			exec.Execute(executable, args);
		}
		catch (Exception ex)
		{
			Error(DetailedStackTrace ? ex.ToString() : ex.Message);
		}

		return result;
	}

	public Guid Execute(string guid, string[] args, bool persist = false)
	{
		try
		{
			var executable = _commandRepository.GetExecutableByGuid(guid);

			if (executable == null)
			{
				throw new Exception($"Command with GUID '{guid}' not found.");
			}

			return Execute(executable, args, persist);
		}
		catch (Exception ex)
        {
            Error(DetailedStackTrace ? ex.ToString() : ex.Message);
        }

		return Guid.Empty;
	}

	public Guid ExecuteHelp(IExecutable executable, string[] args, bool persist = false)
	{
		var result = Guid.Empty;

		try
		{
			var exec = CreateExecutionContext(persist, false);
			result = exec.Guid;
			exec.ExecuteHelp(executable, args);
		}
		catch (Exception ex)
        {
            Error(DetailedStackTrace ? ex.ToString() : ex.Message);
        }

		return result;
	}

	public Guid ExecuteHelp(string guid, string[] args, bool persist = false)
	{
		try
		{
			var executable = _commandRepository.GetExecutableByGuid(guid);

			if (executable == null)
			{
				throw new Exception($"Command with GUID '{guid}' not found.");
			}

			return ExecuteHelp(executable, args, persist);
		}
		catch (Exception ex)
        {
            Error(DetailedStackTrace ? ex.ToString() : ex.Message);
        }

		return Guid.Empty;
	}

	public Guid Execute(string cmd, bool persist = false, bool invert = false)
	{
		var result = Guid.Empty;

		try
		{
			var exec = CreateExecutionContext(persist, invert);
			result = exec.Guid;
			exec.Execute(cmd);
		}
		catch (Exception ex)
        {
            Error(DetailedStackTrace ? ex.ToString() : ex.Message);
        }

		return result;
	}

	public Guid ExecuteFile(string file, bool persist = false, bool invert = false)
	{
		var result = Guid.Empty;

		try
		{
			var exec = CreateExecutionContext(persist, invert);
			result = exec.Guid;
			exec.ExecuteFile(file);
		}
		catch (Exception ex)
        {
            Error(DetailedStackTrace ? ex.ToString() : ex.Message);
        }

		return result;
	}

	public void Info(string message)
	{
		OnInfo?.Invoke(this, ConsoleMessageLevel.Info, message);
	}

	public void Warning(string message)
	{
		OnWarning?.Invoke(this, ConsoleMessageLevel.Warning, message);
	}

	public void Error(string message)
	{
		OnError?.Invoke(this, ConsoleMessageLevel.Error, message);
	}

	public void Debug(string message)
	{
		OnDebug?.Invoke(this, ConsoleMessageLevel.Debug, message);
	}

	public IExecutable GetCommandOrVariable(string name)
	{
		return _commandRepository.GetCommandOrVariable(name);
	}

	public string GetAlias(string name)
	{
		return _commandRepository.GetAlias(name);
	}

	public void RegisterAlias(string name, string command)
	{
		_commandRepository.RegisterAlias(name, command);
	}

	public void UnregisterAlias(string name)
	{
		_commandRepository.UnregisterAlias(name);
	}

	public IExecutable GetCommand(string name)
	{
		return _commandRepository.GetCommand(name);
	}

	public void RegisterCommand(IExecutable command)
	{
		_commandRepository.RegisterCommand(command);
	}

	public void UnregisterCommand(string name)
	{
		_commandRepository.UnregisterCommand(name);
	}

	public IVariable GetVariable(string name)
	{
		return _commandRepository.GetVariable(name);
	}

	public void RegisterVariable(IVariable variable)
	{
		_commandRepository.RegisterVariable(variable);
	}

	public void UnregisterVariable(string name)
	{
		_commandRepository.UnregisterVariable(name);
	}

	public void Step(double delta)
	{
		for (int i = _executionContexts.Count - 1; i >= 0; i--)
		{
			var context = _executionContexts[i];

			if (context.Step(delta) == ExecutionResult.Done && !context.Persist)
			{
				_executionContextGuidMap.Remove(context.Guid);
				_executionContexts.RemoveAt(i);
			}
		}
	}

	private IExecutionContext CreateExecutionContext(bool persist = false, bool invert = false)
	{
		var context = new ExecutionContext(this, invert, persist);
		_executionContexts.Add(context);
		_executionContextGuidMap.Add(context.Guid, context);
		return context;
	}

	public IExecutionContext GetExecutionContextByGuid(Guid guid)
	{
		if (_executionContextGuidMap.ContainsKey(guid))
		{
			return _executionContextGuidMap[guid];
		}

		return null;
	}

	public IExecutionContext GetExecutionContextByGuid(string guid)
	{
		return GetExecutionContextByGuid(Guid.Parse(guid));
	}

	public void ClearExecutionContextByGuid(string guid)
	{
		ClearExecutionContextByGuid(Guid.Parse(guid));
	}

	public void ClearExecutionContextByGuid(Guid guid)
	{
		if (_executionContextGuidMap.ContainsKey(guid))
		{
			_executionContextGuidMap.Remove(guid);
			for (int i = 0; i < _executionContexts.Count; i++)
			{
				if (_executionContexts[i].Guid == guid)
				{
					_executionContexts.RemoveAt(i);
					break;
				}
			}
		}
	}
}
