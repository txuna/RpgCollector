extends CanvasLayer

@onready var item_name_label = $TextureRect/Label
@onready var attack_label = $TextureRect/AttackLabel
@onready var magic_label = $TextureRect/MagicLabel
@onready var defence_label = $TextureRect/DefenceLabel
@onready var item_texture = $TextureRect/TextureRect/TextureRect2
@onready var type_label = $TextureRect/Label2

# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	pass


func _on_open_detail_popup(player_item_id):
	get_player_item_info_request(player_item_id)


func get_player_item_info_request(player_item_id):
	var json = JSON.stringify({
		"UserName" : Global.user_name,
		"AuthToken" : Global.auth_token, 
		"ClientVersion" : Global.client_version,
		"MasterVersion" : Global.master_version,
		"PlayerItemId" : player_item_id
	})
	var http = HTTPRequest.new() 
	add_child(http)
	http.request_completed.connect(get_player_item_info_response)
	http.request(Global.BASE_URL + "Inventory/Item", Global.headers, Global.POST, json)
	
		
func get_player_item_info_response(result, response_code, headers, body):
	if response_code != 200:
		print(body.get_string_from_utf8())
		return 
		
	var json = JSON.parse_string(body.get_string_from_utf8())
	if json.error == 0:
		load_detail(json)
		
	else:
		var msg = Global.ERROR_MSG[str(json['error'])]
		Global.open_alert(msg)


func load_detail(json):
	item_name_label.text = "{item_name}(+{enchant_count})".format({
		"item_name" : json.itemName, 
		"enchant_count" : str(json.enchantCount)
	})
	item_texture.texture = MasterData.item_texture[str(json.itemId)]
	attack_label.text = "공격력 : {base}(+{plus})".format({
		"base" : str(json.baseAttack),
		"plus" : str(json.plusAttack)
	})
	magic_label.text = "마법력 : {base}(+{plus})".format({
		"base" : str(json.baseMagic),
		"plus" : str(json.plusMagic)
	})
	defence_label.text = "방어력 : {base}(+{plus})".format({
		"base" : str(json.baseDefence),
		"plus" : str(json.plusDefence)
	})
	type_label.text = "{attr}({type})".format({
	"attr" : json.attributeName, 
	"type" : json.typeName
	})


func _on_texture_button_pressed():
	queue_free()

