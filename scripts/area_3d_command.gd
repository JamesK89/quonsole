extends Area3D
class_name Area3DCommand

@export_group("Console Commands")
@export var on_entered: String = ""
@export var on_exited: String = ""

@onready var sprite: Sprite3D = $Sprite3D

@onready var spring = SimpleSpring.new()

func _ready():
	spring.damp = 5

	body_shape_entered.connect(_on_entered)
	body_shape_exited.connect(_on_exited)

func _process(delta: float) -> void:
	spring.update(delta)

	var fac: float = 1.0 + spring.value
	sprite.scale = Vector3(fac, fac, fac)

func _on_entered(_body_rid: RID, _body: Node3D, _body_shape_index: int, _local_shape_index: int):
	spring.displacement += 0.25
	if on_entered != null and !on_entered.is_empty():
		Console.execute(on_entered)

func _on_exited(_body_rid: RID, _body: Node3D, _body_shape_index: int, _local_shape_index: int):
	spring.displacement += 0.25
	if on_exited != null and !on_exited.is_empty():
		Console.execute(on_exited)
	
