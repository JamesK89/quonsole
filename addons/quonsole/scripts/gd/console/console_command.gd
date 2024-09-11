extends Node
class_name ConsoleCommand

signal executed(command: ConsoleCommand, guid: String, delta: float, data: Dictionary, arguments: Array)
signal help_executed(command: ConsoleCommand, guid: String, delta: float, data: Dictionary, arguments: Array)

var _console: ConsoleInterface = null
var _guid: String = ""

func get_command_name() -> String:
	return name

func execute(arguments: Array) -> void:
	_console.execute_command(self, arguments)

func execute_help(arguments: Array) -> void:
	_console.execute_help(self, arguments)

func _on_executed(delta: float, data: Dictionary, arguments: Array) -> bool:
	executed.emit(self, _guid, delta, data, arguments)
	return true

func _on_help_executed(delta: float, data: Dictionary, arguments: Array) -> bool:
	help_executed.emit(self, _guid, delta, data, arguments)
	return true
