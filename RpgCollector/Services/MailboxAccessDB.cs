using Microsoft.Extensions.Options;
using RpgCollector.Models;
using SqlKata.Compilers;
using SqlKata.Execution;
using System.Data;

namespace RpgCollector.Services
{
    public interface IMailboxAccessDB
    {
        Task<Mailbox[]?> GetAllMailFromUserId(int userId);
        Task<Mailbox?> GetMail(int mailId);
        Task<bool> ReadMail(int mailId);
        Task<MailItem?> ReceiveMailItem(int mailId);
        Task<bool> HasMailItem(int mailId);
        Task<bool> UndoMailItem(int mailId);
    }

    public class MailboxAccessDB : IMailboxAccessDB
    {
        IDbConnection dbConnection;
        MySqlCompiler compiler;
        QueryFactory queryFactory;

        public MailboxAccessDB(IOptions<DbConfig> dbConfig) 
        {
            dbConnection = DatabaseConnector.OpenMysql(dbConfig.Value.MysqlGameDb);
            if (dbConnection != null)
            {
                compiler = new MySqlCompiler();
                queryFactory = new QueryFactory(dbConnection, compiler);
            }
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
                return null;
            }
        }

        public async Task<Mailbox?> GetMail(int mailId)
        {
            try
            {
                Mailbox mail = await queryFactory.Query("mailbox").Where("mailId", mailId).FirstAsync<Mailbox>();
                return mail;
            }
            catch( Exception ex)
            {
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
                return false;
            }
        }

        public async Task<MailItem?> ReceiveMailItem(int mailId)
        {
            try
            {
                MailItem? mailItem = await queryFactory.Query("mail_item").Where("mailId", mailId).FirstAsync<MailItem>();
                int effectedRow = await queryFactory.Query("mail_Item").Where("mailId", mailId).Where("hasReceived", 0).UpdateAsync(new
                {
                    hasReceived = 1
                }); 
                if(effectedRow < 1)
                {
                    return null;
                }
                return mailItem;
            }
            catch ( Exception ex)
            {
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
                return false;
            }
        }
    }
}
