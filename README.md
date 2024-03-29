
### 컴투스 서버캠퍼스 1기 
서머너즈워를 모작으로 수집형 RPG 멀티플레이어 게임입니다. 
구현된 기능으로는 아래와 같습니다. 

1. 로그인 
2. 회원가입 
3. 로그아웃 
4. 우편함 
5. 출석부 
6. 인앱결제 
7. 강화 
8. 공지 
9. 던전 스테이지 클리어 및 아이템 파밍 및 던전 이어하기 
10. 채팅
11. 상점

### 사용 기술 스택
1. ASP.NET Core 7.0 
2. Redis 
3. Mysql 

### ASP.NET Build 
1. Make Dockerfile 
```
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env
WORKDIR /App

# Copy everything
COPY . ./
# Restore as distinct layers
RUN dotnet restore
# Build and publish a release
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /App
COPY --from=build-env /App/out .
ENTRYPOINT ["dotnet", "[자신의 프로젝트이름].dll"]
```

2. Create Docker Image
```
docker build -t [이미지 이름] -f Dockerfile .
```

3. Create Docker Network 
```
docker network create --driver bridge server-net
```

4. Create Container Using Image
--network 옵션을주어 server-net이라는 네트워크에 묶는다. 
-> server-net 네트워크에 엮인 도커들은 서로 통신 가능
ex) ping AspNetServer
```
docker run -it -p 5000:80 --name [컨테이너 이름] [이미지 이름]
sudo docker run -it -p 56000:80 --network server-net --name AspNetServer [닷넷 이미지 파일]
```

### Trouble Shooting 
1. 호스트에 설정된 ufw는 호스트 내부에 돌아가는 Docker에는 적용되지 않는다. 
대안으로 RedisServer가 만약 로컬에서만 접근가능하게 하려면 포트 바인딩시 
127.0.0.1:10000:6379 이런식으로 옵션을 걸어주어 로컬에서만 접근을 가능하게 한다. 


## Images 
![Login](./Images/login.png)
![main](./Images/main.png)
![mailbox](./Images/mailbox.png)
![mail_read](./Images/read_mail.png)
![attendance](./Images/attendance.png)
![notices](./Images/notice.png)
![inventory](./Images/inventory.png)
![detail](./Images/detail.png)
![package](./Images/payment.png)
![enchant](./Images/enchant.png)
![worlds](./Images/worlds.png)
![enter_stage](./Images/enter_stage.png)
![new_stage](./Images/new_stage.png)
![continue](./Images/continue.png)
![chat](./Images/chat.png)
![shop](./Images/shop.png)

## API 목록
### Login API

1. username과 password를 입력받아 AccountDB의 users 테이블 검증
2. 검증된 유저에 대해 인증토큰 발급 및 redis에 해당 유저 useranem : token 형식 저장
3. redis에 유저상태가 이미 PLAYING이면서 게임스테이지 정보가 있다면 PLAYING으로 전송

**Database**

```csharp
AccountDB 
1. users - GET

Redis AccountDB 
1. UserName - RedisUser - INSERT
```

**Path**

```csharp
POST /Login
```

**Request**

```csharp
public class LoginRequest
{
    [Required]
    [LoginUserName]
    public string UserName { get; set; }

    [Required]
    [LoginUserPassword]
    public string Password { get; set; }

    [Required]
    public string ClientVersion { get; set; }
    [Required]
    public string MasterVersion { get; set; }
    [Required]
    public string AuthToken { get; set; }
}
```

**Response**

```csharp
public class LoginResponse
{
    public ErrorState Error { get; set; }
    public string UserName { get; set; }
    public string AuthToken { get; set; }
    public UserState State { get; set; }
}
```

### Register API

1. username과 password를 입력받아 디비서버 검증 
2. 유효한 요청이라면 AccountDB에 저장 및 GameDB에 players테이블에 userId를 참조하는 행 삽입 
3. GameDB의 init_player_items와 init_player_state의 테이블을 참고하여 초기 데이터 로드
4. 플레이어의 정보는 players 테이블에 저장, player item은 player_items에 저장 
5. 플레이어 기본 스테이지 정보 설정
6. 플레이어 생성 실패시 accountDB에서 해당 유저 정보 롤백(삭제)
7.  

