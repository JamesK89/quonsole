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
using System.Collections.Generic;
using Quonsole.Core;
using Quonsole.Interfaces;
using Quonsole.Extensions;
using Quonsole.Variables;
using Quonsole.Commands;
using System.Linq;

namespace Quonsole;

public partial class ConsoleControl : Control
{
	public const int HistoryLength = 64;

	private LineEdit _inputControl;
	private RichTextLabel _outputControl;

	private IConsoleCore _core = new ConsoleCore();

	private List<string> _history = new List<string>(HistoryLength);

	private Dictionary<string, Variant> _cmdWrappers = new Dictionary<string, Variant>();
	private Dictionary<string, Variant> _varWrappers = new Dictionary<string, Variant>();

	private DebugStackTraceVariable _debugStackTrace;

	private int _historyIndex = 0;

	private static GDScript GdVariableObjectWrapper = GD.Load<GDScript>("res://addons/quonsole/scripts/gd/console/console_variable_internal.gd");
	private static GDScript GdCommandObjectWrapper = GD.Load<GDScript>("res://addons/quonsole/scripts/gd/console/console_command_internal.gd");

	// Sometimes messages are sent before the console is ready
	private static List<(ConsoleMessageLevel level, string text)> _pendingMessages = new List<(ConsoleMessageLevel, string)>();

	private bool DetailedStackTrace
	{
		get => _debugStackTrace?.Enabled ?? false;

    }

	public override void _Ready()
	{
		_core.OnClear += (sender) => ClearOutput();
		_core.OnInfo += (sender, level, message) => PrintInfo(message);
		_core.OnWarning += (sender, level, message) => PrintWarning(message);
		_core.OnError += (sender, level, message) => PrintError(message);
		_core.OnDebug += (sender, level, message) => PrintDebug(message);

		VisibilityChanged += OnVisibilityChanged;

		_outputControl = FindChild("console_output", true) as RichTextLabel;

		if (_outputControl != null)
		{
			_outputControl.Text = String.Empty;

			OutputPendingMessages();
		}

		_inputControl = FindChild("console_input", true) as LineEdit;

		if (_inputControl != null)
		{
			_inputControl.Text = String.Empty;
			_inputControl.TextSubmitted += OnInputTextSubmitted;
			_inputControl.GrabFocus();
		}

		_debugStackTrace = _core.GetVariable("dbg_stacktrace") as DebugStackTraceVariable;
	}

	public override void _Input(InputEvent @event)
	{
		if (_inputControl != null && _inputControl.HasFocus())
		{
			if (Input.IsActionJustPressed("ui_up"))
			{
				HistoryUp();
			}
			else if (Input.IsActionJustPressed("ui_down"))
			{
				HistoryDown();
			}
		}
	}

	public override void _Process(double delta)
	{
		try
		{
			_core.Step(delta);
		}
		catch (Exception ex)
		{
			PrintError(DetailedStackTrace ? ex.ToString() : ex.Message);
		}
	}

	private void OutputPendingMessages()
	{
		var messages = _pendingMessages.ToArray();
		_pendingMessages.Clear();

		foreach (var message in messages)
		{
			OutputVerbatim(message.level, message.text);
		}
	}

	public void HistoryUp()
	{
		_historyIndex -= 1;

		if (_historyIndex < 0)
		{
			_historyIndex = _history.Count - 1;
		}

		DisplayHistory(_historyIndex);
	}

	public void HistoryDown()
	{
		_historyIndex += 1;

		if (_historyIndex >= _history.Count)
		{
			_historyIndex = 0;
		}

		DisplayHistory(_historyIndex);
	}

	public void DisplayHistory(int idx)
	{
		string historyText = string.Empty;

		if (_history.Count > 0 && idx >= 0 && idx < _history.Count)
		{
			historyText = _history[idx];
		}

		if (_inputControl != null)
		{
			_inputControl.Text = historyText;
			_inputControl.CaretColumn = historyText.Length - 1;
		}
	}

	public void AppendHistory(string newText)
	{
		int idx = _history.FindIndex((s) => string.Compare(s, newText, false) == 0);

		if (idx > -1)
		{
			_history.RemoveAt(idx);
		}

		_history.Add(newText);

		if (_history.Count >= HistoryLength)
		{
			_history.RemoveRange(0, _history.Count - HistoryLength);
		}

		_historyIndex = _history.Count;
	}

	private void OnVisibilityChanged()
	{
		if (this.Visible)
		{
			if (_inputControl != null)
			{
				_inputControl.SetProcessInput(true);
				_inputControl.GrabFocus();
			}
			MouseFilter = MouseFilterEnum.Stop;
		}
		else
		{
			if (_inputControl != null)
			{
				_inputControl.SetProcessInput(false);
				_inputControl.ReleaseFocus();
			}
			MouseFilter = MouseFilterEnum.Ignore;
		}
	}

