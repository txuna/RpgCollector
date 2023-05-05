extends CanvasLayer

@onready var notice_container = $TextureRect/ScrollContainer/VBoxContainer

# Called when the node enters the scene tree for the first time.
func _ready():
	get_notice_request()


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	pass


func get_notice_request():
	var json = JSON.stringify({
	})
	var http = HTTPRequest.new() 
	add_child(http)
	http.request_completed.connect(_on_get_notice_response)
	http.request(Global.BASE_URL + "Notice", Global.headers, Global.POST, json)
	
	
func _on_get_notice_response(result, response_code, headers, body):
	if response_code != 200:
		print(body.get_string_from_utf8())
		return 
		
	var json = JSON.parse_string(body.get_string_from_utf8())
	if json.error == 0:
		create_notice(json.noticeList)
		
	else:
		var msg = Global.ERROR_MSG[str(json['error'])]
		Global.open_alert(msg)
		

func create_notice(notices):
	for node in notice_container.get_children():
		node.queue_free()
		
	for notice in notices:
		var color_rect = ColorRect.new() 
		var title = Label.new() 
		var content = Label.new()

		title.text = notice.title 
		content.text = notice.content 
		color_rect.color = Color.DARK_GRAY
		title.add_theme_color_override("font_color", Color.BLACK)
		content.add_theme_color_override("font_color", Color.BLACK)
		title.add_theme_font_size_override("font_size", 24)
		content.add_theme_font_size_override("font_size", 16)

		color_rect.custom_minimum_size = Vector2(750, 100)
		title.custom_minimum_size = Vector2(700, 0) 
		content.custom_minimum_size = Vector2(700, 0)
		
		title.position = Vector2(5, 0)
		content.position = Vector2(5, 45)
		
		color_rect.add_child(title)
		color_rect.add_child(content)
		notice_container.add_child(color_rect)


func _on_exit_btn_pressed():
	queue_free()