**Database**

```csharp
AccountDB 
1. users - INSERT

GameDB 
1. init_player_items - GET 
2. init_player_state - GET 
3. player_items - INSERT
4. players - INSERT
5. player_stage_info - INSERT
```

**Path** 

```csharp
POST /Register
```

**Request** 

```csharp
public class RegisterRequest
{
    [Required]
    [RegisterUserName]
    public string UserName { get; set; }

    [Required]
    [RegisterUserPassword]
    public string Password { get; set; }

    [Required]
    public string ClientVersion { get; set; }
    [Required]
    public string MasterVersion { get; set; }
    [Required]
    public string AuthToken { get; set; }
}
```

**Response** 

```csharp
public class RegisterResponse
{
    public ErrorState Error {  get; set; }
}
```

### Logout API

1. 요청받은 헤더에 포함된 username을 기반으로 Redis에 저장된 유저정보 삭제

**Database** 

```csharp
1. Account Redis DB - DELETE
```

**Path**

```csharp
POST /Logout 
```

**Request** 

```csharp
public class LogoutRequest
{
    [Required]
    public string UserName { get; set; }
    [Required]
    public string ClientVersion { get; set; }
    [Required]
    public string MasterVersion { get; set; }
    [Required]
    public string AuthToken { get; set; }
}
```

**Response** 

```csharp
public class LogoutResponse
{
    public ErrorState Error { get; set; }
}
```

### Notice API

1. Redis에 저장된 모든 공지정보를 넘긴다. 

**Database** 

```csharp
Notice Redis DB - GET
```

**Path**

```csharp
POST /Notice
```

**Request**

```csharp
public class NoticeGetRequest
{
    [Required]
    public string UserName { get; set; }
    [Required]
    public string ClientVersion { get; set; }
    [Required]
    public string MasterVersion { get; set; }
    [Required]
    public string AuthToken { get; set; }
}

```

**Response**

```csharp
public class NoticeGetResponse
{
    public ErrorState Error { get; set; }
    public Notice[] NoticeList { get; set; }
}
```

### MailOpen API

1. 해당 우편함을 처음열었는가, 몇번 페이지를 볼것인가를 기반으로 삭제되지 않았거나 30일 기간이 지나지 않은 메일을  userId기반으로 불러와서 최대 20개 클라이언트에 전송

**Database** 

```csharp
mailbox - GET 
```

**Path** 

```csharp
POST /Mail/Open
```

**Request**

```csharp
public class MailOpenRequest
{
    [Required]
    public int? PageNumber { get; set; }
    [Required]
    public string UserName { get; set; }
    [Required]
    public string ClientVersion { get; set; }
    [Required]
    public string MasterVersion { get; set; }
    [Required]
    public string AuthToken { get; set; }
}
```

**Response**

```csharp
public class OpenMail
{
    public string Title { get; set; }
    public int MailId { get; set; }
}
public class MailOpenResponse
{
    public ErrorState Error { get; set; }
    public int TotalPageNumber { get; set; }
    public OpenMail[] Mails { get; set; }
}
```

### MailRead API

1. 열람하려는 메일에 대해서 삭제되었는지, 소유권자인지, 유효기간이 지났는지를 확인한다 
2. 문제가 없다면 userId를 기반으로 불러온다. 
3. 그리고 해당 MailId를 기반으로 데이터베이스에 읽음(isRead)표시를 진행한다. 

**Database** 

```csharp
mailbox - GET / UPDATE
```

**Path**

```csharp
POST /Mail/Read
```

**Request**

```csharp
public class MailReadRequest
{
    [Required]
    public int MailId { get; set; }
    [Required]
    public string UserName { get; set; }
    [Required]
    public string ClientVersion { get; set; }
    [Required]
    public string MasterVersion { get; set; }
    [Required]
    public string AuthToken { get; set; }
}
```

**Response**

```csharp
public class MailReadResponse
{
    public ErrorCode Error { get; set; }
    public int MailId { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public string SendDate { get; set; }
    public int ItemId { get; set; }
    public int Quantity { get; set; }
    public int HasReceived { get; set; }
}
```

