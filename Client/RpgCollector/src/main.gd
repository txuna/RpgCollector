extends Node2D


# Called when the node enters the scene tree for the first time.
func _ready():
	if Global.login_state == Global.PLAYING:
		continue_stage_request()
		

func continue_stage():
	pass
	
	
func continue_stage_request():
	var json = JSON.stringify({
		"UserName" : Global.user_name,
		"AuthToken" : Global.auth_token, 
		"ClientVersion" : Global.client_version,
		"MasterVersion" : Global.master_version,
	})
	var http = HTTPRequest.new() 
	add_child(http)
	http.request_completed.connect(_on_continue_stage_response)
	http.request(Global.BASE_URL + "Stage/Continue", Global.headers, Global.POST, json)
	
	
func _on_continue_stage_response(result, response_code, headers, body):
	if response_code != 200:
		print(body.get_string_from_utf8())
		return 
		
	var json = JSON.parse_string(body.get_string_from_utf8())
	if json.error == 0:
		print(json)
		var instance = load("res://src/stage.tscn").instantiate()
		get_parent().add_child(instance)
		instance.init_continue_setup(json)

	else:
		var msg = Global.ERROR_MSG[str(json['error'])]
		Global.open_alert(msg)	


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	pass


func _on_mailbox_btn_pressed():
	if get_node_or_null('mailbox') != null:
		return 
		
	var mailbox_instance = load("res://src/ui/mailbox.tscn").instantiate() 
	mailbox_instance.name = "mailbox"
	add_child(mailbox_instance)


func _on_attendance_btn_pressed():
	if get_node_or_null("attendance") != null:
		return 
		
	var attendance_book_instance = load("res://src/attendance_book.tscn").instantiate()
	attendance_book_instance.name = "attendance"
	add_child(attendance_book_instance)


func _on_notice_btn_pressed():
	if get_node_or_null("notice") != null:
		return 
		
	var notice_instance = load("res://src/notice.tscn").instantiate()
	notice_instance.name = "notice"
	add_child(notice_instance)


func _on_inventory_btn_pressed():
	if get_node_or_null("inventory") != null:
		return 
		
	var inventory_instance = load("res://src/ui/inventory.tscn").instantiate()
	inventory_instance.name = 'inventory'
	add_child(inventory_instance)


func _on_enchant_btn_pressed():
	if get_node_or_null("enchant") != null:
		return 
		
	var enchant_instance = load("res://src/enchant.tscn").instantiate()
	enchant_instance.name = 'enchant'
	add_child(enchant_instance)


func _on_payment_btn_pressed():
	if get_node_or_null("payment") != null:
		return 
		
	var payment_instance = load("res://src/ui/package.tscn").instantiate() 
	payment_instance.name = "payment"
	add_child(payment_instance)


func _on_stage_btn_pressed():
	if get_node_or_null("world_map") != null:
		return 
		
	var world_map_instance = load("res://src/world_map.tscn").instantiate()
	world_map_instance.name = "world_map"
	add_child(world_map_instance)
