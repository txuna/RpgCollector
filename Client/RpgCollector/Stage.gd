extends CanvasLayer

@onready var stage_name_label = $TextureRect/Label
@onready var npc_container = $TextureRect/ScrollContainer/VBoxContainer
@onready var item_container = $TextureRect/ScrollContainer2/VBoxContainer
@onready var npc_hunting_btn = $TextureRect/Button

var stage_info
var stage_id 

var item_farming_list = {
	
}
# Called when the node enters the scene tree for the first time.
func _ready():
	pass
	
	
# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	pass


func init_setup(json, _stage_id):
	stage_info = json 
	stage_id = _stage_id
	
	stage_name_label.text = "{name}".format({
	"name" : MasterData.stage_info[str(stage_id)].name
	})
	for item in stage_info.items:
		item_farming_list[item.itemId] = {
			"max_count" : item.quantity, 
			"farming_count" : 0
		}

	load_npc()
	load_item()
	

func load_npc():
	for node in npc_container.get_children():
		node.queue_free()
	
	for npc in stage_info.npcs:
		var label = Label.new() 
		label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
		label.vertical_alignment = VERTICAL_ALIGNMENT_CENTER
		
		label.add_theme_font_size_override("font_size", 18)
		label.add_theme_color_override("font_color", Color.BLACK)
		
		label.text = "{name} : {num}마리".format({
			"name" : MasterData.npc_data[str(npc.npcId)].name,
			"num" : str(npc.count)
		})
		
		npc_container.add_child(label)


func load_item():
	for node in item_container.get_children():
		node.queue_free()
		
	for item in stage_info.items:
		var label = Label.new() 
		label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
		label.vertical_alignment = VERTICAL_ALIGNMENT_CENTER
		
		label.add_theme_font_size_override("font_size", 18)
		label.add_theme_color_override("font_color", Color.BLACK)

		label.text = "{name} - [{c} / {m}]".format({
			"name" : MasterData.item_data[str(item.itemId)], 
			"c" : item_farming_list[item.itemId].farming_count, 
			"m" : item_farming_list[item.itemId].max_count
		})
		
		item_container.add_child(label)
		

# 남은 NPC 체크
func _on_button_pressed():
	var is_clear = true 
	for npc in stage_info.npcs:
		if npc.count > 0:
			is_clear = false 
			hunting_npc_request(npc.npcId)
			npc_hunting_btn.disabled = true
			break 
	
	if is_clear:
		npc_hunting_btn.text = "스테이지 클리어"
		npc_hunting_btn.disabled = true
		# 스테이지 클리어시 파밍 아이템 전송 (받은 아이템 코드) 
		farming_item()
		print("스테이지 클리어")


#stage_info에 count 남은거 처리 
# { "error": 0, "items": [{ "itemId": 2 }], "npcs": [{ "npcId": 101, "count": 10 }] }
func hunting_npc_request(npc_id):
	var json = JSON.stringify({
		"UserName" : Global.user_name,
		"AuthToken" : Global.auth_token, 
		"ClientVersion" : Global.client_version,
		"MasterVersion" : Global.master_version,
		"NpcId" : npc_id
	})
	var http = HTTPRequest.new() 
	add_child(http)
	http.request_completed.connect(_on_hunting_npc_response)
	http.request(Global.BASE_URL + "Stage/Hunting/Npc", Global.headers, Global.POST, json)
	

# 요청보낸 NPC에 대해 count 감소
func _on_hunting_npc_response(result, response_code, headers, body):
	if response_code != 200:
		print(body.get_string_from_utf8())
		return 
		
	var json = JSON.parse_string(body.get_string_from_utf8())
	if json.error == 0:
		for npc in stage_info.npcs:
			if npc["npcId"] == json.npcId:
				npc["count"] -= 1
				break 
		load_npc()
		npc_hunting_btn.disabled = false

	else:
		var msg = Global.ERROR_MSG[str(json['error'])]
		Global.open_alert(msg)	


# farming_count가 max_count를 초과하지 않도록
func farming_item():
	var is_full = true 
	for key in item_farming_list:
		if item_farming_list[key].farming_count < item_farming_list[key].max_count:
			is_full = false 
			
	if is_full:
		return 
		
	while true:
		var rng = RandomNumberGenerator.new()
		var keys = item_farming_list.keys() 
		var choice_key = keys[rng.randi() % keys.size()]
		if item_farming_list[choice_key].farming_count >= item_farming_list[choice_key].max_count:
			continue 
		else:
			farming_item_request(choice_key)
			break
	

func farming_item_request(item_id):
	var json = JSON.stringify({
		"UserName" : Global.user_name,
		"AuthToken" : Global.auth_token, 
		"ClientVersion" : Global.client_version,
		"MasterVersion" : Global.master_version,
		"ItemId" : item_id
	})
	var http = HTTPRequest.new() 
	add_child(http)
	http.request_completed.connect(_on_farming_item_response)
	http.request(Global.BASE_URL + "Stage/Farming/Item", Global.headers, Global.POST, json)
	

# farmingCount ++ 
func _on_farming_item_response(result, response_code, headers, body):
	if response_code != 200:
		print(body.get_string_from_utf8())
		return 
		
	var json = JSON.parse_string(body.get_string_from_utf8())
	if json.error == 0:
		for item in item_farming_list:
			if item == json.itemId:
				item_farming_list[item].farming_count += 1
				break 
				
		load_item()

	else:
		var msg = Global.ERROR_MSG[str(json['error'])]
		Global.open_alert(msg)	
	
	
func clear_stage_request():
	pass
	
	
func _on_clear_stage_response():
	pass
	

func _on_texture_button_pressed():
	queue_free()