### MailGetItem API

1. 열람하려는 메일이 유효기간을 지났는지, 삭제되었는지, 소유권이 누군지확인한다 
2. 해당 메일이 아이템이 동봉되어있는지 확인한다 
3. 해당 메일에 동봉된 아이템을 이미 수령하였는지또한 확인한다. 
4. 문제가 없다면 해당 MailId와 연결되어있는 Item을 Quantity에 맞게 플레이어에게 추가
5. 정상적으로 추가가 되었다면 hasReceived를 1로 둔다. 
6. 만약 플레이어에게 해당 메일 아이템 추가를 실패한다면 메일의 아이템은 수령하지 않은것으로 롤백처리한다. 

**Database**

```csharp
mailbox - GET / UPDATE
player_items - INSERT
```

**Path** 

```csharp
POST /Mail/Item
```

**Request**

```csharp
public class MailGetItemRequest
{
    [Required]
    public int MailId { get; set; }
    [Required]
    public string UserName { get; set; }
    [Required]
    public string ClientVersion { get; set; }
    [Required]
    public string MasterVersion { get; set; }
    [Required]
    public string AuthToken { get; set; }
}
```

**Response**

```csharp
public class MailGetItemResponse
{
    public ErrorState Error { get; set; }
}
```

### MailDelete API

1. 해당 메일이 유효기간이 지났는지, 소유권을 지니는지, 이미 삭제되었는지를 확인한다. 
2. 문제가 없다면 삭제를 진행한다. 이때 실제로 삭제를 하는것이 아닌 isDeleted 플래그를 1로 둔다. 

**Database** 

```csharp
mailbox - GET / UPDATE
```

**Path** 

```csharp
POST /Mail/Delete
```

**Request** 

```csharp
public class MailDeleteRequest
{
    [Required]
    public int MailId { get; set; }
    [Required]
    public string UserName { get; set; }
    [Required]
    public string ClientVersion { get; set; }
    [Required]
    public string MasterVersion { get; set; }
    [Required]
    public string AuthToken { get; set; }
}
```

**Response** 

```csharp
public class MailDeleteResponse
{
    public ErrorState Error { get; set; }
}
```

### Attendance API

1. 오늘날짜를 불러와서 오늘 출석 유무를 확인한다. 
2. 출석을 하지 않았다면 어제자 날짜를 가지고와 연석출석 유무를 확인한다. 
3. 연속출석유무는 아래의 로직과 같다 
    1. 어제자 출석로그가 없다면 연속출석은 1로 반환 
    2. 어제가 출석로그가 존재한다면 어제의 연속출석일수를 가지고와 +1을 한 후 반환
4. 연속출석일수를 인자로 출석을 진행한다.
5. 연속출석일수에 맞춰 보상을 플레이어의 메일로 전송한다. 

**Database** 

```csharp
mailbox - INSERT
master_attendance_reward - GET
master_item_info - GET
player_attendance_info - GET / INSERT
```

**Path** 

```csharp
POST /Attendance
```

**Request**

```csharp
public class AttendanceRequest
{
    [Required]
    public string UserName { get; set; }
    [Required]
    public string ClientVersion { get; set; }
    [Required]
    public string MasterVersion { get; set; }
    [Required]
    public string AuthToken { get; set; }
}
```

**Response**

```csharp
public class AttendanceResponse
{
    public ErrorState Error { get; set; }
}
```

### Enchant API

1. 요청받은 playerItemId가 userId의 소유권인지 존재하는지 확인한다. 
2. 요청받은 playerItemId를 기반으로 플레이어의 아이템을 디비에서 가지고온다. 
2. 해당 강화 비용을 확인하여 플레이어가 소지하고 있는 돈과 비교한다. 
3. 해당 플레이어 아이템의 ItemId를 기반으로 Master Item을 가지와 해당 아이템의 타입이 강화를 진행할 수 있는 타입인지 확인한다. 
4. 플레이어의 아이템에 포함된 현재 강화 단계와 Master Item의 최대 강화횟수와 비교하여 미만이라면 강화를 진행한다. 
5. 위의 모든 조건이 만족되면 강화를 진행한다. 강화시 강화정보 마스터 데이터를 참조하여 현재 강화단계별 퍼센트를 확인한다. 
6. 만약 강화에 실패한다면 해당 아이템은 삭제처리 한다.  

