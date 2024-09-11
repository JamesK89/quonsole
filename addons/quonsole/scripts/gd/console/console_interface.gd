extends Node
class_name ConsoleInterface

enum ConsoleAnimationState
{
	Opened,
	Opening,
	Closed,
	Closing
}

var _ui_printer: ConsolePrinterControl
var _ui_console: Control

var _console_vars: Node
var _console_cmds: Node

var _state: ConsoleAnimationState = ConsoleAnimationState.Closed
var _next_state: ConsoleAnimationState = ConsoleAnimationState.Closed

var open_duration = 0.15
var close_duration = 0.15

var _open_time = 0
var _close_time = 0

const _console_scn_vars_path = "res://addons/quonsole/scenes/console_variables.tscn"
const _console_scn_cmds_path = "res://addons/quonsole/scenes/console_commands.tscn"
const _console_scn_prnt_path = "res://addons/quonsole/scenes/console_printer.tscn"

const _console_scn_ui_path = "res://addons/quonsole/scenes/console.tscn"

# Called when the node enters the scene tree for the first time.
func _ready():
	if FileAccess.file_exists(_console_scn_vars_path):
		_console_vars = preload(_console_scn_vars_path).instantiate()
		
	if FileAccess.file_exists(_console_scn_cmds_path):
		_console_cmds = preload(_console_scn_cmds_path).instantiate()
	
	_ui_console = preload(_console_scn_ui_path).instantiate()
	_ui_console.visible = false

	assign_console_to_commands()
	assign_console_to_variables()

	if FileAccess.file_exists(_console_scn_prnt_path):
		_ui_printer = preload(_console_scn_prnt_path).instantiate()
		_ui_printer.visible = true
		setup_print_command()

	var tree = get_tree()

	add_child(_console_vars)
	add_child(_console_cmds)

	tree.root.call_deferred("add_child", _ui_console)
	tree.root.call_deferred("move_child", _ui_console, tree.root.get_child_count())
	tree.root.call_deferred("add_child", _ui_printer)
	tree.root.call_deferred("move_child", _ui_printer, tree.root.get_child_count())
	
	call_deferred("load_autoexec")

func load_autoexec() -> void:
	if FileAccess.file_exists("autoexec.cfg"):
		_ui_console.ExecuteFile("autoexec.cfg")

func setup_print_command() -> void:
	var printCmd: ConsoleCommand = get_command("print")
	printCmd.executed.connect( \
		func (_command: ConsoleCommand, _guid: String, _delta: float, _data: Dictionary, arguments: Array):
			var result_str: String = ""
			for arg in arguments:
				if (result_str.length() > 0):
					result_str += " "
				result_str += arg

			_ui_printer.print(result_str))

func _input(event: InputEvent) -> void:
	if event.is_action_pressed("Toggle Console"):
		get_viewport().set_input_as_handled()
		toggle()

func _process(_delta):
	match _state:
		ConsoleAnimationState.Opened:
			process_opened(_delta)
		ConsoleAnimationState.Opening:
			process_opening(_delta)
		ConsoleAnimationState.Closed:
			process_closed(_delta)
		ConsoleAnimationState.Closing:
			process_closing(_delta)

func assign_console_to_commands() -> void:
	if _console_cmds == null:
		return

	var nodes: Array = []
	var nodes_to_explore: Array = _console_cmds.get_children()
	
	while nodes_to_explore.size() > 0:
		var node: Node = nodes_to_explore.pop_front()
		if node is ConsoleCommand:
			nodes.append(node)
		else:
			nodes_to_explore.append_array(node.get_children())
	
	for command: ConsoleCommand in nodes:
		command._console = self
		register_command(command)
		
func assign_console_to_variables() -> void:
	if _console_vars == null:
		return

	var nodes: Array = []
	var nodes_to_explore: Array = _console_vars.get_children()
	
	while nodes_to_explore.size() > 0:
		var node: Node = nodes_to_explore.pop_front()
		if node is ConsoleVariable:
			nodes.append(node)
		else:
			nodes_to_explore.append_array(node.get_children())
	
	for variable: ConsoleVariable in nodes:
		variable._console = self
		register_variable(variable)

func process_opened(_delta: float) -> void:
	if not _ui_console.visible:
		_ui_console.visible = true
		_ui_console.position.y = 0

	_state = _next_state
	
	_ui_console.set_process_input(true)

func process_opening(delta: float) -> void:
	if not _ui_console.visible:
		_ui_console.visible = true

	_ui_console.set_process_input(false)
	
	_open_time += delta
	
	var x = clampf(_open_time / open_duration, 0.0, 1.0)

	var y = _ui_console.size.y
	_ui_console.position.y = (y * -1) + (y * x)
	
	if x >= 1.0:
		_ui_console.position.y = 0
		_state = _next_state

