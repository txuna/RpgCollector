extends Control

@onready var username = $ColorRect/FormControl/UsernameEdit
@onready var password = $ColorRect/FormControl/PasswordEdit

# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	pass


func _on_login_btn_pressed():
	if not verify():
		return 
		
	var json = JSON.stringify({
		"UserName" : username.text, 
		"Password" : password.text, 
		"AuthToken" : Global.auth_token, 
		"ClientVersion" : Global.client_version,
		"MasterVersion" : Global.master_version
	})
	var http = HTTPRequest.new()
	add_child(http)
	http.request_completed.connect(_on_http_login_response)
	http.request(Global.BASE_URL + "Login", Global.headers, Global.POST, json)


func _on_register_btn_pressed():
	if not verify():
		return 
		
	var json = JSON.stringify({
		"UserName" : username.text, 
		"Password" : password.text,
		"AuthToken" : Global.auth_token, 
		"ClientVersion" : Global.client_version,
		"MasterVersion" : Global.master_version
	})
	var http = HTTPRequest.new() 
	add_child(http)
	http.request_completed.connect(_on_http_register_response)
	http.request(Global.BASE_URL + "Register", Global.headers, Global.POST, json)


func verify():
	if username.text == "" or password.text == "":
		return false
	else:
		return true

func _on_http_register_response(result, response_code, headers, body):
	if response_code != 200: 
		return 
		
	var json = JSON.parse_string(body.get_string_from_utf8())
	if json.error == 0:
		Global.open_alert("회원가입이 정상적으로 이루어졌습니다")
	
	else:
		var msg = Global.ERROR_MSG[str(json['error'])]
		Global.open_alert(msg)
	

# 이어하기 옵션이 있다면 flag 값을 주어 main화면 이동시 던전 오픈 함
func _on_http_login_response(result, response_code, headers, body):
	if response_code != 200:
		return 

	var json = JSON.parse_string(body.get_string_from_utf8())	
	if json.error == 0:
		Global.user_name = json.userName
		Global.auth_token = json.authToken
		
		Global.login_state = json.state
		get_tree().change_scene_to_file("res://src/main.tscn")
	
	else:
		Global.open_alert(Global.ERROR_MSG[str(json['error'])])





