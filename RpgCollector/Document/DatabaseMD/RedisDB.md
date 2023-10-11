## RedisDB 

### Stage Redis 
```
public class RedisStageItem
{
    public int ItemId { get; set; }
    public int FarmingCount { get; set; }
    public int MaxCount { get; set; }
}

public class RedisStageNpc
{
    public int NpcId { get; set; }
    public int Count { get; set; }
    public int RemaingCount { get; set; }
    public int Exp { get; set; }
}


public class RedisPlayerStageInfo
{
    public int UserId { get; set; }
    public int StageId { get; set; }
    public RedisStageNpc[] Npcs { get; set; }
    public RedisStageItem[] FarmingItems { get; set; }
    public int RewardExp { get; set; }
}
```

### User Auth 
```
public enum UserState
{
    Login = 1, 
    Playing = 2,
}
public class RedisUser
{
    public int UserId { get; set; }
    public string AuthToken { get; set; }
    public UserState State { get; set; }
}
```

### Version 
```
public class GameVersion
{
    public string ClientVersion { get; set; }
    public string MasterVersion { get; set; }
}
```

### Chat User & Lobby 
```
public class Chat - LIST
{
    public string UserName { get; set; }
    public int UserId { get; set; }
    public string Content { get; set; }
    public Int64 TimeStamp { get; set; }
}

public class ChatUser - SETS
{
    public int LobbyId { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; }
}
```

### Notice 
```
public class Notice
{
    public int NoticeId { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public int UploaderId { get; set; }
}
```