extends Node
class_name InputBinder

enum BindState {
	Ready,
	Unpressed,
	Pressed
}

class BindModel:
	var location: int
	var on_press: String
	var on_down: String
	var on_up: String

class BindActionModel:
	var model: BindModel
	var execution_id: String
	var state: BindState
	var pressed: bool
	var invertible: bool

static var _key_map: Dictionary = {
	"ESCAPE": KEY_ESCAPE,
	"TAB": KEY_TAB,
	"BACKTAB": KEY_BACKTAB,
	"BACKSPACE": KEY_BACKSPACE,
	"ENTER": KEY_ENTER,
	"KP_ENTER": KEY_KP_ENTER,
	"INS": KEY_INSERT,
	"DELETE": KEY_DELETE,
	"PAUSE": KEY_PAUSE,
	"PRINT": KEY_PRINT,
	"SYSREQ": KEY_SYSREQ,
	"CLEAR": KEY_CLEAR,
	"HOME": KEY_HOME,
	"END": KEY_END,
	"LEFTARROW": KEY_LEFT,
	"UPARROW": KEY_UP,
	"RIGHTARROW": KEY_RIGHT,
	"DOWNARROW": KEY_DOWN,
	"PGUP": KEY_PAGEUP,
	"PGDN": KEY_PAGEDOWN,
	"SHIFT": { "code": KEY_SHIFT, "location": KEY_LOCATION_LEFT },
	"RSHIFT": { "code": KEY_SHIFT, "location": KEY_LOCATION_RIGHT },
	"CTRL": { "code": KEY_CTRL, "location": KEY_LOCATION_LEFT },
	"RCTRL": { "code": KEY_CTRL, "location": KEY_LOCATION_RIGHT },
	"LWIN": { "code": KEY_META, "location": KEY_LOCATION_LEFT },
	"RWIN": { "code": KEY_META, "location": KEY_LOCATION_RIGHT },
	"ALT": { "code": KEY_ALT, "location": KEY_LOCATION_LEFT },
	"RALT": { "code": KEY_ALT, "location": KEY_LOCATION_RIGHT },
	"CAPSLOCK": KEY_CAPSLOCK,
	"NUMLOCK": KEY_NUMLOCK,
	"SCROLLLOCK": KEY_SCROLLLOCK,
	"F1": KEY_F1,
	"F2": KEY_F2,
	"F3": KEY_F3,
	"F4": KEY_F4,
	"F5": KEY_F5,
	"F6": KEY_F6,
	"F7": KEY_F7,
	"F8": KEY_F8,
	"F9": KEY_F9,
	"F10": KEY_F10,
	"F11": KEY_F11,
	"F12": KEY_F12,
	"F13": KEY_F13,
	"F14": KEY_F14,
	"F15": KEY_F15,
	"F16": KEY_F16,
	"F17": KEY_F17,
	"F18": KEY_F18,
	"F19": KEY_F19,
	"F20": KEY_F20,
	"F21": KEY_F21,
	"F22": KEY_F22,
	"F23": KEY_F23,
	"F24": KEY_F24,
	"F25": KEY_F25,
	"F26": KEY_F26,
	"F27": KEY_F27,
	"F28": KEY_F28,
	"F29": KEY_F29,
	"F30": KEY_F30,
	"F31": KEY_F31,
	"F32": KEY_F32,
	"F33": KEY_F33,
	"F34": KEY_F34,
	"F35": KEY_F35,
	"KP_MULTIPLY": KEY_KP_MULTIPLY,
	"KP_SLASH": KEY_KP_DIVIDE,
	"KP_MINUS": KEY_KP_SUBTRACT,
	"KP_DEL": KEY_KP_PERIOD,
	"KP_PLUS": KEY_KP_ADD,
	"KP_INS": KEY_KP_0,
	"KP_END": KEY_KP_1,
	"KP_DOWNARROW": KEY_KP_2,
	"KP_PGDN": KEY_KP_3,
	"KP_LEFTARROW": KEY_KP_4,
	"KP_5": KEY_KP_5,
	"KP_RIGHTARROW": KEY_KP_6,
	"KP_HOME": KEY_KP_7,
	"KP_UPARROW": KEY_KP_8,
	"KP_PGUP": KEY_KP_9,
	"MENU": KEY_MENU,
	"HYPER": KEY_HYPER,
	"HELP": KEY_HELP,
	"BACK": KEY_BACK,
	"FORWARD": KEY_FORWARD,
	"STOP": KEY_STOP,
	"REFRESH": KEY_REFRESH,
	"VOLUMEDOWN": KEY_VOLUMEDOWN,
	"VOLUMEMUTE": KEY_VOLUMEMUTE,
	"VOLUMEUP": KEY_VOLUMEUP,
	"MEDIAPLAY": KEY_MEDIAPLAY,
	"MEDIASTOP": KEY_MEDIASTOP,
	"MEDIAPREVIOUS": KEY_MEDIAPREVIOUS,
	"MEDIANEXT": KEY_MEDIANEXT,
	"MEDIARECORD": KEY_MEDIARECORD,
	"HOMEPAGE": KEY_HOMEPAGE,
	"FAVORITES": KEY_FAVORITES,
	"SEARCH": KEY_SEARCH,
	"STANDBY": KEY_STANDBY,
	"OPENURL": KEY_OPENURL,
	"LAUNCHMAIL": KEY_LAUNCHMAIL,
	"LAUNCHMEDIA": KEY_LAUNCHMEDIA,
	"LAUNCH0": KEY_LAUNCH0,
	"LAUNCH1": KEY_LAUNCH1,
	"LAUNCH2": KEY_LAUNCH2,
	"LAUNCH3": KEY_LAUNCH3,
	"LAUNCH4": KEY_LAUNCH4,
	"LAUNCH5": KEY_LAUNCH5,
	"LAUNCH6": KEY_LAUNCH6,
	"LAUNCH7": KEY_LAUNCH7,
	"LAUNCH8": KEY_LAUNCH8,
	"LAUNCH9": KEY_LAUNCH9,
	"LAUNCHA": KEY_LAUNCHA,
	"LAUNCHB": KEY_LAUNCHB,
	"LAUNCHC": KEY_LAUNCHC,
	"LAUNCHD": KEY_LAUNCHD,
	"LAUNCHE": KEY_LAUNCHE,
	"LAUNCHF": KEY_LAUNCHF,
	"GLOBE": KEY_GLOBE,
	"KEYBOARD": KEY_KEYBOARD,
	"JIS_EISU": KEY_JIS_EISU,
	"JIS_KANA": KEY_JIS_KANA,
	"UNKNOWN": KEY_UNKNOWN,
	"SPACE": KEY_SPACE,
	"!": KEY_EXCLAM,
	"\"": KEY_QUOTEDBL,
	"#": KEY_NUMBERSIGN,
	"$": KEY_DOLLAR,
	"%": KEY_PERCENT,
	"&": KEY_AMPERSAND,
	"'": KEY_APOSTROPHE,
	"(": KEY_PARENLEFT,
	")": KEY_PARENRIGHT,
	"*": KEY_ASTERISK,
	"+": KEY_PLUS,
	",": KEY_COMMA,
	"-": KEY_MINUS,
	".": KEY_PERIOD,
	"/": KEY_SLASH,
	"0": KEY_0,
	"1": KEY_1,
	"2": KEY_2,
	"3": KEY_3,
	"4": KEY_4,
	"5": KEY_5,
	"6": KEY_6,
	"7": KEY_7,
	"8": KEY_8,
	"9": KEY_9,
	":": KEY_COLON,
	";": KEY_SEMICOLON,
	"<": KEY_LESS,
	"=": KEY_EQUAL,
	">": KEY_GREATER,
	"?": KEY_QUESTION,
	"@": KEY_AT,
	"A": KEY_A,
	"B": KEY_B,
	"C": KEY_C,
	"D": KEY_D,
	"E": KEY_E,
	"F": KEY_F,
	"G": KEY_G,
	"H": KEY_H,
	"I": KEY_I,
	"J": KEY_J,
	"K": KEY_K,
	"L": KEY_L,
	"M": KEY_M,
	"N": KEY_N,
	"O": KEY_O,
	"P": KEY_P,
	"Q": KEY_Q,
	"R": KEY_R,
	"S": KEY_S,
	"T": KEY_T,
	"U": KEY_U,
	"V": KEY_V,
	"W": KEY_W,
	"X": KEY_X,
	"Y": KEY_Y,
	"Z": KEY_Z,
	"[": KEY_BRACKETLEFT,
	"\\": KEY_BACKSLASH,
	"]": KEY_BRACKETRIGHT,
	"^": KEY_ASCIICIRCUM,
	"_": KEY_UNDERSCORE,
	"`": KEY_QUOTELEFT,
	"{": KEY_BRACELEFT,
	"BAR": KEY_BAR,
	"}": KEY_BRACERIGHT,
	"~": KEY_ASCIITILDE,
	"¥": KEY_YEN,
	"§": KEY_SECTION
}

