extends CanvasLayer

@onready var hp_label = $Control/Label
@onready var exp_label = $Control/Label2
@onready var level_label = $Control/Label4
@onready var money_label = $Control/Label3

@onready var hp_progress_bar = $Control/TextureProgressBar
@onready var exp_progress_bar = $Control/TextureProgressBar2

# Called when the node enters the scene tree for the first time.
func _ready():
	player_state_request()


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	pass



func player_state_request():
	var json = JSON.stringify({
		"UserName" : Global.user_name,
		"AuthToken" : Global.auth_token, 
		"ClientVersion" : Global.client_version,
		"MasterVersion" : Global.master_version
	})
	
	print(Global.auth_token)
	print(json)
	var http = HTTPRequest.new() 
	add_child(http)
	http.request_completed.connect(_on_player_state_response)
	http.request(Global.BASE_URL + "Player/State", Global.headers, Global.POST, json)
	
	
func _on_player_state_response(result, response_code, headers, body):
	if response_code != 200:
		print(body.get_string_from_utf8())
		return 
		
	var json = JSON.parse_string(body.get_string_from_utf8())
	if json.error == 0:
		update_hud(json.state, json.masterState)
		
	else:
		var msg = Global.ERROR_MSG[str(json['error'])]
		Global.open_alert(msg)


func update_hud(player_state, master_state):
	hp_label.text = "HP: [ {c} / {m} ]".format({
		"c" : str(player_state.hp),
		"m" : str(master_state.hp)
	})
	exp_label.text = "EXP: [ {c} / {m} ]".format({
		"c" : str(player_state.exp),
		"m" : str(master_state.exp)
	})
	level_label.text = "Lv. {l}".format({
		"l" : str(player_state.level)
	})
	money_label.text = "Money: ${m}".format({
		"m" : str(player_state.money)
	})
	hp_progress_bar.value = int(player_state.hp / master_state.hp * 100)
	exp_progress_bar.value = int(player_state.exp / master_state.exp * 100)
	