**Database**

```csharp
master_item_info - GET
master_item_attribute - GET 
master_enchant_info - GET
player_items - GET / UPDATE / DELETE
players - GET / UPDATE
```

**Path** 

```csharp
POST /Enchant
```

**Request**

```csharp
public class EnchantExecuteRequest
{
    [Required]
    public int PlayerItemId { get; set; }
    [Required]
    public string UserName { get; set; }
    [Required]
    public string ClientVersion { get; set; }
    [Required]
    public string MasterVersion { get; set; }
    [Required]
    public string AuthToken { get; set; }
}
```

**Response** 

```csharp
public class EnchantExecuteResponse
{
    public ErrorState Error { get; set; }
    public int Result { get; set; }
}
```

### Package API

1. 요청된 영수증ID와 패키지 ID의 유효성을 검사한다. 
    1. 중복되지 않은 영수증 ID인가 
    2. 존재하는 패키지 ID인가 
2. 위의 조건이 일치한다면 영수증 디비에 해당 요청을 기록한다. 
3. 패키지ID에 맞는 item과 quantity를 item이 포함된 메일을 각 각 전송한다. 

**Database** 

```csharp
master_package_info - GET 
player_payment_info - GET / INSERT
mailbox - INSERT
```

**Path** 

```csharp
POST /Package/Buy
```

**Request**

```csharp
public class PackageBuyRequest
{
    [Required]
    public int ReceiptId { get; set; }
    [Required]
    public int PackageId { get; set; }
    [Required]
    public string UserName { get; set; }
    [Required]
    public string ClientVersion { get; set; }
    [Required]
    public string MasterVersion { get; set; }
    [Required]
    public string AuthToken { get; set; }
}
```

**Response** 

```csharp
public class PackageBuyResponse
{
    public ErrorState Error { get; set; }
}
```

### MasterItemInfo API 
1. 요청받은 itemId의 master data를 넘겨줌 

**Database** 
```csharp
master_item_info - GET
```

**Path** 
```csharp
POST /Master/Item  
```

**Request**
```csharp
public class MasterItemGetInfoRequest
{
    public int ItemId { get; set; }
    [Required]
    public string UserName { get; set; }
    [Required]
    public string ClientVersion { get; set; }
    [Required]
    public string MasterVersion { get; set; }
    [Required]
    public string AuthToken { get; set; }
}
```

**Response**
```csharp
public class MasterItemGetInfoResponse
{
    public ErrorState Error { get; set; }
    public MasterItem MasterItem { get; set; }
    public string AttributeName { get; set; }
    public string TypeName { get; set; }
}
```

### MasterAttendanceReward API  
1. 출석보상 리스트를 반환함

**Database** 
```csharp
master_attendance_info- GET
```

**Path** 
```csharp
POST /Master/Attendance  
```

**Request**
```csharp 
public class MasterAttendanceInfoRequest
{
    [Required]
    public string UserName { get; set; }
    [Required]
    public string ClientVersion { get; set; }
    [Required]
    public string MasterVersion { get; set; }
    [Required]
    public string AuthToken { get; set; }
}
```

**Response**
```csharp
public class MasterAttendanceInfoResponse
{
    public ErrorState Error { get; set; }
    public MasterAttendanceReward[] AttendanceRewards { get; set; }
}
```

### AttendanceGetLog API 
1. userId기준 마지막 연속출석 날짜를 반환함

**Database** 
```csharp
player_attendance_info- GET
```

**Path** 
```csharp
POST /Attendance/Log  
```

**Request**
```csharp 
public class AtendanceGetLogRequest
{
    [Required]
    public string UserName { get; set; }
    [Required]
    public string ClientVersion { get; set; }
    [Required]
    public string MasterVersion { get; set; }
    [Required]
    public string AuthToken { get; set; }
}
```

