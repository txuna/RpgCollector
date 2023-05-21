extends CanvasLayer

@onready var stage_name_label = $TextureRect/Label
@onready var npc_container = $TextureRect/TextureRect4/ScrollContainer/NpcContainer
@onready var item_container = $TextureRect/TextureRect3/ScrollContainer2/ItemFarmingContainer
@onready var npc_hunting_btn = $TextureRect/Button
@onready var hp_label = $TextureRect/TextureRect/HpLabel
@onready var hp_bar = $TextureRect/TextureRect/TextureProgressBar
@onready var combat_container = $TextureRect/TextureRect2/ScrollContainer3/CombatContainer

var stage_info = {}
var stage_id 

var item_farming_list = {
	
}
# Called when the node enters the scene tree for the first time.
func _ready():
	for node in combat_container.get_children():
			node.queue_free() 
			
	_on_load_player_state_request()
	
	
# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	pass


# { "error": 0, "items": [{ "itemId": 2 }], "npcs": [{ "npcId": 101, "count": 10 }] }
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
	
#{ "error": 0, "items": [{ "itemId": 2, "farmingCount": 0, "maxCount": 1 }], "npcs": [{ "npcId": 101, "count": 10, "remaingCount": 8, "exp": 10 }] }
func init_continue_setup(json):
	stage_id = json.stageId
	# stage_info 설정 
	stage_info["error"] = json.error 
	stage_info["items"] = [] 
	stage_info["npcs"] = []
	
	stage_name_label.text = "{name}(이어하기)".format({
	"name" : MasterData.stage_info[str(stage_id)].name
	})
	
	for item in json.items:
		stage_info["items"].append({
			"itemId" : item.itemId
		})
		
	for npc in json.npcs:
		stage_info["npcs"].append({
			"npcId" : npc.npcId,
			"count" : npc.remaingCount
		})
	
	# item_farming_list 설정
	for item in json.items:
		item_farming_list[item.itemId] = {
			"max_count" : item.maxCount, 
			"farming_count" : item.farmingCount
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
		
		label.add_theme_font_size_override("font_size", 12)
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
		
		label.add_theme_font_size_override("font_size", 12)
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
			simulate_combat(npc.npcId)
			#hunting_npc_request(npc.npcId)
			npc_hunting_btn.disabled = true
			break 

	if is_clear:
		npc_hunting_btn.text = "스테이지 클리어"
		npc_hunting_btn.disabled = true
		# 스테이지 클리어시 파밍 아이템 전송 (받은 아이템 코드) 
		clear_stage_request()

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
		var label = Label.new() 
		label.text = "[{n}]을/를 쓰러트렸습니다.".format({
			"n" : MasterData.npc_data[str(json.npcId)]["name"]
		})
		label.add_theme_color_override("font_color", Color.BLACK)
		combat_container.add_child(label)
		hp_label.text = "HP : [{c} / {m}]".format({
			"c" : PlayerState.player_state.state.hp, 
			"m" : PlayerState.player_state.master_state.hp
		})
		hp_bar.max_value = PlayerState.player_state.master_state.hp
		hp_bar.value = PlayerState.player_state.state.hp
		
		for npc in stage_info.npcs:
			if npc["npcId"] == json.npcId:
				npc["count"] -= 1
				break 
		load_npc()
		
		var is_field_clear = false
		for npc in stage_info.npcs:
			if npc.count > 0:
				is_field_clear = true 
				continue  
		
		if not is_field_clear:
			farming_item()
				
		npc_hunting_btn.disabled = false

	else:
		var msg = Global.ERROR_MSG[str(json['error'])]
		Global.open_alert(msg)	
		queue_free()
		

# farming_count가 max_count를 초과하지 않도록
func farming_item():
	var is_full = true 
	for key in item_farming_list:
		if item_farming_list[key].farming_count < item_farming_list[key].max_count:
			is_full = false 
			break
			
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
		queue_free()
		

# 보낼 때 플레이어 상태도 같이 보냄 (HP 상태)
func clear_stage_request():
	var json = JSON.stringify({
		"UserName" : Global.user_name,
		"AuthToken" : Global.auth_token, 
		"ClientVersion" : Global.client_version,
		"MasterVersion" : Global.master_version,
		"PlayerHp" : PlayerState.player_state.state.hp
	})
	var http = HTTPRequest.new() 
	add_child(http)
	http.request_completed.connect(_on_clear_stage_response)
	http.request(Global.BASE_URL + "Stage/Clear", Global.headers, Global.POST, json)
	
	
func _on_clear_stage_response(result, response_code, headers, body):
	if response_code != 200:
		print(body.get_string_from_utf8())
		return 
		
	var json = JSON.parse_string(body.get_string_from_utf8())
	if json.error == 0:
		Global.open_alert("스테이지가 클리어 되었습니다.")
		get_node("/root/Main/ColorRect/Hud").player_state_request()
		queue_free()

	else:
		var msg = Global.ERROR_MSG[str(json['error'])]
		Global.open_alert(msg)	
		queue_free()

# Stage OUT 스테이지 나가기
func _on_texture_button_pressed():
	var json = JSON.stringify({
		"UserName" : Global.user_name,
		"AuthToken" : Global.auth_token, 
		"ClientVersion" : Global.client_version,
		"MasterVersion" : Global.master_version,
	})
	var http = HTTPRequest.new() 
	add_child(http)
	http.request_completed.connect(_on_exit_stage_response)
	http.request(Global.BASE_URL + "Stage/Exit", Global.headers, Global.POST, json)
	

func _on_exit_stage_response(result, response_code, headers, body):
	if response_code != 200:
		print(body.get_string_from_utf8())
		return 
		
	var json = JSON.parse_string(body.get_string_from_utf8())
	if json.error == 0:
		Global.open_alert("스테이지를 정상적으로 나갔습니다.")
		queue_free()

	else:
		var msg = Global.ERROR_MSG[str(json['error'])]
		Global.open_alert(msg)	
		queue_free()


# 전투 시작시 딱 한번 호출
func _on_load_player_state_request():
	var json = JSON.stringify({
		"UserName" : Global.user_name,
		"AuthToken" : Global.auth_token, 
		"ClientVersion" : Global.client_version,
		"MasterVersion" : Global.master_version,
	})
	var http = HTTPRequest.new() 
	add_child(http)
	http.request_completed.connect(_on_load_player_state_response)
	http.request(Global.BASE_URL + "Player/State", Global.headers, Global.POST, json)
	
	
func _on_load_player_state_response(result, response_code, headers, body):
	if response_code != 200:
		print(body.get_string_from_utf8())
		return 
		
	var json = JSON.parse_string(body.get_string_from_utf8())
	if json.error == 0:
		set_player_state(json)
		
	else:
		var msg = Global.ERROR_MSG[str(json['error'])]
		Global.open_alert(msg)


# 추후 장비또한 착용
func set_player_state(json):
	hp_label.text = "HP : [{c} / {m}]".format({
			"c" : json.state.hp, 
			"m" : json.masterState.hp
		})
	
	hp_bar.max_value = json.state.hp
	hp_bar.value = json.masterState.hp
	
	PlayerState.player_state.state.hp = json.state.hp
	PlayerState.player_state.master_state.hp = json.masterState.hp
	PlayerState.player_state.state.attack = json.state.attack 
	PlayerState.player_state.state.magic = json.state.magic
	PlayerState.player_state.state.defence = json.state.defence


# 여기서 승리해야 hunting_npc_request 호출 
# 선공은 player
# player나 npc의 체력이 0이 되어야 종료
# 만약 player의 체력이 0이 된다면 던전 아웃
func simulate_combat(npc_id):
	var npc = MasterData.npc_data[str(npc_id)].duplicate()
	var combat_order = ["player", "npc"]
	
	while npc.hp > 0 and PlayerState.player_state.state.hp > 0:
		if combat_order[0] == "player":
			PlayerState.player_state.state.hp -= npc.attack 
			
		elif combat_order[0] == "npc":
			npc.hp -= PlayerState.player_state.state.attack

		combat_order.reverse()

	if npc.hp <= 0:
		hunting_npc_request(npc_id)

	elif PlayerState.player_state.state.hp <= 0:
		Global.open_alert("던전에서 쓰러졌습니다.")
		_on_texture_button_pressed()













