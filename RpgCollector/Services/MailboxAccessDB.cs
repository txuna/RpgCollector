using Microsoft.Extensions.Options;
using MySqlConnector;
using RpgCollector.Models;
using RpgCollector.Models.MailModel;
using RpgCollector.Utility;
using SqlKata.Compilers;
using SqlKata.Execution;
using System.Data;
using ZLogger;

namespace RpgCollector.Services;

public interface IMailboxAccessDB
{
    Task<int> GetTotalMailNumber(int receiverId);
    Task<MailItem?> GetMailItem(int mailId);
    Task<Mailbox?> GetMailFromUserId(int mailId, int userId);
    Task<bool> ReadMail(int mailId);
    Task<MailItem?> ReceiveMailItem(int mailId);
    Task<bool> HasMailItem(int mailId);
    Task<bool> SendMail(int senderId, int receiverId, string title, string content); //일반 메일
    Task<bool> SendMail(int senderId, int receiverId, string title, string content, int itemId, int quantity); //아이템 동봉 메일
    // player의 아이템 받기가실패할시 mail undo 처리 
    Task<bool> UndoMailItem(int mailId);
    Task<bool> IsMailOwner(int mailId, int userId);
    Task<Mailbox[]?> GetPartialMails(int userId, bool isFirst, int pageNumber);
    Task<bool> IsDeletedMail(int mailId);
    Task<bool> DeleteMail(int mailId);
    Task<bool> IsDeadLine(int mailId);
}

public class MailboxAccessDB : IMailboxAccessDB
{
    IDbConnection dbConnection;
    MySqlCompiler compiler;
    QueryFactory queryFactory;
    IOptions<DbConfig> _dbConfig;
    ILogger<MailboxAccessDB> _logger;
    DateTime deadLine;

    public MailboxAccessDB(IOptions<DbConfig> dbConfig, ILogger<MailboxAccessDB> logger) 
    {
        _dbConfig = dbConfig;
        _logger = logger;
        deadLine = DateTime.Now.AddDays(-30);
        Open();
    }
    
    /* 유효기간 확인 */
    public async Task<Mailbox[]?> GetPartialMails(int receiverId, bool isFirst, int pageNumber)
    {
        try
        {
            if(isFirst)
            {
                IEnumerable<Mailbox> mails = await queryFactory.Query("mailbox")
                                                               .Where("receiverId", receiverId)
                                                               //.WhereNot("isRead", 1)
                                                               .WhereNot("isDeleted", 1)
                                                               .Where("sendDate", ">=", deadLine)
                                                               .Take(20).GetAsync<Mailbox>();
                return mails.ToArray();
            }
            else
            {
                if(pageNumber <= 0)
                {
                    return null;
                }
                int mailCount = await queryFactory.Query("mailbox")
                                                  .Where("receiverId", receiverId)
                                                  //.WhereNot("isRead", 1)
                                                  .WhereNot("isDeleted", 1)
                                                  .Where("sendDate", ">=", deadLine)
                                                  .CountAsync<int>();
                Console.WriteLine(mailCount);
                if ((pageNumber - 1) * 20 > mailCount)
                {
                    Console.WriteLine(mailCount);
                    return null;
                }
                int start = (pageNumber - 1) * 20;
                int end = start + 20;

                IEnumerable<Mailbox> mails = await queryFactory.Query("mailbox")
                                                               .Where("receiverId", receiverId)
                                                               //.WhereNot("isRead", 1)
                                                               .WhereNot("isDeleted", 1)
                                                               .Where("sendDate", ">=", deadLine)
                                                               .Skip(start)
                                                               .Take(end)
                                                               .GetAsync<Mailbox>();
                return mails.ToArray();
            }
        }
        catch (Exception ex)
        {
            _logger.ZLogError(ex.Message);
            return null;
        }
    }

    public async Task<int> GetTotalMailNumber(int receiverId)
    {
        try
        {
            int count = await queryFactory.Query("mailbox").Where("receiverId", receiverId).Where("sendDate", ">=", deadLine).Where("isDeleted", 0).CountAsync<int>();
            return count;
        }
        catch (Exception ex)
        {
            _logger.ZLogError(ex.Message);
            return 0;
        }
    }

    public async Task<bool> IsDeadLine(int mailId)
    {
        try
        {
            int count = await queryFactory.Query("mailbox").Where("mailId", mailId).Where("sendDate", "<", deadLine).CountAsync<int>();
            if(count > 0)
            {
                return true;
            }
            return false;
        }
        catch(Exception ex)
        {
            _logger.ZLogError(ex.Message);
            return false;
        }
    }

    public async Task<bool> DeleteMail(int mailId)
    {
        try
        {
            await queryFactory.Query("mailbox").Where("mailId", mailId).UpdateAsync(new
            {
                isDeleted = 1
            });
            return true;
        }
        catch (Exception ex)
        {
            _logger.ZLogError(ex.Message);
            return false;
        }
    }

    public async Task<bool> IsDeletedMail(int mailId)
    {
        try
        {
            int count = await queryFactory.Query("mailbox").Where("mailId", mailId).Where("isDeleted", 1).CountAsync<int>();
            if(count > 0)
            {
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.ZLogError(ex.Message);
            return false; 
        }
    }

    public async Task<bool> IsMailOwner(int mailId, int userId)
    {
        try
        {
            int count = await queryFactory.Query("mailbox").Where("mailId", mailId).Where("receiverId", userId).WhereNot("isDeleted", 1).CountAsync<int>();
            if(count > 0)
            {
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.ZLogError(ex.Message);
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
                hasItem = 1,
                isDeleted = 0,
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
            _logger.ZLogError(ex.Message);
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
                hasItem = 0,
                isDeleted = 0,
            });
        }
        catch (Exception ex)
        {
            _logger.ZLogError(ex.Message);
            return false;
        }
        return true;
    }

    public async Task<Mailbox?> GetMailFromUserId(int mailId, int userId)
    {
        try
        {
            Mailbox mail = await queryFactory.Query("mailbox")
                                             .Where("mailId", mailId)
                                             .Where("receiverId", userId)
                                             .WhereNot("isDeleted", 1)
                                             .FirstAsync<Mailbox>();
            return mail;
        }
        catch( Exception ex)
        {
            _logger.ZLogError(ex.Message);
            return null;
        }
    }

    /* 읽은 메일은 다시 읽을 수 있게 */
    public async Task<bool> ReadMail(int mailId)
    {
        try
        {
            int effectedRow = await queryFactory.Query("mailbox").Where("mailId", mailId).UpdateAsync(new {
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
            _logger.ZLogError(ex.Message);
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
            _logger.ZLogError(ex.Message);
            return false;
        }
    }

    public async Task<MailItem?> GetMailItem(int mailId)
    {
        try
        {
            MailItem? mailItem = await queryFactory.Query("mail_item").Where("mailId", mailId).FirstAsync<MailItem>();
            return mailItem;
        }
        catch (Exception ex)
        {
            _logger.ZLogError(ex.Message); 
            return null;
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
            _logger.ZLogError(ex.Message);
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
            _logger.ZLogError(ex.Message);
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
            _logger.ZLogError(ex.Message);
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
            _logger.ZLogError(ex.Message);
        }
    }
}
