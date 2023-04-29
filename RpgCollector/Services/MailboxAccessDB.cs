using Microsoft.Extensions.Options;
using MySqlConnector;
using RpgCollector.Models;
using RpgCollector.Models.MailData;
using RpgCollector.Utility;
using SqlKata.Compilers;
using SqlKata.Execution;
using System.Data;

namespace RpgCollector.Services
{
    public interface IMailboxAccessDB
    {
        Task<Mailbox[]?> GetAllMailFromUserId(int userId);
        Task<Mailbox?> GetMailFromUserId(int mailId, int userId);
        Task<bool> ReadMail(int mailId);
        Task<MailItem?> ReceiveMailItem(int mailId);
        Task<bool> HasMailItem(int mailId);
        Task<bool> SendMail(int senderId, int receiverId, string title, string content); //일반 메일
        Task<bool> SendMail(int senderId, int receiverId, string title, string content, int itemId, int quantity); //아이템 동봉 메일
        // player의 아이템 받기가실패할시 mail undo 처리 
        Task<bool> UndoMailItem(int mailId);
        Task<bool> IsMailOwner(int mailId, int userId);
    }

    public class MailboxAccessDB : IMailboxAccessDB
    {
        IDbConnection dbConnection;
        MySqlCompiler compiler;
        QueryFactory queryFactory;
        IOptions<DbConfig> _dbConfig;

        public MailboxAccessDB(IOptions<DbConfig> dbConfig) 
        {
            _dbConfig = dbConfig;
            Open();
        }

        public async Task<bool> IsMailOwner(int mailId, int userId)
        {
            try
            {
                int count = await queryFactory.Query("mailbox").Where("mailId", mailId).Where("receiverId", userId).CountAsync<int>();
                if(count > 0)
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public async Task<bool> SendMail(int senderId, int receiverId, string title, string content, int itemId, int quantity)
        {
            try
            {
                int mailId = await queryFactory.Query("mailbox").InsertGetIdAsync<int>(new
                {
                    senderId = senderId,
                    receiverId = receiverId,
                    title = title,
                    content = content,
                    isRead = 0,
                    hasItem = 1
                });

                await queryFactory.Query("mail_item").InsertAsync(new
                {
                    mailId = mailId,
                    itemId = itemId,
                    quantity = quantity,
                    hasReceived = 0
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            return true;
        }

        public async Task<bool> SendMail(int senderId, int receiverId, string title, string content)
        {
            try {
                await queryFactory.Query("mailbox").InsertAsync(new
                {
                    senderId = senderId,
                    receiverId = receiverId,
                    title = title,
                    content = content,
                    isRead = 0,
                    hasItem = 0
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            return true;
        }

        public async Task<Mailbox[]?> GetAllMailFromUserId(int userId)
        {
            try
            {
                IEnumerable<Mailbox> mails =  await queryFactory.Query("mailbox").Where("receiverId", userId).GetAsync<Mailbox>();
                return mails.ToArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public async Task<Mailbox?> GetMailFromUserId(int mailId, int userId)
        {
            try
            {
                Mailbox mail = await queryFactory.Query("mailbox")
                                                 .Where("mailId", mailId)
                                                 .Where("receiverId", userId)
                                                 .FirstAsync<Mailbox>();
                return mail;
            }
            catch( Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public async Task<bool> ReadMail(int mailId)
        {
            try
            {
                int effectedRow = await queryFactory.Query("mailbox").Where("mailId", mailId).Where("isRead", 0).UpdateAsync(new {
                    isRead = 1
                });
                // 이미 읽은 경우
                if(effectedRow < 1)
                {
                    return false;
                }
                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        // 메일 아이템은 수령했지만 사용자 아이템에 정상적으로 들어가지 못한다면 해당 메일 읽기를 롤백한다. 
        public async Task<bool> UndoMailItem(int mailId)
        {
            try
            {
                int effectedRow = await queryFactory.Query("mail_Item").Where("mailId", mailId).Where("hasReceived", 1).UpdateAsync(new
                {
                    hasReceived = 0
                });
                if(effectedRow < 1)
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public async Task<MailItem?> ReceiveMailItem(int mailId)
        {
            try
            {
                MailItem? mailItem = await queryFactory.Query("mail_item").Where("mailId", mailId).FirstAsync<MailItem>();
                int effectedRow = await queryFactory.Query("mail_Item")
                                                    .Where("mailId", mailId)
                                                    .Where("hasReceived", 0)
                                                    .UpdateAsync(new {
                    hasReceived = 1,
                });
                if(effectedRow < 1)
                {
                    return null;
                }
                return mailItem;
            }
            catch ( Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public async Task<bool> HasMailItem(int mailId)
        {
            try
            {
                bool success = await queryFactory.Query("mail_item").Where("mailId", mailId).ExistsAsync();
                return success;
            }
            catch ( Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        void Dispose()
        {
            try
            {
                dbConnection.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        void Open()
        {
            try
            {
                dbConnection = new MySqlConnection(_dbConfig.Value.MysqlGameDb);
                dbConnection.Open();
                compiler = new MySqlCompiler();
                queryFactory = new QueryFactory(dbConnection, compiler);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
