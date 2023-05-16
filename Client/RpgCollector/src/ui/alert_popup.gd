extends CanvasLayer

@onready var title = $TextureRect/Title
@onready var content = $TextureRect/Content
@onready var ok_btn = $TextureRect/OkBtn

# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	pass


func setup_alert(_title:String, _content:String):
	title.text = _title
	content.text = _content


func _on_ok_btn_pressed():
	queue_free()