	private Variant GetCommandWrapper(IExecutable command)
	{
		if (_cmdWrappers.ContainsKey(command.Guid))
		{
			return _cmdWrappers[command.Guid];
		}

		if (command is CommandNode node)
		{
			var ret = Variant.From(node.GetNode());
			_cmdWrappers.Add(command.Guid, ret);
			return ret;
		}

		var wrapped = GdCommandObjectWrapper.New(new ConsoleCommandWrapper(_core, command));
		_cmdWrappers.Add(command.Guid, wrapped);

		return wrapped;
	}

	private Variant GetVariableWrapper(IVariable variable)
	{
		if (_varWrappers.ContainsKey(variable.Guid))
		{
			return _varWrappers[variable.Guid];
		}

		if (variable is VariableNode node)
		{
			var ret = Variant.From(node.GetNode());
            _varWrappers.Add(variable.Guid, ret);
            return ret;
        }

		var wrapped = GdVariableObjectWrapper.New(new ConsoleVariableWrapper(_core, variable));
		_varWrappers.Add(variable.Guid, wrapped);

		return wrapped;
	}

	public Godot.Collections.Array GetVariables()
	{
		var variables = _core.Variables;

		var result = new List<Variant>();

		foreach (var variable in variables)
		{
			result.Add(GetVariableWrapper(variable));
		}

		return result.ToGodotArray();
	}

	public Variant GetVariable(string name)
	{
		var variable = _core.GetVariable(name);

		if (variable != null)
		{
			return GetVariableWrapper(variable);
		}

		return Variant.CreateFrom(null as string);
	}

	public Godot.Collections.Array GetCommands()
	{
		var commands = _core.Commands;

		var result = new List<Variant>();

		foreach (var command in commands)
		{
			result.Add(GetCommandWrapper(command));
		}

		return result.ToGodotArray();
	}

	public Variant GetCommand(string name)
	{
		var command = _core.GetCommand(name);

		if (command != null)
		{
			return GetCommandWrapper(command);
		}

		return Variant.CreateFrom(null as string);
	}

	public Variant GetCommandOrVariable(string name)
	{
		var command = _core.GetCommandOrVariable(name);

		if (command != null)
		{
			if (command is IVariable variable)
			{
				return GetVariableWrapper(variable);
			}
			else if (command is IExecutable executable)
			{
				return GetCommandWrapper(executable);
			}
		}

		return Variant.CreateFrom(null as string);
	}

	public string Execute(string cmd)
	{
		try
		{
			return _core.Execute(cmd).ToString();
		}
		catch (Exception ex)
        {
            PrintError(DetailedStackTrace ? ex.ToString() : ex.Message);
        }

		return string.Empty;
	}

	public string ExecutePersist(string cmd)
	{
		var guid = string.Empty;

		try
		{
			guid = _core.Execute(cmd, true, false).ToString();
		}
		catch (Exception ex)
        {
            PrintError(DetailedStackTrace ? ex.ToString() : ex.Message);
        }

		return guid;
	}

	public string ExecuteInverted(string cmd)
	{
		try
		{
			return _core.Execute(cmd, false, true).ToString();
		}
		catch (Exception ex)
        {
            PrintError(DetailedStackTrace ? ex.ToString() : ex.Message);
        }

		return string.Empty;
	}

	public string ExecuteInvertedPersist(string cmd)
	{
		try
		{
			return _core.Execute(cmd, true, true).ToString();
		}
		catch (Exception ex)
        {
            PrintError(DetailedStackTrace ? ex.ToString() : ex.Message);
        }

		return string.Empty;
	}

	public string ExecuteCommand(Node node, Godot.Collections.Array arguments)
	{
		try
		{
			string guid = node.Get("guid").ToString();
			return _core.Execute(guid, arguments.FromGodotArray<string>().ToArray()).ToString();
		}
		catch (Exception ex)
        {
            PrintError(DetailedStackTrace ? ex.ToString() : ex.Message);
        }

		return string.Empty;
	}

	public string ExecuteCommandPersist(Node node, Godot.Collections.Array arguments)
	{
		try
		{
			string guid = node.Get("guid").ToString();
			return _core.Execute(guid, arguments.FromGodotArray<string>().ToArray(), true).ToString();
		}
		catch (Exception ex)
        {
            PrintError(DetailedStackTrace ? ex.ToString() : ex.Message);
        }

		return string.Empty;
	}

	public string ExecuteHelp(Node node, Godot.Collections.Array arguments)
	{
		try
		{
			string guid = node.Get("guid").ToString();
			return _core.ExecuteHelp(guid, arguments.FromGodotArray<string>().ToArray()).ToString();
		}
		catch (Exception ex)
        {
            PrintError(DetailedStackTrace ? ex.ToString() : ex.Message);
        }

		return string.Empty;
	}

