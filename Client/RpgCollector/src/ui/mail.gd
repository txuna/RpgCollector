extends ColorRect

@onready var title = $HBoxContainer/MarginContainer2/Label
@onready var btn = $HBoxContainer/MarginContainer/Button

signal read_mail(mail_id)

var mail_id = 0

# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.


func setup(_title:String, _mail_id:int):
	title.text = _title
	mail_id = _mail_id

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	pass


func _on_button_pressed():
	read_mail.emit(mail_id)