static var _mouse_map: Dictionary = {
	"MOUSE1": MOUSE_BUTTON_LEFT,
	"MOUSE2": MOUSE_BUTTON_RIGHT,
	"MOUSE3": MOUSE_BUTTON_MIDDLE,
	"MWHEELDOWN": MOUSE_BUTTON_WHEEL_DOWN,
	"MWHEELUP": MOUSE_BUTTON_WHEEL_UP,
	"MOUSE4": MOUSE_BUTTON_XBUTTON1,
	"MOUSE5": MOUSE_BUTTON_XBUTTON2
}

static var _joy_map: Dictionary = {
	"JOY1": JOY_BUTTON_A,
	"JOY2": JOY_BUTTON_B,
	"JOY3": JOY_BUTTON_X,
	"JOY4": JOY_BUTTON_Y,
	"JOY7": JOY_BUTTON_BACK,
	"JOY13": JOY_BUTTON_GUIDE,
	"JOY8": JOY_BUTTON_START,
	"JOY9": JOY_BUTTON_LEFT_STICK,
	"JOY10": JOY_BUTTON_RIGHT_STICK,
	"JOY5": JOY_BUTTON_LEFT_SHOULDER,
	"JOY6": JOY_BUTTON_RIGHT_SHOULDER,
	"POV_UP": JOY_BUTTON_DPAD_UP,
	"POV_DOWN": JOY_BUTTON_DPAD_DOWN,
	"POV_LEFT": JOY_BUTTON_DPAD_LEFT,
	"POV_RIGHT": JOY_BUTTON_DPAD_RIGHT,
	"JOY15": JOY_BUTTON_MISC1,
	"JOY16": JOY_BUTTON_PADDLE1,
	"JOY17": JOY_BUTTON_PADDLE2,
	"JOY18": JOY_BUTTON_PADDLE3,
	"JOY19": JOY_BUTTON_PADDLE4,
	"JOY14": JOY_BUTTON_TOUCHPAD
}

