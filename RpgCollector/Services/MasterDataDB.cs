﻿using Microsoft.Extensions.Options;
using MySqlConnector;
using RpgCollector.Models;
using RpgCollector.Models.AttendanceData;
using RpgCollector.Models.EnchantModel;
using RpgCollector.Models.InitPlayerModel;
using RpgCollector.Models.MasterModel;
using RpgCollector.Models.PackageItemModel;
using SqlKata.Compilers;
using SqlKata.Execution;
using System.Data;
using ZLogger;

namespace RpgCollector.Services;

public interface IMasterDataDB
{
    public InitPlayerState GetInitPlayerState();
    public InitPlayerItem[] GetInitPlayerItems();
    public MasterAttendanceReward? GetMasterAttendanceReward(int dayCount);
    public MasterEnchantInfo? GetMasterEnchantInfo(int enchantCount);
    public MasterItem? GetMasterItem(int itemId);
    public MasterItemAttribute? GetMasterItemAttribute(int attributeId);
    public MasterItemType? GetMasterItemType(int typeId);
    public MasterPackage[] GetMasterPackage(int packageId);
    public MasterPlayerState? GetMasterPlayerState(int level);
}

public class MasterDataDB : IMasterDataDB
{
    InitPlayerState initPlayerState;
    InitPlayerItem[] initPlayerItem;
    MasterPlayerState[] masterPlayerState;
    MasterAttendanceReward[] masterAttendanceReward;
    MasterEnchantInfo[] masterEnchantInfo;
    MasterItem[] masterItem;
    MasterItemAttribute[] masterItemAttribute;
    MasterItemType[] masterItemType;
    MasterPackage[] masterPackage;

    IOptions<DbConfig> _dbConfig;
    IDbConnection dbConnection;
    MySqlCompiler compiler;
    QueryFactory queryFactory;
    ILogger<MasterDataDB> _logger;
    public MasterDataDB(IOptions<DbConfig> dbConfig, ILogger<MasterDataDB> logger)
    {
        _dbConfig = dbConfig;
        _logger = logger;
        Open();
        Load();
    }
    public InitPlayerState GetInitPlayerState()
    {
        return initPlayerState;
    }
    public InitPlayerItem[] GetInitPlayerItems()
    {
        return initPlayerItem;
    }
    public MasterAttendanceReward? GetMasterAttendanceReward(int dayCount)
    {
        return masterAttendanceReward.FirstOrDefault(e => e.DayId == dayCount);
    }
    public MasterEnchantInfo? GetMasterEnchantInfo(int enchantCount)
    {
        return masterEnchantInfo.FirstOrDefault(e => e.EnchantCount == enchantCount);
    }
    public MasterItem? GetMasterItem(int itemId)
    {
        return masterItem.FirstOrDefault(e => e.ItemId == itemId);
    }
    public MasterItemAttribute? GetMasterItemAttribute(int attributeId)
    {
        return masterItemAttribute.FirstOrDefault( e => e.AttributeId == attributeId);
    }
    public MasterItemType? GetMasterItemType(int typeId)
    {
        return masterItemType.FirstOrDefault( e=> e.TypeId == typeId);  
    }
    public MasterPackage[] GetMasterPackage(int packageId)
    {
        return masterPackage.Where(e => e.PackageId == packageId).ToArray(); // 조건에 없으면 빈 배열
    }

    public MasterPlayerState? GetMasterPlayerState(int level)
    {
        return masterPlayerState.FirstOrDefault( e => e.Level == level);
    }

    void Load()
    {
        try
        {
            //플레이어 초기 스탯 가지고 오기 
            initPlayerState = queryFactory.Query("init_player_state").First<InitPlayerState>();

            //플레이어 초기 아이템 가지고 오기
            initPlayerItem = (queryFactory.Query("init_player_items").Get<InitPlayerItem>()).ToArray();

            //마스터 출석 보상 가지고 오기 
            masterAttendanceReward = (queryFactory.Query("master_attendance_reward").Get<MasterAttendanceReward>()).ToArray();

            //마스터 강화 정보 가지고 오기
            masterEnchantInfo = (queryFactory.Query("master_enchant_info").Get<MasterEnchantInfo>()).ToArray();

            //마스터 아이템 가지고 오기 
            masterItem = (queryFactory.Query("master_item_info").Get<MasterItem>()).ToArray();

            //마스터 아이템 속성 가지고 오기
            masterItemAttribute = (queryFactory.Query("master_item_attribute").Get<MasterItemAttribute>()).ToArray();

            //마스터 아이템 타입 가지고 오기
            masterItemType = (queryFactory.Query("master_item_type").Get<MasterItemType>()).ToArray();

            //마스터 패키지 리스트 가지고 오기
            masterPackage = (queryFactory.Query("master_package_info").Get<MasterPackage>()).ToArray();

            //마스터 플레이어 스탯 가지고 오기 
            masterPlayerState = (queryFactory.Query("master_player_state").Get<MasterPlayerState>()).ToArray();
        }
        catch (Exception ex)
        {
            _logger.ZLogError("Failed Load Master Data");
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