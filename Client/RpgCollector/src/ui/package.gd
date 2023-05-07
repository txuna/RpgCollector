extends CanvasLayer

@onready var package_container = $TextureRect/ScrollContainer/GridContainer

# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	pass


func get_package_list_request():
	var json = JSON.stringify({})
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
		pass
		
	else:
		var msg = Global.ERROR_MSG[str(json['error'])]
		Global.open_alert(msg)
		

func buy_package_request():
	var json = JSON.stringify({
		"ReceiptId" : 1,
		"PackageId" : 1
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
		pass
		
	else:
		var msg = Global.ERROR_MSG[str(json['error'])]
		Global.open_alert(msg)	
