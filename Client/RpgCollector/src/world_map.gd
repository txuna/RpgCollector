extends CanvasLayer

@onready var map_list = $TextureRect/Control
# Called when the node enters the scene tree for the first time.
func _ready():
	load_stage_request()


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	pass


func load_stage_request():
	var json = JSON.stringify({
		"UserName" : Global.user_name,
		"AuthToken" : Global.auth_token, 
		"ClientVersion" : Global.client_version,
		"MasterVersion" : Global.master_version,
	})
	var http = HTTPRequest.new() 
	add_child(http)
	http.request_completed.connect(_on_load_stage_response)
	http.request(Global.BASE_URL + "Stage/Info", Global.headers, Global.POST, json)
	
	
func _on_load_stage_response(result, response_code, headers, body):
	if response_code != 200:
		print(body.get_string_from_utf8())
		return 
		
	var json = JSON.parse_string(body.get_string_from_utf8())
	if json.error == 0:
		load_map(json.curStageId)
		
	else:
		var msg = Global.ERROR_MSG[str(json['error'])]
		Global.open_alert(msg)
	

func load_map(cur_stage_id):
	var map_array = map_list.get_children()
	var index = 1
	for node in map_list.get_children():
		if index <= cur_stage_id:
			node.get_node("Button").pressed.connect(enter_stage.bind(index))
		else:
			node.get_node("Button").disabled = true
		
		index+=1

##############################################################################	
	

func enter_stage(stage_id):
	enter_stage_request(stage_id)

	
func enter_stage_request(stage_id):
	var json = JSON.stringify({
		"UserName" : Global.user_name,
		"AuthToken" : Global.auth_token, 
		"ClientVersion" : Global.client_version,
		"MasterVersion" : Global.master_version,
		"StageId" : stage_id
	})
	var http = HTTPRequest.new() 
	add_child(http)
	http.request_completed.connect(_on_enter_stage_response)
	http.request(Global.BASE_URL + "Stage/Choice", Global.headers, Global.POST, json)
	
	
func _on_enter_stage_response(result, response_code, headers, body):
	if response_code != 200:
		print(body.get_string_from_utf8())
		return 
		
	var json = JSON.parse_string(body.get_string_from_utf8())
	if json.error == 0:
		print(json)
		
	else:
		var msg = Global.ERROR_MSG[str(json['error'])]
		Global.open_alert(msg)


func _on_texture_button_pressed():
	queue_free()
