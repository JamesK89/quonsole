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
using Quonsole.Interfaces;
using Quonsole.Attributes;
using System.Linq;
using System.Reflection;
using System;

namespace Quonsole.Repository;

public class CommandRepository : ICommandRepository
{
    private Dictionary<string, IExecutable> _commands = new Dictionary<string, IExecutable>();
    private Dictionary<string, IVariable> _variables = new Dictionary<string, IVariable>();
    private Dictionary<string, string> _aliases = new Dictionary<string, string>();

    private Dictionary<string, IExecutable> _guidMap = new Dictionary<string, IExecutable>();

    public IEnumerable<IExecutable> Commands => _commands.Values;

    public IEnumerable<IVariable> Variables => _variables.Values;

    public IReadOnlyDictionary<string, string> Aliases => _aliases;

    public event CommandRepositoryEventHandler CommandRegistered;
    public event CommandRepositoryEventHandler CommandUnregistered;
    public event CommandRepositoryEventHandler VariableRegistered;
    public event CommandRepositoryEventHandler VariableUnregistered;
    public event CommandRepositoryEventHandler AliasRegistered;
    public event CommandRepositoryEventHandler AliasUnregistered;

    public CommandRepository()
    {
        AddBuiltInCommands();
        AddBuiltInVariables();
    }

    public IExecutable GetExecutableByGuid(string guid)
    {
        return _guidMap[guid];
    }

    public IExecutable GetCommandOrVariable(string name)
    {
        var sanitized = SanitizeName(name);

        if (_commands.TryGetValue(sanitized, out var command))
        {
            return command;
        }
        else if (_variables.TryGetValue(sanitized, out var variable))
        {
            return variable;
        }

        return null;
    }

    public string GetAlias(string name)
    {
        var sanitized = SanitizeName(name);
        return _aliases.ContainsKey(sanitized) ? _aliases[sanitized] : null;
    }

    public void RegisterAlias(string name, string command)
    {
        var sanitized = SanitizeName(name);
        _aliases.Add(sanitized, command);
        AliasRegistered?.Invoke(this, sanitized);
    }

    public void UnregisterAlias(string name)
    {
        var sanitized = SanitizeName(name);
        _aliases.Remove(sanitized);
        AliasUnregistered?.Invoke(this, sanitized);
    }

    public IExecutable GetCommand(string name)
    {
        var sanitized = SanitizeName(name);
        return _commands.ContainsKey(sanitized) ? _commands[sanitized] : null;
    }

    public void RegisterCommand(IExecutable command)
    {
        var sanitized = SanitizeName(command.GetName());
        _commands.Add(sanitized, command);
        _guidMap.Add(command.Guid, command);
        CommandRegistered?.Invoke(this, sanitized);
    }

    public void UnregisterCommand(string name)
    {
        var sanitized = SanitizeName(name);
        _guidMap.Remove(_commands[sanitized].Guid);
        _commands.Remove(sanitized);
        CommandUnregistered?.Invoke(this, sanitized);
    }

    public IVariable GetVariable(string name)
    {
        var sanitized = SanitizeName(name);
        return _variables.ContainsKey(sanitized) ? _variables[sanitized] : null;
    }

    public void RegisterVariable(IVariable variable)
    {
        var sanitized = SanitizeName(variable.GetName());
        _variables.Add(sanitized, variable);
        _guidMap.Add(variable.Guid, variable);
        VariableRegistered?.Invoke(this, sanitized);
    }

    public void UnregisterVariable(string name)
    {
        var sanitized = SanitizeName(name);
        _guidMap.Remove(_variables[sanitized].Guid);
        _variables.Remove(sanitized);
        VariableUnregistered?.Invoke(this, sanitized);
    }

    private string SanitizeName(string name)
    {
        return name?.Trim().ToUpperInvariant() ?? string.Empty;
    }

    private IEnumerable<TOut> AddBuiltIn<TAttribute, TOut>()
        where TAttribute : Attribute
        where TOut : class, IExecutable
    {
        var result = new List<TOut>();

        this.GetType().Assembly.GetTypes()
            .Where(t => t.GetCustomAttribute<TAttribute>() != null)
            .ToList()
            .ForEach(t =>
            {
                var instanceProperty = t.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
                TOut instance = null;

                if (instanceProperty != null)
                {
                    instance = instanceProperty.GetValue(null) as TOut;
                }
                else
                {
                    instance = Activator.CreateInstance(t) as TOut;
                }

                if (instance != null)
                {
                    result.Add(instance);
                }
            });

        return result;
    }

    private void AddBuiltInCommands()
    {
        AddBuiltIn<CommandAttribute, IExecutable>()
            .ToList()
            .ForEach(c => RegisterCommand(c));
    }

    private void AddBuiltInVariables()
    {
        AddBuiltIn<VariableAttribute, IVariable>()
            .ToList()
            .ForEach(v => RegisterVariable(v));
    }
}