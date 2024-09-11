extends Object
class_name SimpleSpring

@export var spring: float = 150.0
@export var damp: float = 10.0
@export var multiplier: float = 1.0
@export var displacement_limit: float = 100.0
@export var limit_displacement: bool = false

var value: float = 0.0

var displacement: float = 0.0
var velocity: float = 0.0

func update(delta: float) -> void:
	var force = -spring * displacement - damp * velocity
	velocity += force * delta
	displacement += velocity * delta
	
	if limit_displacement:
		displacement = clampf(displacement, -displacement_limit, displacement_limit)

	value = displacement * multiplier
