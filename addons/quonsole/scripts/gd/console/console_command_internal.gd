extends ConsoleCommand
class_name ConsoleCommandInternal

var _internal: Object = null

func _init(internal) -> void:
	_internal = internal
	
	_guid = internal.GetGuid()
	
	_internal.connect("Executed", \
		func(_sender: Object, __guid: String, delta: float, data: Dictionary, arguments: Array):
			_on_executed(delta, data, arguments)
	)

	_internal.connect("HelpExecuted", \
		func(_sender: Object, __guid: String, delta: float, data: Dictionary, arguments: Array):
			_on_help_executed(delta, data, arguments)
	)

func get_command_name() -> String:
	return _internal.GetName()

func execute(arguments: Array) -> void:
	_internal.Execute(arguments)

func execute_help(arguments: Array) -> void:
	_internal.ExecuteHelp(arguments)
