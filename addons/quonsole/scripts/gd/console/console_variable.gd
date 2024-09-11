extends ConsoleCommand
class_name ConsoleVariable

signal value_changed(variable: ConsoleVariable, value)

@export var default_value = ""
@export var help_text: String = ""

@onready var _value = default_value

var value:
	get:
		return get_value()
	set(new_value):
		set_value(new_value)

func reset():
	_value = default_value
	_on_value_changed(_value)

func get_variable_name() -> String:
	return name

func get_value():
	return _value

func set_value(new_value) -> void:
	_value = new_value
	_on_value_changed(new_value)

func as_bool() -> bool:
	var result: bool = false
	var v = get_value()
	
	if v != null && not v.is_empty():
		v = v.to_upper()
		result = \
			v == "TRUE" or \
			v == "T" or \
			v == "YES" or \
			v == "Y" or \
			v.to_int() != 0
	
	return result

func as_int() -> int:
	var v = get_value()
	return v.to_int() if v != null and not v.is_empty() else 0

func as_float() -> float:
	var v = get_value()
	return v.to_float() if v != null and not v.is_empty() else 0

func _on_value_changed(new_value) -> void:
	value_changed.emit(self, _value)
	
func _on_executed(delta: float, data: Dictionary, arguments: Array) -> bool:
	if arguments != null:
		if arguments.size() == 1:
			value = arguments[0]
		elif arguments.size() > 1:
			Console.error("Too many arguments specified.")
		else:
			Console.info("%s" % value)
	return super._on_executed(delta, data, arguments)

func _on_help_executed(delta: float, data: Dictionary, arguments: Array) -> bool:
	Console.info(help_text)
	return super._on_executed(delta, data, arguments)
