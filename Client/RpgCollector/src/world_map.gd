extends CanvasLayer

@onready var map_list = $TextureRect/Control

var cur_stage_id = -1
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
			node.get_node("Button").text = MasterData.stage_info[str(index)].name
			node.get_node("Button").pressed.connect(enter_stage.bind(index))
		else:
			node.get_node("Button").text = MasterData.stage_info[str(index)].name
			node.get_node("Button").disabled = true
		
		index+=1

##############################################################################	
	

func enter_stage(stage_id):
	if get_node_or_null("stage_msg_box") != null:
		return 
		
	var instance = load("res://src/ui/enter_stage_msg_box.tscn").instantiate()
	instance.name = "stage_msg_box"
	add_child(instance)
	instance.set_stage_name(MasterData.stage_info[str(stage_id)].name)
	instance.enter_stage.connect(enter_stage_request.bind(stage_id))

	
func enter_stage_request(stage_id):
	var json = JSON.stringify({
		"UserName" : Global.user_name,
		"AuthToken" : Global.auth_token, 
		"ClientVersion" : Global.client_version,
		"MasterVersion" : Global.master_version,
		"StageId" : stage_id
	})
	cur_stage_id = stage_id
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
		load_stage(json, cur_stage_id)
		
	else:
		var msg = Global.ERROR_MSG[str(json['error'])]
		Global.open_alert(msg)


func load_stage(json, stage_id):
	var instance = load("res://src/stage.tscn").instantiate()
	get_parent().add_child(instance)
	instance.init_setup(json, stage_id)
	

func _on_texture_button_pressed():
	queue_free()
