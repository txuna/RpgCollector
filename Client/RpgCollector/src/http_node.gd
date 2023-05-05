extends Node

@onready var http_request = $HTTPRequest

const BASE_URL = "http://localhost:5271/Login"
const POST = HTTPClient.METHOD_POST


func login_request(username, password):
	var json = JSON.stringify({
		"UserName" : username,
		"Password" : password
	})
	var headers = ["Content-Type: application/json", 
					"Client-Version: 1.0.0", 
					"MasterData-Version: 1.0.0"]
	http_request.request(BASE_URL, headers, POST, json)
