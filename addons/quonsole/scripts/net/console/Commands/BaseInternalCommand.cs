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

using Quonsole.Core;
using Quonsole.Interfaces;
using System;

namespace Quonsole.Commands;

public abstract class BaseInternalCommand : IExecutable
{
	public event CommandExecutedEventHandler Executed;
	public event CommandExecutedEventHandler HelpExecuted;

	public BaseInternalCommand()
	{
		Guid = System.Guid.NewGuid().ToString();
	}

	public string Guid
	{
		get;
		private set;
	}

	public virtual ExecutionResult ExecuteHelp(IExecutionContext context)
	{
		throw new NotImplementedException();
	}

	public virtual ExecutionResult Execute(IExecutionContext context)
	{
		throw new NotImplementedException();
	}

	public virtual string GetName()
	{
		throw new NotImplementedException();
	}

	protected virtual void RaiseExecutedEvent(IExecutionContext context)
	{
		Executed?.Invoke(this, new CommandEventHandlerArgs()
		{
			Context = context,
			Guid = Guid,
			Arguments = context?.Arguments ?? System.Array.Empty<string>(),
			Delta = context?.Delta ?? 0,
			Executable = this
		});
	}

	protected virtual void RaiseHelpEvent(IExecutionContext context)
	{
		HelpExecuted?.Invoke(this, new CommandEventHandlerArgs()
		{
			Context = context,
			Guid = Guid,
			Arguments = context?.Arguments ?? System.Array.Empty<string>(),
			Delta = context?.Delta ?? 0,
			Executable = this
		});
	}
}
