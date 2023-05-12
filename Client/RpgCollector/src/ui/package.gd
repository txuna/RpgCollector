extends CanvasLayer

@onready var package_container = $TextureRect/ScrollContainer/GridContainer

# Called when the node enters the scene tree for the first time.
func _ready():
	get_package_list_request()


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	pass


func get_package_list_request():
	var json = JSON.stringify({
		"UserName" : Global.user_name,
		"AuthToken" : Global.auth_token, 
		"ClientVersion" : Global.client_version,
		"MasterVersion" : Global.master_version
	})
	var http = HTTPRequest.new() 
	add_child(http)
	http.request_completed.connect(_on_get_package_list_response)
	http.request(Global.BASE_URL + "Package/Show", Global.headers, Global.POST, json)
	

func _on_get_package_list_response(result, response_code, headers, body):
	if response_code != 200:
		print(body.get_string_from_utf8())
		return 
		
	var json = JSON.parse_string(body.get_string_from_utf8())
	if json.error == 0:
		load_package(json.packagePayment)
		
	else:
		var msg = Global.ERROR_MSG[str(json['error'])]
		Global.open_alert(msg)
		
		
func load_package(packages):
	for node in package_container.get_children():
		node.queue_free()
		
	for package in packages:
		var back_texture = TextureRect.new() 
		var package_texture = TextureRect.new() 
		var button = Button.new() 
		var label = Label.new()
		
		back_texture.custom_minimum_size = Vector2(156, 156)
		package_texture.custom_minimum_size = Vector2(156, 156)
		label.position = Vector2(2, 160)
		label.add_theme_font_size_override("font_size", 16)
		label.add_theme_color_override("font_color", Color.BLACK)
		button.add_theme_font_size_override("font_size", 24)
		button.position = Vector2(0, 190)
		button.size = Vector2(155, 42)
		
		back_texture.texture = load("res://assets/inventory_slot.png")
		package_texture.texture = MasterData.package_texture[str(package.packageId)]
		button.text = "구매하기"
		button.pressed.connect(buy_package_request.bind(package.packageId))
		label.text = "{name} ${price}".format({
			"name" : package.packageName, 
			"price" : package.price
		})
		back_texture.add_child(package_texture)
		back_texture.add_child(button)
		back_texture.add_child(label)
		package_container.add_child(back_texture)
	

func buy_package_request(package_id):
	var json = JSON.stringify({
		"UserName" : Global.user_name,
		"AuthToken" : Global.auth_token, 
		"ClientVersion" : Global.client_version,
		"MasterVersion" : Global.master_version,
		"ReceiptId" : Global.get_random_receipt_id(),
		"PackageId" : package_id
	})
	var http = HTTPRequest.new() 
	add_child(http)
	http.request_completed.connect(_on_buy_package_response)
	http.request(Global.BASE_URL + "Package/Buy", Global.headers, Global.POST, json)
	
	
func _on_buy_package_response(result, response_code, headers, body):
	if response_code != 200:
		print(body.get_string_from_utf8())
		return 
		
	var json = JSON.parse_string(body.get_string_from_utf8())
	if json.error == 0:
		Global.open_alert("패키지가 우편으로 보내집니다.")
		
	else:
		var msg = Global.ERROR_MSG[str(json['error'])]
		Global.open_alert(msg)	


func _on_texture_button_pressed():
	queue_free()