var _key_binds: Dictionary = {}
var _key_bind_action: Dictionary = {}

var _mouse_binds: Dictionary = {}
var _mouse_bind_action: Dictionary = {}

var _joy_binds: Dictionary = {}
var _joy_bind_action: Dictionary = {}

func get_bind(key: String) -> String:
	var bind: BindModel = handle_get_bind(_key_map, _key_binds, key)
	
	if bind == null:
		bind = handle_get_bind(_mouse_map, _mouse_binds, key)
	
	if bind == null:
		bind = handle_get_bind(_joy_map, _joy_binds, key)
	
	if bind != null:
		var result: String = ""
		
		var k: String = sanitize_key(key)
		
		if bind.on_down != "":
			result += "%s+%s: %s" % [
				"\n" if result.length() > 0 else "",
				k, bind.on_down]
		
		if bind.on_up != "":
			result += "%s-%s: %s" % [
				"\n" if result.length() > 0 else "",
				k, bind.on_up]
		
		if bind.on_press != "":
			result += "%s%s: %s" % [
				"\n" if result.length() > 0 else "",
				k, bind.on_press]
			
		return result
	else:
		return ""

func bind_key(key: String, command: String) -> bool:
	return handle_bind(_key_map, _key_binds, key, command)

func bind_mouse(button: String, command: String) -> bool:
	return handle_bind(_mouse_map, _mouse_binds, button, command)

