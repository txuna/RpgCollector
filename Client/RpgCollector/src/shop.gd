extends CanvasLayer

@onready var buy_container = $TextureRect/TextureRect/ScrollContainer/BuyContainer
@onready var sell_container = $TextureRect/TextureRect2/ScrollContainer/SellContainer

# Called when the node enters the scene tree for the first time.
func _ready():
	load_buy_product()
	_on_load_player_items_request()

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	pass


func _on_texture_button_pressed():
	queue_free()


func _on_load_player_items_request():
	var json = JSON.stringify({
		"UserName" : Global.user_name,
		"AuthToken" : Global.auth_token, 
		"ClientVersion" : Global.client_version,
		"MasterVersion" : Global.master_version,
	})
	var http = HTTPRequest.new() 
	add_child(http)
	http.request_completed.connect(_on_load_player_items_response)
	http.request(Global.BASE_URL + "Inventory", Global.headers, Global.POST, json)
	

func _on_load_player_items_response(result, response_code, headers, body):
	if response_code != 200:
		print(body.get_string_from_utf8())
		return 
		
	var json = JSON.parse_string(body.get_string_from_utf8())
	if json.error == 0:
		load_sell_product(json.items)
		
	else:
		var msg = Global.ERROR_MSG[str(json['error'])]
		Global.open_alert(msg)


func load_sell_product(items):
	for node in sell_container.get_children():
		node.queue_free()

	for item in items:
		if item.itemId == 1:
			continue 
			
		var outer_hbox = HBoxContainer.new() 
		var inner_vbox = VBoxContainer.new() 
		var outer_texture = TextureRect.new() 
		var inner_texture = TextureRect.new() 
		var name_label = Label.new() 
		var price_label = Label.new() 
		var button = Button.new() 
	
		outer_texture.texture = load("res://assets/inventory_slot.png")
		inner_texture.texture = MasterData.item_texture[str(item.itemId)]
		outer_texture.gui_input.connect(_on_open_detail.bind(item.playerItemId))
		
		inner_vbox.size_flags_horizontal = Control.SIZE_EXPAND_FILL
		
		button.text = "판매"
		button.custom_minimum_size = Vector2(70, 0)
		button.size_flags_horizontal = Control.SIZE_SHRINK_END
		button.pressed.connect(_on_sell_request.bind(item.playerItemId))
		
		var style = StyleBoxFlat.new() 
		style.bg_color = Color.BLACK 
		style.border_color = Color.BLACK 
		style.set_border_width_all(2)
		style.set_corner_radius_all(4)
		button.add_theme_stylebox_override("normal", style)
		
		name_label.text = MasterData.item_data[str(item.itemId)]
		price_label.text = "${p}".format({
			"p" : MasterData.item_price[str(item.itemId)].sell
		})
		
		name_label.add_theme_color_override("font_color", Color.BLACK)
		price_label.add_theme_color_override("font_color", Color.BLACK)
		
		outer_texture.add_child(inner_texture)
		inner_vbox.add_child(name_label)
		inner_vbox.add_child(price_label)
		
		outer_hbox.add_child(outer_texture)
		outer_hbox.add_child(inner_vbox)
		outer_hbox.add_child(button)
		
		sell_container.add_child(outer_hbox)



func load_buy_product():
	for node in buy_container.get_children():
		node.queue_free()

	for item_id in MasterData.item_data:
		if item_id == '1':
			continue 
			
		var outer_hbox = HBoxContainer.new() 
		var inner_vbox = VBoxContainer.new() 
		var outer_texture = TextureRect.new() 
		var inner_texture = TextureRect.new() 
		var name_label = Label.new() 
		var price_label = Label.new() 
		var button = Button.new() 
	
		outer_texture.texture = load("res://assets/inventory_slot.png")
		inner_texture.texture = MasterData.item_texture[item_id]
		
		inner_vbox.size_flags_horizontal = Control.SIZE_EXPAND_FILL
		
		button.text = "구매"
		button.custom_minimum_size = Vector2(70, 0)
		button.size_flags_horizontal = Control.SIZE_SHRINK_END
		button.pressed.connect(_on_buy_request.bind(int(item_id)))
		
		var style = StyleBoxFlat.new() 
		style.bg_color = Color.BLACK 
		style.border_color = Color.BLACK 
		style.set_border_width_all(2)
		style.set_corner_radius_all(4)
		button.add_theme_stylebox_override("normal", style)
		
		name_label.text = MasterData.item_data[item_id]
		price_label.text = "${p}".format({
			"p" : MasterData.item_price[item_id].buy
		})
		
		name_label.add_theme_color_override("font_color", Color.BLACK)
		price_label.add_theme_color_override("font_color", Color.BLACK)
		
		outer_texture.add_child(inner_texture)
		inner_vbox.add_child(name_label)
		inner_vbox.add_child(price_label)
		
		outer_hbox.add_child(outer_texture)
		outer_hbox.add_child(inner_vbox)
		outer_hbox.add_child(button)
		
		buy_container.add_child(outer_hbox)
	
	
func _on_buy_request(item_id):
	var json = JSON.stringify({
		"UserName" : Global.user_name,
		"AuthToken" : Global.auth_token, 
		"ClientVersion" : Global.client_version,
		"MasterVersion" : Global.master_version,
		"ItemId" : item_id
	})
	var http = HTTPRequest.new() 
	add_child(http)
	http.request_completed.connect(_on_buy_response)
	http.request(Global.BASE_URL + "Shop/Buy", Global.headers, Global.POST, json)
	

func _on_buy_response(result, response_code, headers, body):
	if response_code != 200:
		print(body.get_string_from_utf8())
		return 
		
	var json = JSON.parse_string(body.get_string_from_utf8())
	if json.error == 0:
		pass
		
	else:
		var msg = Global.ERROR_MSG[str(json['error'])]
		Global.open_alert(msg)
		
		
func _on_sell_request(player_item_id):
	var json = JSON.stringify({
		"UserName" : Global.user_name,
		"AuthToken" : Global.auth_token, 
		"ClientVersion" : Global.client_version,
		"MasterVersion" : Global.master_version,
		"PlayerItemId" : player_item_id
	})
	var http = HTTPRequest.new() 
	add_child(http)
	http.request_completed.connect(_on_sell_response)
	http.request(Global.BASE_URL + "Shop/Sell", Global.headers, Global.POST, json)
	

func _on_sell_response(result, response_code, headers, body):
	if response_code != 200:
		print(body.get_string_from_utf8())
		return 
		
	var json = JSON.parse_string(body.get_string_from_utf8())
	if json.error == 0:
		pass
		
	else:
		var msg = Global.ERROR_MSG[str(json['error'])]
		Global.open_alert(msg)
	
	
func _on_open_detail(event: InputEvent, player_item_id):
	if event is InputEventMouseButton:
		if event.button_index == MOUSE_BUTTON_RIGHT and not event.pressed:		
			if get_node_or_null('detail') != null:
				return
			var detail_popup = load("res://src/ui/detail_popup.tscn").instantiate() 
			detail_popup.name = 'detail'
			add_child(detail_popup)
			detail_popup._on_open_detail_popup(player_item_id)
		# item use !
		elif event.double_click:
			# 아이템 타입이 소비타입이라면 사용
			pass 
	return 	

	
	
