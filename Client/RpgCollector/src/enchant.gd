extends CanvasLayer

@onready var inventory_container = $TextureRect2/ScrollContainer/GridContainer
@onready var enchant_texture = $TextureRect/TextureRect/TextureRect
@onready var current_enchant_label = $TextureRect/Label2
@onready var next_enchant_label = $TextureRect/Label3
@onready var percent_label = $TextureRect/Label4
@onready var increase_label = $TextureRect/Label5
@onready var price_label = $TextureRect/Label6

@onready var enchant_btn = $TextureRect/Button

var enchant_item_id = -1

# Called when the node enters the scene tree for the first time.
func _ready():
	init_info()
	load_inventory_request()


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	pass


func _on_texture_button_pressed():
	queue_free()


# DO ENCHANT
func _on_button_pressed():
	do_enchant_request()


func load_inventory_request():
	var json = JSON.stringify({})
	var http = HTTPRequest.new() 
	add_child(http)
	http.request_completed.connect(_on_load_inventory_response)
	http.request(Global.BASE_URL + "Inventory", Global.headers, Global.POST, json)
	
	
func _on_load_inventory_response(result, response_code, headers, body):
	if response_code != 200:
		print(body.get_string_from_utf8())
		return 
		
	var json = JSON.parse_string(body.get_string_from_utf8())
	if json.error == 0:
		load_inventory(json.items)
		
	else:
		var msg = Global.ERROR_MSG[str(json['error'])]
		Global.open_alert(msg)
	

func load_inventory(items):
	for node in inventory_container.get_children():
		node.queue_free()
		
	for item in items:
		var back_texture = TextureRect.new() 
		var item_texture = TextureRect.new() 
		var quantity_label = Label.new() 
		
		quantity_label.add_theme_font_size_override("font_size", 16)
		quantity_label.add_theme_color_override("font_color", Color.BLACK)
		quantity_label.position = Vector2(45, 45)
		#quantity_label.size = Vector2(28, 26)
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
			if get_node_or_null('detail') != null:
				return
			var detail_popup = load("res://src/ui/detail_popup.tscn").instantiate() 
			add_child(detail_popup)
			detail_popup.name = 'detail'
			detail_popup._on_open_detail_popup(player_item_id)
		# item use !
		elif event.double_click:
			get_enchant_info_request(player_item_id)
	return 		
	
	
func get_enchant_info_request(player_item_id):
	var json = JSON.stringify({
		'PlayerItemId' : player_item_id
	})
	var http = HTTPRequest.new() 
	add_child(http)
	http.request_completed.connect(_on_get_enchant_info_response)
	http.request(Global.BASE_URL + "Enchant/Info", Global.headers, Global.POST, json)
	
	
func _on_get_enchant_info_response(result, response_code, headers, body):
	if response_code != 200:
		print(body.get_string_from_utf8())
		return 
		
	var json = JSON.parse_string(body.get_string_from_utf8())
	if json.error == 0:
		load_enchant_info(json.currentEnchantCount, json.nextEnchantCount, json.percent, json.increasementValue, json.itemId, json.playerItemId, json.price)
		
	else:
		var msg = Global.ERROR_MSG[str(json['error'])]
		Global.open_alert(msg)



func load_enchant_info(current_enchant_count, next_enchant_count, percent, increasement_value, item_id, player_item_id, price):
	current_enchant_label.text = "현재 등급 : {c}".format({
		"c" : str(current_enchant_count)
	})
	next_enchant_label.text = "다음 등급 : {n}".format({
		"n" : str(next_enchant_count)
	})
	percent_label.text = "성공 확률 : {p}%".format({
		"p" : str(percent)
	})
	increase_label.text = "스탯 증가량 : {i}%".format({
		"i" : str(increasement_value)
	})
	price_label.text = "강화 비용 : ${p}".format({
		"p" : str(price)
	})
	enchant_texture.texture = MasterData.item_texture[str(item_id)]
	enchant_btn.disabled = false
	enchant_item_id = player_item_id
	

func init_info():
	current_enchant_label.text = "현재 등급 : "
	next_enchant_label.text = "다음 등급 : "
	percent_label.text = "성공 확률 : "
	increase_label.text = "스탯 증가량 : "
	price_label.text = "강화 비용 : "
	enchant_texture.texture = null
	enchant_item_id = -1
	enchant_btn.disabled = true

	
# 요청후 다시 인벤토리 로드
func do_enchant_request():
	if enchant_item_id == -1:
		Global.open_alert("강화할 장비를 선택해주세요.")
		return 
		
	var json = JSON.stringify({
		'PlayerItemId' : enchant_item_id
	})
	var http = HTTPRequest.new() 
	add_child(http)
	http.request_completed.connect(_on_do_enchant_response)
	http.request(Global.BASE_URL + "Enchant", Global.headers, Global.POST, json)
	
	
func _on_do_enchant_response(result, response_code, headers, body):
	if response_code != 200:
		print(body.get_string_from_utf8())
		return 
		
	var json = JSON.parse_string(body.get_string_from_utf8())
	if json.error == 0:
		if json.result == 1:
			Global.open_alert("해당 아이템의 강화가 성공하였습니다.")
			
		else:
			Global.open_alert("강화가 실패되어 아이템이 삭제됩니다.")
			
		load_inventory_request()
		init_info()
		
	else:
		var msg = Global.ERROR_MSG[str(json['error'])]
		Global.open_alert(msg)
	

func _on_texture_rect_gui_input(event):
	if event is InputEventMouseButton:
		if event.button_index == MOUSE_BUTTON_RIGHT and not event.pressed:		
			if get_node_or_null('detail') != null:
				return
				
			if enchant_item_id == -1:
				Global.open_alert("강화할 장비를 선택해주세요.")
				return 
				
			var detail_popup = load("res://src/ui/detail_popup.tscn").instantiate() 
			add_child(detail_popup)
			detail_popup.name = 'detail'
			detail_popup._on_open_detail_popup(enchant_item_id)

	return 	