	public string ExecuteHelpPersist(Node node, Godot.Collections.Array arguments)
	{
		try
		{
			string guid = node.Get("guid").ToString();
			return _core.ExecuteHelp(guid, arguments.FromGodotArray<string>().ToArray(), true).ToString();
		}
		catch (Exception ex)
        {
            PrintError(DetailedStackTrace ? ex.ToString() : ex.Message);
        }

		return string.Empty;
	}

	public string ExecuteFile(string file)
	{
		try
		{
			return _core.ExecuteFile(file).ToString();
		}
		catch (Exception ex)
        {
            PrintError(DetailedStackTrace ? ex.ToString() : ex.Message);
        }

		return string.Empty;
	}

	public string ExecuteFilePersist(string file)
	{
		try
		{
			return _core.ExecuteFile(file, true, false).ToString();
		}
		catch (Exception ex)
        {
            PrintError(DetailedStackTrace ? ex.ToString() : ex.Message);
        }

		return string.Empty;
	}

	public string ExecuteFileInverted(string file)
	{
		try
		{
			return _core.ExecuteFile(file, false, true).ToString();
		}
		catch (Exception ex)
        {
            PrintError(DetailedStackTrace ? ex.ToString() : ex.Message);
        }

		return string.Empty;
	}

	public string ExecuteFileInvertedPersist(string file)
	{
		try
		{
			return _core.ExecuteFile(file, true, true).ToString();
		}
		catch (Exception ex)
        {
            PrintError(DetailedStackTrace ? ex.ToString() : ex.Message);
        }

		return string.Empty;
	}

	public void Info(string message)
	{
		_core.Info(message);
	}

	public void Warning(string message)
	{
		_core.Warning(message);
	}

	public void Error(string message)
	{
		_core.Error(message);
	}

	public void Debug(string message)
	{
		_core.Debug(message);
	}

	public void Clear()
	{
		_core.Clear();
	}

	public string RegisterVariable(Node node)
	{
		var variable = new VariableNode(_core, node);
		_core.RegisterVariable(variable);

		return variable.Guid;
	}

	public string RegisterCommand(Node node)
	{
		var command = new CommandNode(_core, node);
		_core.RegisterCommand(command);

		return command.Guid;
	}

	public Godot.Collections.Dictionary GetExecutionContext(string guid)
	{
		var context = _core.GetExecutionContextByGuid(guid);

		return new Godot.Collections.Dictionary
		{
			{ "guid", (context?.Guid ?? Guid.Empty).ToString() },
			{ "is_invertible", context?.Invertible ?? false },
			{ "is_executing", context?.IsExecuting ?? false },
			{ "is_persistent", context?.Persist ?? false },
			{ "is_finished", context?.IsFinished ?? false }
		};
	}

	public void ClearExecutionContext(string guid)
	{
		_core.ClearExecutionContextByGuid(guid);
	}

	private void OnInputTextSubmitted(string newText)
	{
		if (string.IsNullOrWhiteSpace(newText))
		{
			return;
		}

		AppendHistory(newText);

		if (_inputControl != null)
		{
			_inputControl.Text = string.Empty;
		}

		if (_outputControl != null)
		{
			_outputControl.AppendText($"> {FilterText(newText)}\n");
		}

		Execute(newText);
	}

	private void OutputVerbatim(ConsoleMessageLevel level, string text)
	{
		if (_outputControl != null)
		{
			_outputControl.AppendText(text);
		}
		else
		{
			_pendingMessages.Add((level, text));
		}
	}

	private void PrintInfo(string text)
	{
		if (!string.IsNullOrWhiteSpace(text))
		{
			GD.PrintRich($"[color=white]Console: {text}[/color]");
			OutputVerbatim(ConsoleMessageLevel.Info, $"[color=white]{text?.Trim()}[/color]\n");
		}
	}

	private void PrintWarning(string text)
	{
		if (!string.IsNullOrWhiteSpace(text))
		{
			GD.PrintRich($"[color=yellow]Console Warning: {text}[/color]");
			OutputVerbatim(ConsoleMessageLevel.Warning, $"[color=yellow]Warning: {text?.Trim()}[/color]\n");
		}
	}

	private void PrintError(string text)
	{
		if (!string.IsNullOrWhiteSpace(text))
		{
			GD.PrintRich($"[color=red]Console Error: {text}[/color]");
			OutputVerbatim(ConsoleMessageLevel.Error, $"[color=red]Error: {text?.Trim()}[/color]\n");
		}
	}

	private void PrintDebug(string text)
	{
		if (!string.IsNullOrWhiteSpace(text))
		{
			GD.PrintRich($"[color=magenta]Console Debug: {text}[/color]");
			OutputVerbatim(ConsoleMessageLevel.Debug, $"[color=magenta]Debug: {text?.Trim()}[/color]\n");
		}
	}

	private string FilterText(string text)
	{
		return text.Replace("[", "[lb]");
	}

	private void ClearOutput()
	{
		if (_outputControl != null)
		{
			_outputControl.Text = "";
		}
	}
}