**Response**
```csharp
public class AttendanceGetLogResponse
{
    public ErrorState Error { get; set; }
    public int SequenceDayCount { get; set; } 
}
```

### Player Items Get API
1. userId기준 소유한 아이템을 반환함

**Database** 
```csharp
player_items - GET
```

**Path** 
```csharp
POST /Inventory
```

**Request**
```csharp 
public class PlayerInventoryGetRequest
{
    [Required]
    public string UserName { get; set; }
    [Required]
    public string ClientVersion { get; set; }
    [Required]
    public string MasterVersion { get; set; }
    [Required]
    public string AuthToken { get; set; }
}
```

**Response**
```csharp
public class PlayerInventoryGetResponse
{
    public ErrorState Error { get; set; }
    public PlayerItem[]? Items { get; set; }
}
```

### Player Items Detail GET API
1. userId기준 소유한 아이템의 상세설명을 반환함

**Database** 
```csharp
player_items - GET
master_enchant_info - GET
```

**Path** 
```csharp
POST /Inventory/Item
```

**Request**
```csharp 
public class PlayerItemDetailGetRequest
{
    public int PlayerItemId { get; set; }
    [Required]
    public string UserName { get; set; }
    [Required]
    public string ClientVersion { get; set; }
    [Required]
    public string MasterVersion { get; set; }
    [Required]
    public string AuthToken { get; set; }
}
```

**Response**
```csharp
public class PlayerItemDetailGetResponse
{
    public ErrorCode Error { get; set; }
    public int ItemId { get; set; }
    public string ItemName { get; set; }
    public int BaseAttack { get; set; }
    public int BaseMagic { get; set; }
    public int BaseDefence { get; set; }
    public int PlusAttack { get; set; }
    public int PlusDefence { get; set; }
    public int PlusMagic { get; set; }
    public int EnchantCount { get; set; }
    public string AttributeName { get; set; }
    public string TypeName { get; set; }
}
```

### Package Show API
1. 인앱결제 상품 목록을 반환함

**Database** 
```csharp
master_package_payment
```

**Path** 
```csharp
POST /Package/Show
```

**Request**
```csharp 
public class PackageShowRequest
{
    [Required]
    public string UserName { get; set; }
    [Required]
    public string ClientVersion { get; set; }
    [Required]
    public string MasterVersion { get; set; }
    [Required]
    public string AuthToken { get; set; }
}
```

**Response**
```csharp
public class PackageShowResponse
{
    public ErrorState Error { get; set; }
    public MasterPackagePayment[] PackagePayment { get; set; }
}
```

### Master Enchant Info Get API
1. 요청받은 플레이어 아이템 ID기준으로 강화 능력치를 가지고 옴

**Database** 
```csharp
master_item_info - GET
master_enchant_info - GET
player_items - GET
```

**Path** 
```csharp
POST /Enchant/Info
```

**Request**
```csharp 
public class EnchantInfoGetRequest
{
    [Required]
    public int PlayerItemId { get; set; }
    [Required]
    public string UserName { get; set; }
    [Required]
    public string ClientVersion { get; set; }
    [Required]
    public string MasterVersion { get; set; }
    [Required]
    public string AuthToken { get; set; }
}
```

**Response**
```csharp
public class EnchantInfoGetResponse
{
    public ErrorState Error { get; set; }
    public int CurrentEnchantCount { get; set; }
    public int NextEnchantCount { get; set; }
    public int Percent { get; set; }
    public int IncreasementValue { get; set; }
    public int ItemId { get; set; }
    public int PlayerItemId { get; set; }
    public int Price { get; set; }
}
```

### Player State Get API
1. 현재 플레이어의 상태를 반환함

**Database** 
```csharp
master_player_state - GET
player - GET
```

**Path** 
```csharp
POST /Player/State
```

**Request**
```csharp 
public class PlayerStateGetRequest
{
    [Required]
    public string UserName { get; set; }
    [Required]
    public string ClientVersion { get; set; }
    [Required]
    public string MasterVersion { get; set; }
    [Required]
    public string AuthToken { get; set; }
}
```

