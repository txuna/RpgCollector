extends CanvasLayer

@onready var title = $ColorRect/TitleLabel
@onready var content = $ColorRect/ContentLabel
@onready var get_item_btn = $ColorRect/GetItemBtn
@onready var date = $ColorRect/DateLabel
@onready var item_label = $ColorRect/ItemLabel
@onready var item_texture = $ColorRect/TextureRect

signal get_item(item_id)
signal delete_mail(mail_id)

var item_id = -1
var mail_id = 0
var quantity = 0

# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	pass


func setup(json:Dictionary):
	title.text = json.title 
	content.text = json.content 
	date.text = json.sendDate
	mail_id = json.mailId
	
	if json.hasItem == 0 or json.mailItem == null:
		get_item_btn.disabled = true
		item_label.text = "아이템 포함되지 않은 메일입니다."
		
	else:
		if json.mailItem.hasReceived == 1:
			get_item_btn.disabled = true
			item_label.text = "이미 아이템을 수령한 메일입니다."
		else:
			item_id = json.mailItem.itemId 
			quantity = json.mailItem.quantity
			item_info_request(item_id)
		
		
func item_info_request(item_id):
	var json = JSON.stringify({
		"ItemId" : item_id
	})
	var http = HTTPRequest.new()
	add_child(http)
	http.request_completed.connect(_on_item_info_response)
	http.request(Global.BASE_URL + "Master/Item", Global.headers, Global.POST, json)


func _on_item_info_response(result, response_code, headers, body):
	if response_code != 200:
		print(body.get_string_from_utf8())
		return
		
	var json = JSON.parse_string(body.get_string_from_utf8())
	if json.error == 0:
		item_label.text = "{item_name} x {quantity}".format({
			"item_name" : json.masterItem.itemName,
			"quantity" : quantity
		})
		item_texture.texture = MasterData.item_texture[str(json.masterItem.itemId)]
		
	else:
		var msg = Global.ERROR_MSG[str(json['error'])]
		Global.open_alert(msg)
	

func _on_exit_mail_btn_pressed():
	queue_free()


func _on_get_item_btn_pressed():
	get_item.emit(mail_id)


func _on_delete_mail_btn_pressed():
	delete_mail.emit(mail_id)