func bind_joy(button: String, command: String) -> bool:
	return handle_bind(_joy_map, _joy_binds, button, command)

func sanitize_key(key: String) -> String:
	var k: String = handle_sanitize_key(_key_map, key)
	
	if k == "":
		k = handle_sanitize_key(_mouse_map, key)
	
	if k == "":
		k = handle_sanitize_key(_joy_map, key)
	
	return k

func handle_sanitize_key(map: Dictionary, key: String) -> String:
	var kup = key.to_upper()
	
	if kup == "":
		return ""
	
	var isForDown: bool = kup.begins_with("+")
	var isForUp: bool = kup.begins_with("-")
	
	if isForUp or isForDown:
		kup = kup.right(-1)

	if not map.has(kup):
		return ""
	
	return kup

func handle_get_bind(map: Dictionary, binds: Dictionary, key: String) -> BindModel:
	var kup = sanitize_key(key)
	
	if not map.has(kup):
		return null
	
	var mval = map[kup]
	var midx: String = "%s[%s]" % [mval.code, mval.location] if mval is Dictionary else "%s" % [mval]
	
	var cmdUp: String = ""
	var cmdDown: String = ""
	
	if binds.has(midx):
		return binds[midx]

	return null

func handle_bind(map: Dictionary, binds: Dictionary, key: String, command: String) -> bool:
	var kup = key.to_upper()
	
	if kup == "":
		return false
	
	var isForDown: bool = kup.begins_with("+")
	var isForUp: bool = kup.begins_with("-")
	
	if isForUp or isForDown:
		kup = kup.right(-1)

	if not map.has(kup):
		return false
	
	var mval = map[kup]
	var midx: String = "%s[%s]" % [mval.code, mval.location] if mval is Dictionary else "%s" % [mval]
	
	var cmdUp: String = ""
	var cmdDown: String = ""
	
	if binds.has(midx):
		cmdUp = binds[midx].on_up
		cmdDown = binds[midx].on_down
		binds.erase(midx)
	
	var state: BindModel = BindModel.new()
	
	state.location = 0
	state.on_press = command if not isForUp and not isForDown else ""
	state.on_up = command if isForUp else cmdUp
	state.on_down = command if isForDown else cmdDown
	
	binds[midx] = state
	
	return true
	
func bind(key: String, command: String):
	var kup = key.to_upper()
	if not bind_key(key, command) and \
		not bind_mouse(key, command) and \
		not bind_joy(key, command):
		Console.error("Key or button '%s' does not exist." % [kup])

func handle_unbind(map: Dictionary, binds: Dictionary, key: String) -> bool:
	var kup = sanitize_key(key)
	
	if not map.has(kup):
		return false
	
	var kidx = map[kup]
	
	if binds.has(kidx):
		binds.erase(kidx)
	
	return true

func unbind_key(key: String) -> bool:
	return handle_unbind(_key_map, _key_binds, key)
	
func unbind_mouse(button: String) -> bool:
	return handle_unbind(_mouse_map, _mouse_binds, button)

func unbind_joy(button: String) -> bool:
	return handle_unbind(_joy_map, _joy_binds, button)