**Response**
```csharp
public class PlayerStateGetResponse
{
    public ErrorState Error { get; set; }
    public PlayerState State { get; set; }
    public MasterPlayerState MasterState { get; set; }
}
```

### Player Stage Info API 
1. 현재 플레이어가 진행할 수 있는 스테이지를 반환함

**Database** 
```csharp
player_stage_info - GET
```

**Path** 
```csharp
POST /Stage/Info
```

**Request**
```csharp 
public class StageInfoGetRequest
{
    [Required]
    public string UserName { get; set; }
    [Required]
    public string ClientVersion { get; set; }
    [Required]
    public string MasterVersion { get; set; }
    [Required]
    public string AuthToken { get; set; }
}
```

**Response**
```csharp   
public class StageInfoGetResponse
{
    public ErrorCode Error { get; set; }
    public int CurStageId { get; set; }
}
```

### Player Stage Choice API
1. 플레이어가 선택한 스테이지에 대한 검증

**Database** 
```csharp
Redis DB - GET / INSERT
player_stage_info - GET
master_stage_npc - GET
master_stage_farming_item - GET
```

**Path** 
```csharp
POST /Stage/Choice
```

**Request**
```csharp 
public class StageChoiceRequest
{
    [Required]
    public string UserName { get; set; }
    [Required]
    public string ClientVersion { get; set; }
    [Required]
    public string MasterVersion { get; set; }
    [Required]
    public string AuthToken { get; set; }
    [Required]
    public int StageId { get; set; }
}
```

**Response**
```csharp   
public class StageItem
{
    public int ItemId { get; set; }
    public int Quantity { get; set; }
}

public class StageNpc
{
    public int NpcId { get; set; }
    public int Count { get; set; }
}

public class StageChoiceResponse
{
    public ErrorCode Error { get; set; }
    public StageItem[] Items { get; set; }
    public StageNpc[] Npcs { get; set; }
}
```

### Stage Hunting NPC API
1. 클라이언트가 NPC을 잡았다고 보내는 API

**Database** 
```csharp
Redis DB - GET / INSERT
```

**Path** 
```csharp
POST /Stage/Hunting/Npc
```

**Request**
```csharp 
public class StageHuntingNpcRequest
{
    [Required]
    public string UserName { get; set; }
    [Required]
    public string ClientVersion { get; set; }
    [Required]
    public string MasterVersion { get; set; }
    [Required]
    public string AuthToken { get; set; }
    [Required]
    public int NpcId { get; set; }
}
```

**Response**
```csharp   
public class StageHuntingNpcResponse
{
    public int NpcId { get; set; }
    public ErrorCode Error { get; set; }
}
```

### Stage Farming Item API
1. 클라이언트가 아이템을 파밍했다는 API 

**Database** 
```csharp
Redis DB - GET / INSERT
```

**Path** 
```csharp
POST /Stage/Farming/Item
```

**Request**
```csharp 
public class StageFarmingItemRequest
{
    [Required]
    public string UserName { get; set; }
    [Required]
    public string ClientVersion { get; set; }
    [Required]
    public string MasterVersion { get; set; }
    [Required]
    public string AuthToken { get; set; }
    [Required]
    public int ItemId { get; set; }
}
```

**Response**
```csharp   
public class StageFarmingItemResponse
{
    public int ItemId { get; set; }
    public ErrorCode Error { get; set; }
}
```

### Stage Clear API
1. 클라이언트가 해당 스테이지를 클리어했다는 API 
1. 아이템 보상 및 경험치 보상 진행

**Database** 
```csharp
Redis DB - GET / INSERT
master_player_state - GET
players - GET / UPDATE
palyer_stage_info / UPDATE
```

**Path** 
```csharp
POST /Stage/Farming/Item
```

**Request**
```csharp 
public class StageClearRequest
{
    [Required]
    public string UserName { get; set; }
    [Required]
    public string ClientVersion { get; set; }
    [Required]
    public string MasterVersion { get; set; }
    [Required]
    public string AuthToken { get; set; }
}
```

**Response**
```csharp   
public class StageClearRequest
{
    public ErrorCode Error { get; set; }
}
```

