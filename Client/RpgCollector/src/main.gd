extends Node2D


# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	pass


func _on_mailbox_btn_pressed():
	if get_node_or_null('mailbox') != null:
		return 
		
	var mailbox_instance = load("res://src/ui/mailbox.tscn").instantiate() 
	mailbox_instance.name = "mailbox"
	add_child(mailbox_instance)
