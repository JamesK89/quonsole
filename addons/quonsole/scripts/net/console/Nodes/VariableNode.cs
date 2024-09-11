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

namespace Quonsole.Variables;

public class VariableNode : BaseInternalVariable, IExecutableNode
{ 
    public const string ExecuteFunctionName = "_on_executed";
    public const string ExecuteHelpFunctionName = "_on_help_executed";
    public const string GetVariableFunctionName = "get_value";
    public const string SetVariableFunctionName = "set_value";

    public const string ValueChangedSignalName = "value_changed";
    public const string HelpExecutedSignalName = "help_executed";
    public const string ExecutedSignalName = "executed";

    public const string GuidPropertyName = "_guid";

    public VariableNode(IConsoleCore console, Node node)
        : base()
    {
        Node = node;
        Console = console;

        Node.Set(GuidPropertyName, Guid.ToString());

        Node.Connect(ValueChangedSignalName, Callable.From((GodotObject sender, Variant value) =>
        {
            RaiseChangedEvent(value);
        }));

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

    public override Variant Get()
    {
        if (!Node.HasMethod(GetVariableFunctionName))
        {
            throw new NodeFunctionNotImplemented(GetName(), GetVariableFunctionName);
        }

        return Node.Call(GetVariableFunctionName);
    }

    public override void Set(Variant value)
    {
        if (!Node.HasMethod(SetVariableFunctionName))
        {
            throw new NodeFunctionNotImplemented(GetName(), SetVariableFunctionName);
        }

        Node.Call(SetVariableFunctionName, value);
    }

    public override ExecutionResult Execute(IExecutionContext context)
    {
        if (!Node.HasMethod(ExecuteFunctionName))
        {
            throw new NodeFunctionNotImplemented(GetName(), ExecuteFunctionName);
        }

        var result = Node.Call(ExecuteFunctionName, context.Delta, (Dictionary<string, Variant>)context.Data, context.Arguments.ToGodotArray());

        return result.VariantType == Variant.Type.Nil || result.AsBool() ? ExecutionResult.Done : ExecutionResult.Continue;
    }

    public override ExecutionResult ExecuteHelp(IExecutionContext context)
    {
        if (!Node.HasMethod(ExecuteHelpFunctionName))
            throw new NodeFunctionNotImplemented(GetName(), ExecuteHelpFunctionName);

        var result = Node.Call(ExecuteHelpFunctionName, context.Delta, (Dictionary<string, Variant>)context.Data, context.Arguments.ToGodotArray());

        return result.VariantType == Variant.Type.Nil || result.AsBool() ? ExecutionResult.Done : ExecutionResult.Continue;
    }
}