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
using Quonsole.Core;
using Quonsole.Exceptions;
using Quonsole.Extensions;
using Quonsole.Interfaces;

namespace Quonsole.Commands;

public class CommandNode : BaseInternalCommand, IExecutableNode
{
	public const string ExecuteCommandFunctionName = "_on_executed";
	public const string ExecuteHelpCommandFunctionName = "_on_help_executed";

	public const string HelpExecutedSignalName = "help_executed";
	public const string ExecutedSignalName = "executed";

	public const string GuidPropertyName = "_guid";

	public CommandNode(IConsoleCore console, Node node) :
		base()
	{
		Node = node;
		Console = console;

		Node.Set(GuidPropertyName, Guid.ToString());

		Node.Connect(ExecutedSignalName, Callable.From((GodotObject sender, string guid, double delta, Dictionary data, Array arguments) =>
		{
			IExecutionContext ctx = null;

			if (data.ContainsKey(ExecutionContext.ExecutionContextGuidKey))
			{
				ctx = Console.GetExecutionContextByGuid(data[ExecutionContext.ExecutionContextGuidKey].ToString());
			}

			RaiseExecutedEvent(ctx);
		}));

		Node.Connect(HelpExecutedSignalName, Callable.From((GodotObject sender, string guid, double delta, Dictionary data, Array arguments) =>
		{
			IExecutionContext ctx = null;

			if (data.ContainsKey(ExecutionContext.ExecutionContextGuidKey))
			{
				ctx = Console.GetExecutionContextByGuid(data[ExecutionContext.ExecutionContextGuidKey].ToString());
			}

			RaiseHelpEvent(ctx);
		}));
	}

	public IConsoleCore Console { get; set; }

	public Node Node { get; }

	public Node GetNode() => Node;

	public override string GetName()
	{
		return ((string)Node.Name).Trim().ToLowerInvariant();
	}

	public override ExecutionResult ExecuteHelp(IExecutionContext context)
	{
		if (!Node.HasMethod(ExecuteHelpCommandFunctionName))
			throw new NodeFunctionNotImplemented(GetName(), ExecuteHelpCommandFunctionName);

		var result = Node.Call(ExecuteHelpCommandFunctionName, context.Delta, (Dictionary<string, Variant>)context.Data, context.Arguments.ToGodotArray());

		return result.VariantType == Variant.Type.Nil || result.AsBool() ? ExecutionResult.Done : ExecutionResult.Continue;
	}

	public override ExecutionResult Execute(IExecutionContext context)
	{
		if (!Node.HasMethod(ExecuteCommandFunctionName))
		{
			throw new NodeFunctionNotImplemented(GetName(), ExecuteCommandFunctionName);
		}

		var result = Node.Call(ExecuteCommandFunctionName, context.Delta, (Dictionary<string, Variant>)context.Data, context.Arguments.ToGodotArray());

		return result.VariantType == Variant.Type.Nil || result.AsBool() ? ExecutionResult.Done : ExecutionResult.Continue;
	}
}
