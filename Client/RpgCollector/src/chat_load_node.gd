extends HTTPRequest


signal chat_load_response(json)

# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	pass


func _on_load_chat_request(last_timestamp):
	var json = JSON.stringify({
		"UserName" : Global.user_name,
		"AuthToken" : Global.auth_token, 
		"ClientVersion" : Global.client_version,
		"MasterVersion" : Global.master_version,
		"TimeStamp" : last_timestamp,
	})

	var http = HTTPRequest.new() 
	add_child(http)
	http.request_completed.connect(_on_load_chat_response)
	http.request(Global.BASE_URL + "Chat/Load", Global.headers, Global.POST, json)


func _on_load_chat_response(result, response_code, headers, body):
	if response_code != 200:
		print(body.get_string_from_utf8())
		return 
		
	var json = JSON.parse_string(body.get_string_from_utf8())
	emit_signal("chat_load_response", json)
	queue_free()
