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
		load_detail(json.itemPrototype, json.plusState, json.enchantCount, json.attributeName, json.typeName)
		
	else:
		var msg = Global.ERROR_MSG[str(json['error'])]
		Global.open_alert(msg)


func load_detail(item_prototype, plus_state, enchant_count, attr_name, type_name):
	item_name_label.text = "{item_name}(+{enchant_count})".format({
		"item_name" : item_prototype.itemName, 
		"enchant_count" : str(enchant_count)
	})
	item_texture.texture = MasterData.item_texture[str(item_prototype.itemId)]
	attack_label.text = "공격력 : {base}(+{plus})".format({
		"base" : str(item_prototype.attack),
		"plus" : str(plus_state.attack)
	})
	magic_label.text = "마법력 : {base}(+{plus})".format({
		"base" : str(item_prototype.magic),
		"plus" : str(plus_state.magic)
	})
	defence_label.text = "방어력 : {base}(+{plus})".format({
		"base" : str(item_prototype.defence),
		"plus" : str(plus_state.defence)
	})
	type_label.text = "{attr}({type})".format({
	"attr" : attr_name, 
	"type" : type_name
	})


func _on_texture_button_pressed():
	queue_free()

