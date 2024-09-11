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

using System.Collections.Generic;

namespace Quonsole.Interfaces;

public delegate void CommandRepositoryEventHandler(ICommandRepository sender, string name);

public interface ICommandRepository
{
    event CommandRepositoryEventHandler CommandRegistered;
    event CommandRepositoryEventHandler CommandUnregistered;

    event CommandRepositoryEventHandler VariableRegistered;
    event CommandRepositoryEventHandler VariableUnregistered;

    event CommandRepositoryEventHandler AliasRegistered;
    event CommandRepositoryEventHandler AliasUnregistered;

    IExecutable GetExecutableByGuid(string guid);

    IEnumerable<IExecutable> Commands { get; }

    IExecutable GetCommand(string name);
    void RegisterCommand(IExecutable command);
    void UnregisterCommand(string name);

    IEnumerable<IVariable> Variables { get; }

    IVariable GetVariable(string name);
    void RegisterVariable(IVariable variable);
    void UnregisterVariable(string name);

    IExecutable GetCommandOrVariable(string name);

    IReadOnlyDictionary<string, string> Aliases { get; }

    string GetAlias(string name);
    void RegisterAlias(string name, string command);
    void UnregisterAlias(string name);
}