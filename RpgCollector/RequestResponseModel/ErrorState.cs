namespace RpgCollector.RequestResponseModel
{
    public enum ErrorState
    {
        None = 0,
        InvalidModel = 1,
        InvalidPassword = 2,
        InvalidUserName = 3,
        NoneExistName = 4,
        FailedConnectRedis = 5,
        FailedConnectMysql = 6,
        FailedConnectDatabase = 7,
        FailedRegister = 8,
        FailedCreatePlayer = 9,
        FailedUndoRegisterUser = 10,
        InvalidPageNumber = 11,
        FailedFetchMail = 12,
        NoneExistMail = 13,
        AlreadyReadMail = 14,
        NoneHaveItemInMail = 15,
        AlreadyReceivedItemFromMail = 16,
        FailedAddMailItemToPlayer = 17,
        NoneOwnerThisMail = 18,
        InvalidReceipt = 19,
        NoneExistPackgeId = 20,
        FailedSendMail = 21,
        InvalidPackage = 22,
    }
}
