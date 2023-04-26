namespace RpgCollector.Services
{
    public interface IPlayerService
    {
        Task<(bool success, string content)> CreatePlayer(int userId);
    }
    public class PlayerService : IPlayerService
    {
        public async Task<(bool success, string content)> CreatePlayer(int userId)
        {
            return (true, "Created");
        }
    }
}
