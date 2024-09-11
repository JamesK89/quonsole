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
using System;
using Quonsole.Core;
using Quonsole.Commands;
using Quonsole.Interfaces;
using Quonsole.Exceptions;

namespace Quonsole.Variables;

public abstract class BaseInternalVariable : BaseInternalCommand, IVariable
{
    public event VariableChangedEventHandler Changed;

    public virtual Variant Get()
    {
        throw new NotImplementedException();
    }

    public virtual void Set(Variant value)
    {
        throw new NotImplementedException();
    }

    public string AsString()
    {
        return Get().AsString();
    }

    public bool AsBool()
    {
        var v = Get().AsString().ToUpperInvariant() ?? string.Empty;

        if (string.Compare(v, "TRUE", true) == 0 ||
            string.Compare(v, "T", true) == 0 ||
            string.Compare(v, "YES", true) == 0 ||
            string.Compare(v, "Y", true) == 0)
        {
            return true;
        }

        if (AsInt() != 0)
        {
            return true;
        }

        return false;
    }

    public int AsInt()
    {
        return Get().AsInt32();
    }

    public float AsFloat()
    {
        return (float)Get().AsDouble();
    }

    public double AsDouble()
    {
        return Get().AsDouble();
    }

    public override ExecutionResult Execute(IExecutionContext context)
    {
        var argCount = context.Arguments?.Count ?? 0;

        if (argCount > 1)
            throw new TooManyArgumentsException(GetName(), argCount, 1);

        if (argCount == 0)
        {
            context.Console.Info(Get().AsString());
        }
        else
        {
            Set(Variant.From(context.Arguments[0]));
        }

        RaiseExecutedEvent(context);

        return ExecutionResult.Done;
    }

    protected void RaiseChangedEvent(Variant value)
    {
        Changed?.Invoke(this, value);
    }
}