func process_closed(_delta: float) -> void:
	if _ui_console.visible:
		_ui_console.visible = false
		_ui_console.position.y = _ui_console.size.y * -1
		
	_ui_console.set_process_input(false)
	
	_state = _next_state

func process_closing(delta: float) -> void:
	if not _ui_console.visible:
		_ui_console.visible = true
		
	_ui_console.set_process_input(false)
		
	_close_time += delta

	var x = clampf(_close_time / close_duration, 0.0, 1.0)

	var y = _ui_console.size.y
	_ui_console.position.y = (y * x) * -1
	
	if x >= 1.0:
		_ui_console.position.y = y * -1
		_state = _next_state

func get_ui_node() -> Node:
	return _ui_console

func is_active() -> bool:
	return _ui_console != null && \
			_ui_console.visible == true && \
			_state == ConsoleAnimationState.Opened

func toggle() -> void:
	if _ui_console.visible == false:
		show()
	else:
		hide()

func show() -> void:
	match _state:
		ConsoleAnimationState.Closed:	
			_open_time = 0
			
			_state = ConsoleAnimationState.Opening
			_next_state = ConsoleAnimationState.Opened
		ConsoleAnimationState.Closing:
			_next_state = ConsoleAnimationState.Opening

func hide() -> void:
	match _state:
		ConsoleAnimationState.Opened:	
			_close_time = 0
			
			_state = ConsoleAnimationState.Closing
			_next_state = ConsoleAnimationState.Closed
		ConsoleAnimationState.Opening:
			_next_state = ConsoleAnimationState.Closing

func get_variables() -> Array:
	return _ui_console.GetVariables()

func get_variable(varName) -> ConsoleVariable:
	return _ui_console.GetVariable(varName)

func get_commands() -> Array:
	return _ui_console.GetCommands()

func get_command(cmdName) -> ConsoleCommand:
	return _ui_console.GetCommand(cmdName)

func get_command_or_variable(cmdName) -> ConsoleCommand:
	return _ui_console.GetCommandOrVariable(cmdName)

func info(text: String) -> void:
	_ui_console.Info(text)
	
func debug(text: String) -> void:
	_ui_console.Debug(text)

func error(text: String) -> void:
	_ui_console.Error(text)

func warning(text: String) -> void:
	_ui_console.Warning(text)

func execute(cmd: String) -> String:
	return _ui_console.Execute(cmd)

func execute_persist(cmd: String) -> String:
	return _ui_console.ExecutePersist(cmd)

func execute_inverted(cmd: String) -> String:
	return _ui_console.ExecuteInverted(cmd)
	
func execute_inverted_persist(cmd: String) -> String:
	return _ui_console.ExecuteInvertedPersist(cmd)

func execute_command(cmd: ConsoleCommand, arguments: Array) -> String:
	return _ui_console.ExecuteCommand(cmd, arguments)

func execute_command_persist(cmd: ConsoleCommand, arguments: Array) -> String:
	return _ui_console.ExecuteCommandPersist(cmd, arguments)

func execute_help(cmd: ConsoleCommand, arguments: Array) -> String:
	return _ui_console.ExecuteHelp(cmd, arguments)

func execute_help_persist(cmd: ConsoleCommand, arguments: Array) -> String:
	return _ui_console.ExecuteHelpPersist(cmd, arguments)

func execute_file(file: String) -> String:
	return _ui_console.ExecuteFile(file)
	
func execute_file_persist(file: String) -> String:
	return _ui_console.ExecuteFilePersist(file)

func execute_file_inverted(file: String) -> String:
	return _ui_console.ExecuteFileInverted(file)
	
func execute_file_inverted_persist(file: String) -> String:
	return _ui_console.ExecuteFileInvertedPersist(file)

func get_execution_context(guid: String) -> Dictionary:
	return _ui_console.GetExecutionContext(guid)

func clear_execution_context(guid: String) -> void:
	return _ui_console.ClearExecutionContext(guid)

func connect_variable(var_name, set_func: Callable) -> ConsoleVariable:
	var cvar: ConsoleVariable = get_variable(var_name)
	set_func.call(cvar, cvar.get_value())
	cvar.connect("value_changed", set_func)
	return cvar

func connect_command(cmd_name, exec_func: Callable) -> ConsoleCommand:
	var cmd: ConsoleCommand = get_command(cmd_name)
	cmd.connect("executed", exec_func)
	return cmd

func register_command(command: ConsoleCommand) -> void:
	command._guid = _ui_console.RegisterCommand(command)
	if OS.has_feature('debug'):
		print("Registered command %s with guid %s" % [command.get_command_name(), command._guid])

func register_variable(variable: ConsoleVariable) -> void:
	variable._guid = _ui_console.RegisterVariable(variable)
	if OS.has_feature('debug'):
		print("Registered variable %s with guid %s" % [variable.get_variable_name(), variable._guid])
