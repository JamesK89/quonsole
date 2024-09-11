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
using Quonsole.Interfaces;

namespace Quonsole.Core;

public class ExecutionData : Dictionary<string, Variant>, IExecutionData
{
    Variant IExecutionData.this[string key] { get => this[key]; set => this[key] = value; }

    void IExecutionData.Add(string key, Variant value)
    {
        this.Add(key, value);
    }

    bool IExecutionData.ContainsKey(string key)
    {
        return this.ContainsKey(key);
    }

    Variant IExecutionData.Get(string key)
    {
        return this[key];
    }

    void IExecutionData.Remove(string key)
    {
        this.Remove(key);
    }

    void IExecutionData.Set(string key, Variant value)
    {
        this[key] = value;
    }
}