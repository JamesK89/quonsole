@tool
extends EditorPlugin

const AUTOLOAD_CONSOLE_NAME = "Console"
const AUTOLOAD_BINDER_NAME = "Bindings"

func _enable_plugin():
	add_autoload_singleton(AUTOLOAD_CONSOLE_NAME, "res://addons/quonsole/scripts/gd/console/console_interface.gd")
	add_autoload_singleton(AUTOLOAD_BINDER_NAME, "res://addons/quonsole/scripts/gd/binder/input_binder.gd")

func _disable_plugin():
	remove_autoload_singleton(AUTOLOAD_CONSOLE_NAME)
	remove_autoload_singleton(AUTOLOAD_BINDER_NAME)
