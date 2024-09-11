extends Control
class_name ConsolePrinterControl

@onready var _container: BoxContainer = $MarginContainer/console_printer
@onready var _line_item_template: ConsolePrinterItem = $MarginContainer/console_printer/console_print

func _ready() -> void:
	_line_item_template.visible = false

func print(text: String) -> void:
	if text == null or text.length() < 1:
		return

	var item: ConsolePrinterItem = _line_item_template.duplicate()

	item.expires = true
	item.text = text

	_container.add_child(item)
