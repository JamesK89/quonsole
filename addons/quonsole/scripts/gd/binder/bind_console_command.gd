extends ConsoleCommand

func _on_executed(_delta: float, _data: Dictionary, _args: Array) -> bool:
	if _args.size() < 1:
		Console.error("Input and command not specified for bind.")
		return true
	
	if _args.size() < 2:
		var bind = Bindings.get_bind(_args[0])
		if bind != "" and bind != null:
			Console.info(bind)
		else:
			Console.info("Not bound")
		return true
	else:
		Bindings.bind(_args[0], _args[1])

	super._on_executed(_delta, _data, _args)

	return true

func _on_help_executed(_delta: float, _data: Dictionary, _args: Array) -> bool:
	Console.info("Binds a command to an input. Usage:")
	Console.info("bind \"key\" \"command\"")
	Console.info("Omit \"command\" to see what is currently bound to that input.")
	super._on_help_executed(_delta, _data, _args)
	return true
