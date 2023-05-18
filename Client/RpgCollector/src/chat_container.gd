extends TextureRect

@onready var content = $LineEdit


# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	pass


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
	http.request(Global.BASE_URL + "Chat", Global.headers, Global.POST, json)


func _on_send_chat_response(result, response_code, headers, body):
	if response_code != 200:
		print(body.get_string_from_utf8())
		return 
		
	var json = JSON.parse_string(body.get_string_from_utf8())
	if json.error == 0:
		print("OK")
		
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
