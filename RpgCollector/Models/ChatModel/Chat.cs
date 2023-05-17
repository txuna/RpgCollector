namespace RpgCollector.Models.ChatModel
{
    public class Chat
    {
        public string UserName { get; set; }
        public int UserId { get; set; }
        public string Content { get; set; }
        public Int64 TimeStamp { get; set; }
    }

    public class Lobby
    {
        public int LobbyId { get; set; }
        public int[] UserList { get; set; }
    }
}
