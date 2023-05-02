namespace RpgCollector.RequestResponseModel
{
    public enum ErrorState
    {
        /* Common 0 ~ 100 */
        None = 0,
        InvalidModel = 1,
        NoneExistName = 3,
        FailedConnectRedis = 4,
        FailedConnectMysql = 5,
        FailedConnectDatabase = 6,
        NoneExistItem = 7,

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

        /* NOtice 301 ~ 400 */


        /* Payment 401 ~ 500*/
        InvalidReceipt = 401,
        NoneExistPackgeId = 402,
        InvalidPackage = 403,
        FailedAddItemToPlayer = 404,
        FailedUndoMailItem = 405,

        /* Enchant 501~600 */
        IsNotOwnerThisItem = 501,
        CantNotEnchantThisType = 502,
        AlreadyMaxiumEnchantCount = 503,
        NoneExistEnchantCount = 504,
        FailedLogEnchant = 505,
        NoneExistItemType = 506,


        /* Attendance 601 ~ 700 */
        AlreadyAttendance = 601,
        FailedAttendance = 602,
        FailedSendAttendanceReward = 603,
        FailedUndoAttendance = 604,
    }
}
