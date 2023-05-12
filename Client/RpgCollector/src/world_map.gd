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
		load_map(json.stages)
		
	else:
		var msg = Global.ERROR_MSG[str(json['error'])]
		Global.open_alert(msg)
	

func load_map(stages):
	var map_array = map_list.get_children()
	for i in range(stages.size()):
		if stages[i].isOpen == true:
			map_array[i].get_node("Button").pressed.connect(enter_stage.bind(stages[i].stageId))
			
		else:
			map_array[i].get_node("Button").disabled = true
		

##############################################################################	
	

func enter_stage(stage_id):
	print(stage_id)

	
func enter_stage_request():
	var json = JSON.stringify({
	})
	var http = HTTPRequest.new() 
	add_child(http)
	http.request_completed.connect(_on_enter_stage_response)
	http.request(Global.BASE_URL + "/Stage/Choice", Global.headers, Global.POST, json)
	
	
func _on_enter_stage_response(result, response_code, headers, body):
	if response_code != 200:
		print(body.get_string_from_utf8())
		return 
		
	var json = JSON.parse_string(body.get_string_from_utf8())
	if json.error == 0:
		pass
		
	else:
		var msg = Global.ERROR_MSG[str(json['error'])]
		Global.open_alert(msg)


func _on_texture_button_pressed():
	queue_free()
