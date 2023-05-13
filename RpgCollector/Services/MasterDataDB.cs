using Microsoft.Extensions.Options;
using MySqlConnector;
using RpgCollector.Models;
using RpgCollector.Models.AttendanceData;
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
    public MasterAttendanceReward[] GetAllMasterAttendanceReward();
    public MasterAttendanceReward? GetMasterAttendanceReward(int dayCount);
    public MasterEnchantInfo? GetMasterEnchantInfo(int enchantCount);
    public MasterItem? GetMasterItem(int itemId);
    public MasterItemAttribute? GetMasterItemAttribute(int attributeId);
    public MasterItemType? GetMasterItemType(int typeId);
    public MasterPackage[] GetMasterPackage(int packageId);
    public MasterPlayerState? GetMasterPlayerState(int level);
    public MasterPackagePayment[] GetPackagePayment();
    public MasterStageInfo[] GetMasterStageInfoList();
    public MasterStageInfo GetMasterStageInfo(int stageId);
    MasterStageNpc[] GetMasterStageNpcs(int stageId);
    MasterStageItem[] GetMasterStageItems(int stageId);
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
    MasterPackagePayment[] masterPackagePayment;
    MasterStageInfo[] masterStageInfo;

    MasterStageNpc[] masterStageNpc;
    MasterStageItem[] masterStageItem;

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
    public MasterStageItem[] GetMasterStageItems(int stageId)
    {
        return masterStageItem.Where(e => e.StageId == stageId).ToArray();
    }

    public MasterStageNpc[] GetMasterStageNpcs(int stageId)
    {
        return masterStageNpc.Where(e => e.StageId == stageId).ToArray();
    }
    //public StageItem[] GetMasterStageItems(int stageId)
    //{
    //    MasterStageItem[] items = masterStageItem.Where(e => e.StageId == stageId).ToArray();
    //    StageItem[] stageItem = new StageItem[items.Length];
    //    for (int i = 0; i < items.Length; i++)
    //    {
    //        stageItem[i] = new StageItem
    //        {
    //            ItemId = items[i].ItemId,
    //        };
    //    }
    //    return stageItem;
    //}

    //public StageNpc[] GetMasterStageNpcs(int stageId)
    //{
    //    MasterStageNpc[] npcs = masterStageNpc.Where(e => e.StageId == stageId).ToArray();
    //    StageNpc[] stageNpc = new StageNpc[npcs.Length];
    //    for (int i = 0; i < npcs.Length; i++)
    //    {
    //        stageNpc[i] = new StageNpc
    //        {
    //            NpcId = npcs[i].NpcId, 
    //            Count = npcs[i].Count
    //        };
    //    }
    //    return stageNpc;
    //}
    public MasterPackagePayment[] GetPackagePayment()
    {
        return masterPackagePayment;
    }
    public InitPlayerState GetInitPlayerState()
    {
        return initPlayerState;
    }
    public InitPlayerItem[] GetInitPlayerItems()
    {
        return initPlayerItem;
    }
    public MasterAttendanceReward[] GetAllMasterAttendanceReward()
    {
        return masterAttendanceReward;
    }
    public MasterAttendanceReward? GetMasterAttendanceReward(int dayCount)
    {
        return masterAttendanceReward.First(e => e.DayId == dayCount);
    }
    public MasterEnchantInfo? GetMasterEnchantInfo(int enchantCount)
    {
        return masterEnchantInfo.First(e => e.EnchantCount == enchantCount);
    }
    public MasterItem? GetMasterItem(int itemId)
    {
        return masterItem.First(e => e.ItemId == itemId);
    }
    public MasterItemAttribute? GetMasterItemAttribute(int attributeId)
    {
        return masterItemAttribute.First( e => e.AttributeId == attributeId);
    }
    public MasterItemType? GetMasterItemType(int typeId)
    {
        return masterItemType.First( e=> e.TypeId == typeId);  
    }
    public MasterPackage[] GetMasterPackage(int packageId)
    {
        return masterPackage.Where(e => e.PackageId == packageId).ToArray(); // 조건에 없으면 빈 배열
    }

    public MasterPlayerState? GetMasterPlayerState(int level)
    {
        return masterPlayerState.First( e => e.Level == level);
    }

    public MasterStageInfo[] GetMasterStageInfoList()
    {
        return masterStageInfo;
    }

    public MasterStageInfo GetMasterStageInfo(int stageId)
    {
        return masterStageInfo.First( e=> e.StageId == stageId);
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

            //인앱결제 상품 정보 불러오기 
            masterPackagePayment = (queryFactory.Query("master_package_payment").Get<MasterPackagePayment>()).ToArray();

            //스테이지 정보 가지고 오기
            masterStageInfo = (queryFactory.Query("master_stage_info").Get<MasterStageInfo>()).ToArray();

            //스테이지별 NPC 불러오기 
            masterStageNpc = (queryFactory.Query("master_stage_npc").Get<MasterStageNpc>()).ToArray();

            //스테이지별 아이템 불러오기 
            masterStageItem = (queryFactory.Query("master_stage_item").Get<MasterStageItem>()).ToArray();
        }
        catch (Exception ex)
        {
            _logger.ZLogError("Failed Load Master Data");
            _logger.ZLogError(ex.Message);
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