extends TextureRect

@onready var content = $LineEdit
@onready var chat_container = $ScrollContainer/VBoxContainer

var last_timestamp = 0
var index = 0

# Called when the node enters the scene tree for the first time.
func _ready():
	for node in chat_container.get_children():
		node.queue_free()
		
	_on_test_join_pressed()


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	if Input.is_action_just_pressed("enter"):
		_on_button_pressed()


func _on_button_pressed():
	if content.text == "":
		return 
		
	var json = JSON.stringify({
		"UserName" : Global.user_name,
		"AuthToken" : Global.auth_token, 
		"ClientVersion" : Global.client_version,
		"MasterVersion" : Global.master_version,
		"Content" : content.text
	})
	content.text = ""

	var http = HTTPRequest.new() 
	add_child(http)
	http.request_completed.connect(_on_send_chat_response)
	http.request(Global.BASE_URL + "Chat/Send", Global.headers, Global.POST, json)


func _on_send_chat_response(result, response_code, headers, body):
	if response_code != 200:
		print(body.get_string_from_utf8())
		return 
		
	var json = JSON.parse_string(body.get_string_from_utf8())
	if json.error == 0:
		pass
		
	else:
		var msg = Global.ERROR_MSG[str(json['error'])]
		Global.open_alert(msg)
			

func _on_test_join_pressed():
	var json = JSON.stringify({
		"UserName" : Global.user_name,
		"AuthToken" : Global.auth_token, 
		"ClientVersion" : Global.client_version,
		"MasterVersion" : Global.master_version,
	})

	var http = HTTPRequest.new() 
	add_child(http)
	http.request_completed.connect(_on_join_chat_response)
	http.request(Global.BASE_URL + "Chat/Join", Global.headers, Global.POST, json)


func _on_join_chat_response(result, response_code, headers, body):
	if response_code != 200:
		print(body.get_string_from_utf8())
		return 
		
	var json = JSON.parse_string(body.get_string_from_utf8())
	if json.error == 0:
		print("OK")
		
	else:
		var msg = Global.ERROR_MSG[str(json['error'])]
		Global.open_alert(msg)

		
func load_chat(json):
	if json.error != 0:
		var msg = Global.ERROR_MSG[str(json['error'])]
		Global.open_alert(msg)
		return
	
	last_timestamp = json.timeStamp
	
	for chat in json.chatLog:
		var label = Label.new() 
		label.add_theme_font_size_override("font_size", 16)
		label.add_theme_color_override("font_color", Color.BLACK)
		label.text = "[{name}] {content}".format({
			"name" : chat.userName, 
			"content" : chat.content
		})
		chat_container.add_child(label)


func _on_timer_timeout():
	var chat_node = load("res://src/chat_load_node.tscn").instantiate()
	chat_node.chat_load_response.connect(load_chat)
	add_child(chat_node)
	chat_node._on_load_chat_request(last_timestamp)




