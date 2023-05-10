﻿using Microsoft.Extensions.Options;
using RpgCollector.Models;
using RpgCollector.RequestResponseModel.NoticeGetModel;
using RpgCollector.RequestResponseModel;
using RpgCollector.Utility;
using StackExchange.Redis;
using System.Text.Json;
using ZLogger;
using RpgCollector.Models.NoticeModel;

namespace RpgCollector.Services;

public interface INoticeMemoryDB
{
    Task<Notice[]?> GetAllNotice();
}

public class NoticeMemoryDB : INoticeMemoryDB
{

    private ConnectionMultiplexer? redisClient;
    private IDatabase redisDB;
    IOptions<DbConfig> _dbConfig;
    ILogger<NoticeMemoryDB> _logger;

    public NoticeMemoryDB(IOptions<DbConfig> dbConfig, ILogger<NoticeMemoryDB> logger) 
    {
        _dbConfig = dbConfig;
        _logger = logger;
        Open();
    }

    public async Task<Notice[]?> GetAllNotice()
    {
        if (redisClient == null)
        {
            return null;
        }

        IDatabase redisDB = redisClient.GetDatabase();

        // Redis에 공지사항이 저장되어 있다면
        if (await redisDB.KeyExistsAsync("Notices"))
        {
            RedisValue[] noticesRedis;
            try
            {
                noticesRedis = await redisDB.ListRangeAsync("Notices");
            }
            catch (Exception ex)
            {
                _logger.ZLogError(ex.Message);
                return null;
            }

            Notice[] noticesArray = new Notice[noticesRedis.Length];

            for (int i = 0; i < noticesRedis.Length; i++)
            {
                noticesArray[i] = JsonSerializer.Deserialize<Notice>(noticesRedis[i]);
            }

            NoticeGetResponse noticeResponse = new NoticeGetResponse
            {
                Error = ErrorCode.None,
                NoticeList = noticesArray
            };

            return noticesArray;
        }
        return null;
    }

    void Dispose()
    {
        try
        {
            redisClient.Dispose();
        }
        catch (Exception ex)
        {
            _logger.ZLogError(ex.Message);
        }
    }

    void Open()
    {
        ConfigurationOptions option = new ConfigurationOptions
        {
            EndPoints = { _dbConfig.Value.RedisDb }
        };
        try
        {
            redisClient = ConnectionMultiplexer.Connect(option);
            redisDB = redisClient.GetDatabase();
        }
        catch (Exception ex)
        {
            _logger.ZLogError(ex.Message);
        }
    }
}
