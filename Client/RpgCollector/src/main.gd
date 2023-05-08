extends Node2D


# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	pass


func _on_mailbox_btn_pressed():
	if get_node_or_null('mailbox') != null:
		return 
		
	var mailbox_instance = load("res://src/ui/mailbox.tscn").instantiate() 
	mailbox_instance.name = "mailbox"
	add_child(mailbox_instance)


func _on_attendance_btn_pressed():
	if get_node_or_null("attendance") != null:
		return 
		
	var attendance_book_instance = load("res://src/attendance_book.tscn").instantiate()
	attendance_book_instance.name = "attendance"
	add_child(attendance_book_instance)


func _on_notice_btn_pressed():
	if get_node_or_null("notice") != null:
		return 
		
	var notice_instance = load("res://src/notice.tscn").instantiate()
	notice_instance.name = "notice"
	add_child(notice_instance)


func _on_inventory_btn_pressed():
	if get_node_or_null("inventory") != null:
		return 
		
	var inventory_instance = load("res://src/ui/inventory.tscn").instantiate()
	inventory_instance.name = 'inventory'
	add_child(inventory_instance)


func _on_enchant_btn_pressed():
	if get_node_or_null("enchant") != null:
		return 
		
	var enchant_instance = load("res://src/enchant.tscn").instantiate()
	enchant_instance.name = 'enchant'
	add_child(enchant_instance)


func _on_payment_btn_pressed():
	if get_node_or_null("payment") != null:
		return 
		
	var payment_instance = load("res://src/ui/package.tscn").instantiate() 
	payment_instance.name = "payment"
	add_child(payment_instance)
