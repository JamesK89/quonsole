extends RichTextLabel
class_name ConsolePrinterItem

@export var expires: bool = false
@export var lifetime: float = 8.0
@export var fade: bool = true
@export var fade_time: float = 2.0
@export var fade_curve: Curve = null

@onready var _time: float = lifetime

func _ready() -> void:
	visible = true

func _process(delta: float) -> void:
	if expires:
		_time -= delta
		
		if _time <= 0.0:
			if not fade or absf(_time) >= fade_time:
				visible = false
				queue_free()
			else:
				var progress = clampf(absf(_time) / fade_time, 0, 1)
				modulate.a = fade_curve.sample_baked(progress) \
					if not fade_curve == null else 1.0 - progress
