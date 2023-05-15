extends CanvasLayer


signal continue_stage 
signal exit_stage 

# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	pass


func _on_button_pressed():
	emit_signal("continue_stage")
	queue_free()


func _on_button_2_pressed():
	emit_signal("exit_stage")
	queue_free()
