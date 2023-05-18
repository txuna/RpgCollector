namespace RpgCollector.RequestResponseModel
{
    public enum ErrorCode
    {
        /* Common 0 ~ 100 */
        None = 0,
        InvalidModel = 1,
        NoneExistName = 3,
        FailedConnectRedis = 4,
        FailedConnectMysql = 5,
        FailedConnectDatabase = 6,
        NoneExistItem = 7,
        NonePermission = 8,
        FailedUpdateMoney = 9,
        FailedFetchPlayerItem = 10,
        InValidRequestHttpBody = 11,
        AuthTokenFailWrongKeyword = 12,
        CannotChangeUserState = 13,

        /* Account 101 ~ 200 */
        InvalidPassword = 101,
        InvalidUserName = 102,
        FailedRegister = 103,
        FailedCreatePlayer = 104,
        FailedUndoRegisterUser = 105,
        AlreadyExistUser = 106,


        /* Mail 201 ~ 300 */
        InvalidPageNumber = 201,
        FailedFetchMail = 202,
        NoneExistMail = 203,
        AlreadyReadMail = 204,
        NoneHaveItemInMail = 205,
        AlreadyReceivedItemFromMail = 206,
        FailedAddMailItemToPlayer = 207,
        NoneOwnerThisMail = 208,
        FailedSendMail = 209,
        DeletedMail = 210,
        FailedDeleteMail = 211,
        AlreadyMailDeadlineExpireDate = 212,
        CannotSetReceivedFlagInMail = 213,
        FailedReadMail = 214,
        FailedFetchMailItem = 215,

        /* NOtice 301 ~ 400 */


        /* Payment 401 ~ 500*/
        InvalidReceipt = 401,
        NoneExistPackgeId = 402,
        InvalidPackage = 403,
        FailedAddItemToPlayer = 404,
        FailedUndoMailItem = 405,
        FailedUndoPaymentLog = 406,
        FailedBuyPackage = 407,

        /* Enchant 501~600 */
        IsNotOwnerThisItem = 501,
        CantNotEnchantThisType = 502,
        AlreadyMaxiumEnchantCount = 503,
        NoneExistEnchantCount = 504,
        FailedLogEnchant = 505,
        NoneExistItemType = 506,
        NotEnoughMoney = 507,
        FailedFetchMoney = 508,
        


        /* Attendance 601 ~ 700 */
        AlreadyAttendance = 601,
        FailedAttendance = 602,
        FailedSendAttendanceReward = 603,
        FailedUndoAttendance = 604,


        /* DungeonStage 701 ~ 800 */
        FailedFetchStageInfo = 701,
        NeedClearPreconditionStage = 702,
        RedisErrorCannotEnterStage = 703,
        FailedLoadStageNpc = 704, 
        FaiedLoadStageItem = 705,
        AlreadyPlayStage = 706,
        NotPlayingStage = 707, 
        NoneExistNpcInStage = 708,
        FailedProcessHuntingNpc = 709,
        FailedProcessFarmingItem = 710,
        NoneClearStage = 711,
        FailedSendStageReward = 712,
        FailedSetNextStage = 713,
        FailedSetStageRewardExp = 714,
        FailedRemoveStageInfoInMemory = 715,
        FailedSetPlayerInfoInRedis = 716,
        NoneExistStageId = 717,

        /* CHAT */ 
        FailedSendChat = 801,
        FailedJoinRoom = 802,
        FailedLoadLobbyUser = 803,
        FailedFindUser = 804,
        FailedLoadChat = 805,
    }
}
