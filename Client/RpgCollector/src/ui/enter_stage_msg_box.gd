extends CanvasLayer

@onready var stage_name = $TextureRect/Label2

signal enter_stage

# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	pass


func set_stage_name(_name:String):
	stage_name.text = _name


func _on_button_pressed():
	emit_signal("enter_stage")
	queue_free()


func _on_texture_button_pressed():
	queue_free()
