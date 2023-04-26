using Microsoft.Extensions.Options;
using RpgCollector.Models;

namespace RpgCollector.Services
{
    public interface INoticeService
    {
        Task<(bool success, string content)> GetAllNotice();
    }

    public class NoticeService : INoticeService
    {
        private IOptions<DbConfig> _dbConfig; 

        public NoticeService(IOptions<DbConfig> dbConfig)
        {
            _dbConfig = dbConfig;
        }

        public async Task<(bool success, string content)> GetAllNotice()
        {
            return (true, "HELLO WORLD");
        }

    }
}
