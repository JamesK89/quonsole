extends ConsoleCommand

func _on_executed(_delta: float, _data: Dictionary, _args: Array) -> bool:
	if _args.size() < 1:
		Console.error("Input not specified for unbind.")
		return true

	Bindings.unbind(_args[0])
	
	super._on_executed(_delta, _data, _args)

	return true

func _on_help_executed(_delta: float, _data: Dictionary, _args: Array) -> bool:
	Console.info("Removes any binding of a command to an input. Usage:")
	Console.info("unbind \"key\"")
	super._on_help_executed(_delta, _data, _args)
	return true