### Stage Continue API
1. 게임 진행 도중 끊겼을 시 다시 접속하여 던전 진행 정보를 받아옴
1. 던전 스테이지 클리어 과정에서 팅김 
2. 클라이언트는 다시 로그인 요청 
3. 서버는 로그인 요청시 기존 Redis에서 해당 userName를 key로 가지는 value중에 UserState가 Playing이였다면 토큰 갱신 및 클라이언트에게 게임진행중이였다 정보 전송
4. 받은 클라이언트는 로그인 화면에서 메인화면으로 넘어가는 과정에서 던전이 진행중이였음 플래그 전송 
5. 플래그를 받은 클라이언트는 서버에 진행중이였던 던전 내용을 받아서 화면으로 표시함

**Database** 
```csharp
Redis DB - GET / INSERT
```

**Path** 
```csharp
POST /Stage/Continue
```

**Request**
```csharp 
public class StagePlayingInfoLoadRequest
{
    [Required]
    public string UserName { get; set; }
    [Required]
    public string ClientVersion { get; set; }
    [Required]
    public string MasterVersion { get; set; }
    [Required]
    public string AuthToken { get; set; }
}
```

**Response**
```csharp   
public class StagePlayingInfoLoadResponse
{
    public ErrorCode Error { get; set; }
    public RedisStageItem[] Items { get; set; }
    public RedisStageNpc[] Npcs { get; set; }
    public int StageId { get; set; }
}
```

### Stage Exit API
1. 진행중인 던전을 끝내는 API 

**Database** 
```csharp
Redis DB - GET / DELETE
```

**Path** 
```csharp
POST /Stage/Exit
```

**Request**
```csharp 
public class StageExitRequest
{
    [Required]
    public string UserName { get; set; }
    [Required]
    public string ClientVersion { get; set; }
    [Required]
    public string MasterVersion { get; set; }
    [Required]
    public string AuthToken { get; set; }
}
```

**Response**
```csharp   
public class StageExitResponse
{
    public ErrorCode Error { get; set; }
}
```

### Chat Lobby Join API
1. 채팅 로비에 가입하는 API

**Database** 
```csharp
Redis DB - INSERT / GET
```

**Path** 
```csharp
POST /Chat/Join
```

**Request**
```csharp 
public class ChatJoinLobbyRequest
{
    [Required]
    public string UserName { get; set; }
    [Required]
    public string ClientVersion { get; set; }
    [Required]
    public string MasterVersion { get; set; }
    [Required]
    public string AuthToken { get; set; }
}
```

**Response**
```csharp   
public class StageExitResponse
{
    public ErrorCode Error { get; set; }
}
```

### Chat Load API
1. 클라이언트가 받은 마지막의 메시지의 TimeStamp의 값을 기반으로 그 이후의 메시지를 보냄 

**Database** 
```csharp
Redis DB - GET / INSERT
```

**Path** 
```csharp
POST /Chat/Load
```

**Request**
```csharp 
public class StageExitRequest
{
    [Required]
    public string UserName { get; set; }
    [Required]
    public string ClientVersion { get; set; }
    [Required]
    public string MasterVersion { get; set; }
    [Required]
    public string AuthToken { get; set; }
    [Required]
    public Int64 TimeStamp { get; set; }
}
```

**Response**
```csharp   
public class ChatLoadResponse
{
    public ErrorCode Error { get; set; }
    public Chat[] ChatLog { get; set; }
    public Int64 TimeStamp { get; set; }
}
```

### Chat Load API
1. 클라이언트가 보낸 채팅을 해당 클라이언트가 속한 로비에 넣음
이때 각 로비마다 리스트에 저장되는 채팅의 갯수는 50개이다. 
레디스에서 리스트를 가지고와서 50개인지 확인하고 넣는 작업은 2개의 명령어가 필요한데 이때 서로 다른 스레드가 아래와 같이 접근하면 문제가 발생한다. 
A라는 스레드에서 50인지 확인 
B라는 스레드에서 50인지 확인 
A라는 스레드에서 50이 아니기에 값을 넣음 
B라는 스레드에서 50이 아니기에 값을 넣음