func unbind(key: String) -> void:
	var kup = key.to_upper()
	if not unbind_key(key) and not unbind_mouse(key):
		Console.error("Input '%s' does not exist." % [kup])

func process_bind_actions(bind_actions: Dictionary) -> void:
	for key in bind_actions.keys():
		var action: BindActionModel = bind_actions[key]
		
		if action.execution_id != "" and action.execution_id != null:
			var context = Console.get_execution_context(action.execution_id)

			if context["is_finished"]:
				Console.clear_execution_context(action.execution_id)
				action.execution_id = ""
				action.invertible = context["is_invertible"]
		elif action.pressed and action.state == BindState.Unpressed:
			if action.model.on_down != "":
				action.execution_id = Console.execute_persist(action.model.on_down)
			else:
				action.execution_id = Console.execute_persist(action.model.on_press)
			action.state = BindState.Pressed
		elif not action.pressed and action.state == BindState.Pressed:
			if action.model.on_up != "":
				action.execution_id = Console.execute_persist(action.model.on_up)
			else:
				if action.invertible:
					action.execution_id = Console.execute_inverted_persist(action.model.on_press)
			action.state = BindState.Unpressed
		else:
			action.state = BindState.Pressed if action.pressed else BindState.Unpressed
		
		bind_actions[key] = action

func _process(delta: float) -> void:
	process_bind_actions(_key_bind_action)
	process_bind_actions(_mouse_bind_action)
	process_bind_actions(_joy_bind_action)

func handle_key_input(key: InputEventKey) -> void:
	var key_name = "%s" % [key.keycode]
	
	if key.location == KEY_LOCATION_LEFT or key.location == KEY_LOCATION_RIGHT:
		key_name = "%s[%s]" % [key.keycode, key.location]
	
	if _key_binds.has(key_name):
		var state: BindModel = _key_binds[key_name]
		var action: BindActionModel
		
		if _key_bind_action.has(key_name):
			action = _key_bind_action[key_name]
			action.model = state
			action.pressed = key.pressed
		else:
			action = BindActionModel.new()
			action.execution_id = ""
			action.model = state
			action.pressed = key.pressed
			action.state = BindState.Unpressed if key.pressed else BindState.Ready
			
		_key_bind_action[key_name] = action

func handle_mouse_input(mouse: InputEventMouseButton) -> void:
	var key_name = "%s" % [mouse.button_index]

	if _mouse_binds.has(key_name):
		var state: BindModel = _mouse_binds[key_name]

		var action: BindActionModel
		
		if _mouse_bind_action.has(key_name):
			action = _mouse_bind_action[key_name]
			action.model = state
			action.pressed = mouse.pressed
		else:
			action = BindActionModel.new()
			action.execution_id = ""
			action.model = state
			action.pressed = mouse.pressed
			action.state = BindState.Unpressed if mouse.pressed else BindState.Ready

		_mouse_binds[key_name] = state
		_mouse_bind_action[key_name] = action

func handle_joy_input(joy: InputEventJoypadButton) -> void:
	var key_name = "%s" % [joy.button_index]

	if _joy_binds.has(key_name):
		var state: BindModel = _joy_binds[key_name]

		var action: BindActionModel
		
		if _joy_bind_action.has(key_name):
			action = _joy_bind_action[key_name]
			action.model = state
			action.pressed = joy.pressed
		else:
			action = BindActionModel.new()
			action.execution_id = ""
			action.model = state
			action.pressed = joy.pressed
			action.state = BindState.Unpressed if joy.pressed else BindState.Ready

		_joy_binds[key_name] = state
		_joy_bind_action[key_name] = action

func _unhandled_key_input(event: InputEvent) -> void:
	if event is InputEventKey:
		handle_key_input(event)

func _unhandled_input(event: InputEvent) -> void:
	if event is InputEventMouseButton:
		handle_mouse_input(event)
	elif event is InputEventJoypadButton:
		handle_joy_input(event)
