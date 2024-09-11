extends ConsoleVariable
class_name ConsoleVariableInternal

var _internal: Object = null

func _init(internal) -> void:
	_internal = internal
	
	_guid = internal.GetGuid()
	
	_internal.connect("Changed", \
		func(_sender: Object, _value: Variant):
			_on_value_changed(_value)
	)

	_internal.connect("Executed", \
		func(_sender: Object, __guid: String, delta: float, data: Dictionary, arguments: Array):
			_on_executed(delta, data, arguments)
	)

	_internal.connect("HelpExecuted", \
		func(_sender: Object, __guid: String, delta: float, data: Dictionary, arguments: Array):
			_on_help_executed(delta, data, arguments)
	)

func reset() -> void:
	return

func get_variable_name() -> String:
	return _internal.GetName()

func get_value() -> Variant:
	return _internal.GetValue()

func set_value(_value: Variant) -> void:
	_internal.SetValue(_value)

func execute(args: Array) -> void:
	_internal.Execute(args)

func execute_help(args: Array) -> void:
	_internal.ExecuteHelp(args)