비록 레디스가 싱글스레드로 동작하더라도 명령어 하나씩만 따로 처리하다보니 위와같은 상황이 벌여지는데 해결책으로 루아스크립트를 사용하면 된다. 
이 때 루아스크립트 하나를 하나의 명령어로 인식 

**Database** 
```csharp
Redis DB - GET / INSERT
```

**Path** 
```csharp
POST /Chat/Send
```

**Request**
```csharp 
public class ChatSendRequest
{
    [Required]
    public string UserName { get; set; }
    [Required]
    public string ClientVersion { get; set; }
    [Required]
    public string MasterVersion { get; set; }
    [Required]
    public string AuthToken { get; set; }
    [Required]
    public string Content { get; set; }
}
```

**Response**
```csharp   
public class ChatSendResponse
{
    public ErrorCode Error { get; set; }
}
```

### 2차 피드백  
2차 피드백  
[해결]1. 중복로그인 체크하지 말고 -> 새로운값으로 토큰 발급하고 갱신  + TTL 추가   
[해결]2. 컨트롤러마다 Request, Response 사용하기    
[해결]3. Response에 true false + message가 아닌 Error코드 정의해서 전송   
[해결]4. Document폴더만들어서 MD파일들 넣기    
[해결]5. 패스워드 유효성 검사는 로그인 컨트롤러에서    
[해결]6. Json으로 반환안해도 됨 -> 알아서 모델 매칭 해주는듯    
[해결]7. Redis에 User저장할때 해쉬로 X -> 간단하게 username - authToken - ttl 설정하기    
[해결]8. Insert한 row의 auto_increase한 값 가지고 오는법 찾기   
[해결]9. [FromBody] 에러뜨는 이유 알기    
-> [ApiController] : 모델 바인딩     
[해결]10. ModelState.Isvalid 없어도 되는지 확인    
[해결]11 계정은 만들어졌지만 CreatePlayer 실패했을 때 Undo기능 추가하기   
[해결]12. 에러 로깅   
[해결]13. SetupSaltAndHash 나누기    
[해결]14. Database 커넥터 각 파일에 그냥 넣기   
[보류]15. 토큰 유니코드 문제 바이트에서 스트링으로 해결하기   
[보류]16. Register시 try - catch 잘생각해보기 - 유저있는지 없는지 확인하는데 Error발산 좀 그럼    
-> Insert시 에러발생 말고 다른 메소드 있는지 찾아보기   
-> INSERT IGNORE INTO 를 사용하려고 만들어진 rawSQL을 수정해서 다시 컴파일하고 실행하려는 방법을 찾던 도중 실패   

### 3차 피드백 
[해결] 1. mailbox - mail_item 하나로 합치기    
[해결]2. enchant 이력이라는게 log를 남기라는 것이 아닌 count 이력임  테이블 삭제    
[해결]3. 패키지 구매가 아닌 구글플레이에서 사서 주는것이기에 이름 변경    
[해결]4. ReadMail -> 이름 변경 UpdateReadFlag    
[해결]5. log라는 이름 다 바꾸기 log가 아닌 Content임    
[해결]6. 디비 접근 최소화하기  
ex) verify (3개로 나누지 말기)  하나로 할 수 있으면 하기   
[해결]7. 디비 쿼리 결과값 (영향받은 row 값 확인하기) 다 확인하기    
[해결]8. 헤더 재고하기    
[해결]9. 아이템에 attack, defence, magic 컬럼 추가하기   
[해결]10. 메일 유효기간 넣기   
[해결]11. GetPartial -> 너무 구체적임    
[해결]12. 출석디비 최신값만 이력은 저장 X    
[해결]13. mail 열때 isFirst 필요 없음    
[해결]14. ErrorState -> ErrorCode로 변경   
[해결]15. ! -> == true 로 확인    
[미해결]16. 로깅은 구조화된 로깅으로   
[해결]17. playerItem 있고 masterItem 있을 경우 특정 위에서 실패하면 안가지고와도 되게 불필요한 디비 쿼리 자제   
[해결]18. clientVersion, masterDataVersion 클래스로 감싸기    
[해결]19. Redis CloudStructure 사용하기
