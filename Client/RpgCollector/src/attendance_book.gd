extends CanvasLayer

@onready var reward_container = $TextureRect/GridContainer

# Called when the node enters the scene tree for the first time.
func _ready():
	get_attendance_reward_request()


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	pass


func get_attendance_reward_request():
	for node in reward_container.get_children():
		node.queue_free()
		
	var json = JSON.stringify({
		"UserName" : Global.user_name,
		"AuthToken" : Global.auth_token, 
		"ClientVersion" : Global.client_version,
		"MasterVersion" : Global.master_version,
	})
	var http = HTTPRequest.new() 
	add_child(http)
	http.request_completed.connect(_on_attendance_reward_response)
	http.request(Global.BASE_URL + "Master/Attendance", Global.headers, Global.POST, json)
	
	
func _on_attendance_reward_response(result, response_code, headers, body):
	if response_code != 200:
		print(body.get_string_from_utf8())
		return 
		
	var json = JSON.parse_string(body.get_string_from_utf8())
	if json.error == 0:
		create_reward(json['attendanceRewards'])
		get_last_attendance_log_request()
		
	else:
		var msg = Global.ERROR_MSG[str(json['error'])]
		Global.open_alert(msg)
		
		
func create_reward(rewards):
	for reward in rewards:
		var back_texture = TextureRect.new() 
		var item_texture = TextureRect.new() 
		item_texture.name = 'item'
		#var ok_texture = TextureRect.new() #추후 출석유무에 따라 체크사진
		#ok_texture.name = "ok"
		var label = Label.new() 
		
		back_texture.texture = load("res://assets/inventory_slot.png")
		item_texture.texture = MasterData.item_texture[str(reward.itemId)]
		label.text = "연속 {day}일".format({
			"day" : reward.dayId
		})
		label.position = Vector2(-6, 66)
		label.add_theme_font_size_override("font_size", 16)
		label.add_theme_color_override("font_color", Color.BLACK)
		#item_texture.add_child(ok_texture)
		back_texture.add_child(item_texture)
		back_texture.add_child(label)
		reward_container.add_child(back_texture)


# 제일 마지막 연속출석을 가지고 옴 - 6이라면 6칸 체크, 
func get_last_attendance_log_request():
	var json = JSON.stringify({
		"UserName" : Global.user_name,
		"AuthToken" : Global.auth_token, 
		"ClientVersion" : Global.client_version,
		"MasterVersion" : Global.master_version,
	})
	var http = HTTPRequest.new() 
	add_child(http)
	http.request_completed.connect(_on_last_attendance_log_response)
	http.request(Global.BASE_URL + "Attendance/Log", Global.headers, Global.POST, json)
	

func _on_last_attendance_log_response(result, response_code, headers, body):
	if response_code != 200:
		print(body.get_string_from_utf8())
		return 
		
	var json = JSON.parse_string(body.get_string_from_utf8())
	if json.error == 0:
		do_check_daycount(json.sequenceDayCount)
		
	else:
		var msg = Global.ERROR_MSG[str(json['error'])]
		Global.open_alert(msg)


func do_check_daycount(count):
	var index = 0
	for node in reward_container.get_children():
		if index >= count:	
			return
		var item_texture = node.get_node("item")
		var ok = TextureRect.new() 
		ok.texture = load("res://assets/check.png")
		index+=1
		item_texture.add_child(ok)
		

func do_attendance_request():
	var json = JSON.stringify({
		"UserName" : Global.user_name,
		"AuthToken" : Global.auth_token, 
		"ClientVersion" : Global.client_version,
		"MasterVersion" : Global.master_version,
	})
	var http = HTTPRequest.new() 
	add_child(http)
	http.request_completed.connect(_on_do_attendance_response)
	http.request(Global.BASE_URL + "Attendance", Global.headers, Global.POST, json)
	
	
func _on_do_attendance_response(result, response_code, headers, body):
	if response_code != 200:
		print(body.get_string_from_utf8())
		return 
		
	var json = JSON.parse_string(body.get_string_from_utf8())
	if json.error == 0:
		get_attendance_reward_request()
		
	else:
		var msg = Global.ERROR_MSG[str(json['error'])]
		Global.open_alert(msg)


func _on_attendance_btn_pressed():
	do_attendance_request()


func _on_exit_btn_pressed():
	queue_free()
