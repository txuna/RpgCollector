namespace RpgCollector.Models.ChatModel
{
    public class Chat
    {
        public string UserName { get; set; }
        public string UserId { get; set; }
        public string Content { get; set; }
        public string Date { get; set; }
    }

    public class RedisChat
    {
        public int LobbyId { get; set; }
        public Chat[] ChatList { get; set; }
        public int[] UserList { get; set; }
    }
}
