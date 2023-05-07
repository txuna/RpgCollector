extends CanvasLayer

@onready var inventory_container = $TextureRect/ScrollContainer/GridContainer

# Called when the node enters the scene tree for the first time.
func _ready():
	get_player_inventory_request()


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	pass


func get_player_inventory_request():
	var json = JSON.stringify({})
	var http = HTTPRequest.new() 
	add_child(http)
	http.request_completed.connect(get_player_inventory_response)
	http.request(Global.BASE_URL + "Inventory", Global.headers, Global.POST, json)
	
	
func get_player_inventory_response(result, response_code, headers, body):
	if response_code != 200:
		print(body.get_string_from_utf8())
		return 
		
	var json = JSON.parse_string(body.get_string_from_utf8())
	if json.error == 0:
		load_inventory(json.items)
		
	else:
		var msg = Global.ERROR_MSG[str(json['error'])]
		Global.open_alert(msg)


func init_inventory():
	for node in inventory_container.get_children():
		node.queue_free()


func load_inventory(items):
	init_inventory()
	for item in items:
		var back_texture = TextureRect.new() 
		var item_texture = TextureRect.new() 
		var quantity_label = Label.new() 
		
		back_texture.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
		back_texture.custom_minimum_size = Vector2(92, 92)
		
		item_texture.custom_minimum_size = Vector2(92, 92)
		#item_texture.anchors_preset = Control.PRESET_FULL_RECT
		
		quantity_label.add_theme_font_size_override("font_size", 16)
		quantity_label.add_theme_color_override("font_color", Color.BLACK)
		quantity_label.position = Vector2(64, 66)
		quantity_label.size = Vector2(28, 26)
		quantity_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_RIGHT
		
		back_texture.texture = load("res://assets/inventory_slot.png")
		item_texture.texture = MasterData.item_texture[str(item.itemId)]
		quantity_label.text = str(item.quantity)
		
		back_texture.add_child(item_texture)
		back_texture.add_child(quantity_label)
		inventory_container.add_child(back_texture)
		
		back_texture.gui_input.connect(_on_open_detail.bind(item.playerItemId))
		
		
func _on_open_detail(event: InputEvent, player_item_id):
	if event is InputEventMouseButton:
		if event.button_index == MOUSE_BUTTON_RIGHT and not event.pressed:		
			var detail_popup = load("res://src/ui/detail_popup.tscn").instantiate() 
			add_child(detail_popup)
			detail_popup._on_open_detail_popup(player_item_id)
		# item use !
		elif event.double_click:
			# 아이템 타입이 기타 아이템이라면 패스
			pass 
	return 	
		

func _on_exit_btn_pressed():
	queue_free()
