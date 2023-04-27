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
        //Task<Mailbox> ReadMail(int mailId); 
        //Task<Item> GetItemFromMail();
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
                var mails =  await queryFactory.Query("mailbox").Where("receiverId", userId).GetAsync<Mailbox>();
                return mails.ToArray();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        //public async Task<Mailbox> ReadMail(int mailId)
        //{

        //}
    }
}
