extends CanvasLayer

@onready var mail_container = $ColorRect/ScrollContainer/VBoxContainer
@onready var page_btn_list = $ColorRect/PageBtnList

# Called when the node enters the scene tree for the first time.
func _ready():
	open_mail_request(true, 1)


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	pass

func open_mail_request(is_first:bool, page_number:int):
	for node in mail_container.get_children():
		node.queue_free()

	var json = JSON.stringify({
		"IsFirstOpen" : is_first, 
		"PageNumber" : page_number
	})
	var http = HTTPRequest.new() 
	add_child(http)
	http.request_completed.connect(_on_http_open_mail_response)
	http.request(Global.BASE_URL + "Mail/Open", Global.headers, Global.POST, json)
	

func make_page_number(total_page_number):
	for node in page_btn_list.get_children():
		node.queue_free()
		
	for num in range(total_page_number):
		var btn = Button.new() 
		btn.text = str(num+1)
		btn.add_theme_font_size_override("font_size", 24)
		btn.pressed.connect(open_mail_request.bind(false, num+1))
		page_btn_list.add_child(btn)


func _on_exit_btn_pressed():
	queue_free()


func _on_http_open_mail_response(result, response_code, headers, body):
	if response_code != 200:
		print(body.get_string_from_utf8())
		return 
		
	var json = JSON.parse_string(body.get_string_from_utf8())
	if json.error == 0:
		make_page_number(json.totalPageNumber)
		for mail in json.mails:
			add_mail(mail.title, mail.mailId)
		
	else:
		var msg = Global.ERROR_MSG[str(json['error'])]
		Global.open_alert(msg)


func add_mail(title:String, mail_id:int):
	var mail_instance = load("res://src/ui/mail.tscn").instantiate()
	mail_container.add_child(mail_instance)
	mail_instance.setup(title, mail_id)
	mail_instance.custom_minimum_size = Vector2(600, 45)
	mail_instance.read_mail.connect(_on_read_mail)
	
	
func _on_read_mail(mail_id:int):
	var json = JSON.stringify({
		"MailId" : mail_id
	})
	var http = HTTPRequest.new()
	add_child(http)
	http.request_completed.connect(_on_http_read_mail_response)
	http.request(Global.BASE_URL + "Mail/Read", Global.headers, Global.POST, json)
	

func _on_http_read_mail_response(result, response_code, headers, body):
	if response_code != 200:
		print(body.get_string_from_utf8())
		return 
		
	var json = JSON.parse_string(body.get_string_from_utf8())
	if json.error == 0:
		var mail_content = load("res://src/ui/mail_content.tscn").instantiate()
		add_child(mail_content)
		mail_content.setup(json)
		mail_content.delete_mail.connect(_on_delete_mail)
		mail_content.get_item.connect(_on_get_item)
		
	else:
		var msg = Global.ERROR_MSG[str(json['error'])]
		Global.open_alert(msg)
	
	
func _on_get_item(mail_id):
	var json = JSON.stringify({
		"MailId" : mail_id
	})
	var http = HTTPRequest.new()
	add_child(http)
	http.request_completed.connect(_on_http_get_item_response)
	http.request(Global.BASE_URL + "Mail/Item", Global.headers, Global.POST, json)
	
	
func _on_delete_mail(mail_id):
	var json = JSON.stringify({
		"MailId" : mail_id
	})
	var http = HTTPRequest.new()
	add_child(http)
	http.request_completed.connect(_on_http_delete_mail_response)
	http.request(Global.BASE_URL + "Mail/Delete", Global.headers, Global.POST, json)
	
	

func _on_http_delete_mail_response(result, response_code, headers, body):
	if response_code != 200:
		return 
		
	var json = JSON.parse_string(body.get_string_from_utf8())
	if json.error == 0:
		Global.open_alert("해당 매일이 정상적으로 삭제되었습니다.")
		
	else:
		var msg = Global.ERROR_MSG[str(json['error'])]
		Global.open_alert(msg)
	
	queue_free()
	
	
func _on_http_get_item_response(result, response_code, headers, body):
	if response_code != 200:
		return 
		
	var json = JSON.parse_string(body.get_string_from_utf8())
	if json.error == 0:
		Global.open_alert("해당 아이템 수령이 완료되었습니다.")
		
	else:
		var msg = Global.ERROR_MSG[str(json['error'])]
		Global.open_alert(msg)
	
	queue_free()
	
	
	
	
	
	
