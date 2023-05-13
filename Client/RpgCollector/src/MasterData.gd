extends Node


var item_texture = {
	'1' : load("res://assets/coin.png"),
	'2' : load("res://assets/rusty_sword.png"),
	'3' : load("res://assets/sharp_knife.png"),
	'4' : load("res://assets/shield.png"),
	'5' : load("res://assets/biking_hat.png"),
	'6' : load("res://assets/hp_potion1.png"),
}

var item_data = {
	'1' : "돈", 
	'2' : "작은 칼",
	'3' : "도금 칼", 
	'4' : "나무 방패",
	'5' : "보통 모자",
	'6' : "포션",
}


var package_texture = {
	'1' : load("res://assets/one_pack.png"),
	'2' : load("res://assets/two_pack.png"),
	'3' : load("res://assets/three_pack.png")
}

var stage_info = {
	'1' : {
		'name' : '리스항구'
	},
	'2' : {
		'name' : '헤네시스'
	},
	'3' : {
		'name' : '엘리니아'
	},
	'4' : {
		'name' : "커닝시티"
	},
	'5' : {
		'name' : '페리온'
	}
}


var npc_data = {
	'101' : {
		"attack" : 1, 
		"defence" : 1, 
		"hp" : 10,
		'name' : '슬라임'
	},
	'201' : {
		"attack" : 1, 
		"defence" : 1, 
		"hp" : 10,
		'name' : '주황버섯'
	},
	'202' : {
		"attack" : 1, 
		"defence" : 1, 
		"hp" : 10,
		'name' : '파란버섯'
	},
	'301' : {
		"attack" : 1, 
		"defence" : 1, 
		"hp" : 10,
		'name' : '미니언'
	},
	'302' : {
		"attack" : 1, 
		"defence" : 1, 
		"hp" : 10,
		'name' : '파란슬라임'
	},
	'401' : {
		"attack" : 1, 
		"defence" : 1, 
		"hp" : 10,
		'name' : '레드슬라임'
	},
	'402' : {
		"attack" : 1, 
		"defence" : 1, 
		"hp" : 10,
		'name' : '레드미니언'
	},
	'501' : {
		"attack" : 1, 
		"defence" : 1, 
		"hp" : 10,
		'name' : '대포미니언'
	},
	
	'502' : {
		"attack" : 1, 
		"defence" : 1, 
		"hp" : 10,
		'name' : '블랙슬라임'
	},
	'503' : {
		"attack" : 1, 
		"defence" : 1, 
		"hp" : 10,
		'name' : '나무꾼'
	},
}
