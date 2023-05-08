extends Node

const BASE_URL = "http://localhost:5271/"
const POST = HTTPClient.METHOD_POST

var user_name = ""
var auth_token = ""

var headers = ["Content-Type: application/json", 
				"Client-Version: 1.0.0", 
				"MasterData-Version: 1.0.0"]
				
const ERROR_MSG = {
	'0' : "정상",
	'1' : "유효하지 않은 모델입니다. ",
	'3' : "유저이름이 존재하지 않습니다.",
	'4' : "레디스 서버와 연결을 실패했습니다.",
	'5' : "mysql 서버와 연결을 실패했습니다.",
	'6' : "데이터베이스와의 연결을 실패했습니다.",
	'7' : "존재하지 않는 아이템입니다.",
	'101' : "비밀번호가 틀렸습니다.",
	'102' : "유저이름이 틀렸습니다.",
	'103' : "회원가입이 실패했습니다.",
	'104' : "플레이어 생성을 실패했습니다. 계정이 삭제됩니다.",
	'105' : "계정 롤백이 실패했습니다. 고객센터 문의 부탁드립니다.",
	'106' : "이미 존재하는 유저이름 입니다.",
	'201' : "유효하지 않은 페이지 번호입니다.",
	'202' : "메일을 정상적으로 불러오지 못했습니다.",
	'203' : "존재하지 않는 메일입니다", 
	'204' : "이미 읽은 메일입니다", 
	'205' : "해당 메일에 아이템이 존재하지 않습니다.",
	'206' : "이미 아이템을 수령한 메일입니다.",
	'207' : "플레이어에게 메일 아이템을 추가하는 것을 실패했습니다.",
	'208' : "해당 메일의 소유자가 아닙니다.",
	'209' : "메일 전송이 실패했습니다.",
	'210' : "이미 삭제된 메일입니다,",
	'211' : '메일을 삭제하는 것을 실패했습니다.',
	'212' : "이미 만료기한이 지난 메일입니다.",
	'401' : "유효하지 않은 영수증입니다.",
	'402' : "존재하지 않는 패키지입니다.",
	'403' : "유효하지 않은 패키지입니다.",
	'404' : "플레이어에게 아이템을 추가하는 것을 실패했습니다.",
	'405' : "메일 아이템을 되돌리는 것을 실패했습니다",
	'501' : "해당 아이템의 소유자가 아닙니다.",
	'502' : "해당 타입의 아이템을 강화할 수 없습니다.",
	'503' : "이미 강회횟수가 최대치에 도달했습니다.",
	'504' : "강화카운트가 존재하지 않습니다.",
	'505' : '강화 로깅을 남기는 것을 실패했습니다.',
	'506' : '존재하지 않는 아이템 타입 입니다.',
	'601' : "이미 출석을 했습니다",
	'602' : "출석을 실패했습니다.",
	'603' : "출석보상 전송이 실패했습니다.",
	'604' : "출석을 되돌리는 것이 실패했습니다."
	
}

func get_random_receipt_id():
	var rng = RandomNumberGenerator.new()
	return rng.randi_range(1, 999999999)

func add_username_in_header(username:String):
	headers.append("User-Name: {username}".format({
		"username" : username
	}))
	
	
func add_auth_token_in_header(auth_token:String):
	headers.append("Auth-Token: {auth_token}".format({
		"auth_token" : auth_token
	}))
	

func open_alert(msg):
	var alert_popup = load("res://src/ui/alert_popup.tscn").instantiate() 
	add_child(alert_popup)
	alert_popup.setup_alert("알림", msg)

'''
		None = 0,
		InvalidModel = 1,
		NoneExistName = 3,
		FailedConnectRedis = 4,
		FailedConnectMysql = 5,
		FailedConnectDatabase = 6,
		NoneExistItem = 7,

		/* Account 101 ~ 200 */
		InvalidPassword = 101,
		InvalidUserName = 102,
		FailedRegister = 103,
		FailedCreatePlayer = 104,
		FailedUndoRegisterUser = 105,
		AlreadyExistUser = 106,


		/* Mail 201 ~ 300 */
		InvalidPageNumber = 201,
		FailedFetchMail = 202,
		NoneExistMail = 203,
		AlreadyReadMail = 204,
		NoneHaveItemInMail = 205,
		AlreadyReceivedItemFromMail = 206,
		FailedAddMailItemToPlayer = 207,
		NoneOwnerThisMail = 208,
		FailedSendMail = 209,
		DeletedMail = 210,
		FailedDeleteMail = 211,
		AlreadyMailDeadlineExpireDate = 212,

		/* NOtice 301 ~ 400 */


		/* Payment 401 ~ 500*/
		InvalidReceipt = 401,
		NoneExistPackgeId = 402,
		InvalidPackage = 403,
		FailedAddItemToPlayer = 404,
		FailedUndoMailItem = 405,

		/* Enchant 501~600 */
		IsNotOwnerThisItem = 501,
		CantNotEnchantThisType = 502,
		AlreadyMaxiumEnchantCount = 503,
		NoneExistEnchantCount = 504,
		FailedLogEnchant = 505,
		NoneExistItemType = 506,


		/* Attendance 601 ~ 700 */
		AlreadyAttendance = 601,
		FailedAttendance = 602,
		FailedSendAttendanceReward = 603,
		FailedUndoAttendance = 604,
'''
