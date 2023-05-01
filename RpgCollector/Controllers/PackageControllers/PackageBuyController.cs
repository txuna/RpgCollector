using Microsoft.AspNetCore.Mvc;
using RpgCollector.RequestResponseModel.PaymentModel;
using RpgCollector.RequestResponseModel;
using RpgCollector.Services;
using RpgCollector.Models.PackgeItemData;
using RpgCollector.Models;
using ZLogger;

namespace RpgCollector.Controllers.PackageControllers;

[ApiController]
public class PackageBuyController : Controller
{
    IPackagePaymentDB _packagePaymentDB; 
    IAccountDB _accountDB;
    IMailboxAccessDB _mailboxAccessDB;
    ILogger<PackageBuyController> _logger;

    public PackageBuyController(IPackagePaymentDB packagePaymentDB, IAccountDB accountDB, IMailboxAccessDB mailboxAccessDB, ILogger<PackageBuyController> logger) 
    {
        _packagePaymentDB = packagePaymentDB;
        _accountDB = accountDB;
        _mailboxAccessDB = mailboxAccessDB;
        _logger = logger;
    }
    /*
     * 클라이언트가 어떤 패키지를 샀는지 확인 및 보낸 영수증 ID 중복 검증 
     * 패키지가 존재하는지 확인 
     * 구매가 정상적으로 이뤄진다면 메일로 발송
     */
    [Route("/Package/Buy")]
    [HttpPost]
    public async Task<PackageBuyResponse> BuyPackage(PackageBuyRequest packageBuyRequest)
    {
        string userName = HttpContext.Request.Headers["User-Name"];
        int userId = await _accountDB.GetUserId(userName);

        _logger.ZLogInformation($"[{userId} {userName}] Request 'Buy Package'");

        if (userId == -1)
        {
            _logger.ZLogInformation($"[{userName}] None Exist Name");

            return new PackageBuyResponse
            {
                Error = ErrorState.NoneExistName
            };
        }

        ErrorState Error = await Verify(packageBuyRequest); 

        if(Error != ErrorState.None)
        {
            _logger.ZLogInformation($"[{userName}] Invalid PackageId : {packageBuyRequest.PackageId} or ReceiptId : {packageBuyRequest.ReceiptId}");

            return new PackageBuyResponse
            {
                Error = Error
            };
        }

        (Error, userId) = await Buy(packageBuyRequest, userId);

        if(Error != ErrorState.None)
        {
            _logger.ZLogInformation($"[{userId} {userName}] Cannot buy This PackageId : {packageBuyRequest.PackageId}");
            return new PackageBuyResponse
            {
                Error = Error
            };
        }

        Error = await SendPackageToMail(packageBuyRequest, userId);

        if(Error != ErrorState.None)
        {
            _logger.ZLogInformation($"[{userId} {userName}] Failed Send Mail This PackageId : {packageBuyRequest.PackageId} To Player");
        }

        _logger.ZLogInformation($"[{userId} {userName}] Success Send mail This PackageId : {packageBuyRequest.PackageId}");

        return new PackageBuyResponse
        {
            Error = Error
        };
    }

    async Task<(ErrorState, int)> Buy(PackageBuyRequest packageBuyRequest, int userId)
    {
        if(!await _packagePaymentDB.BuyPackage(packageBuyRequest.ReceiptId, packageBuyRequest.PackageId, userId))
        {
            return (ErrorState.FailedConnectDatabase, -1);
        }

        return (ErrorState.None, userId);
    }

    async Task<ErrorState> Verify(PackageBuyRequest packageBuyRequest)
    {
        if (!await _packagePaymentDB.VerifyReceipt(packageBuyRequest.ReceiptId))
        {
            return ErrorState.InvalidReceipt;
        }

        if (!await _packagePaymentDB.VertifyPackageId(packageBuyRequest.PackageId))
        {
            return ErrorState.InvalidPackage;
        }

        return ErrorState.None;
    }

    async Task<ErrorState> SendPackageToMail(PackageBuyRequest packageBuyRequest, int userId)
    {
        PackageItem[]? packageItems = await _packagePaymentDB.GetPackageItems(packageBuyRequest.PackageId);

        if (packageItems == null)
        {
            return ErrorState.NoneExistPackgeId;
        }

        foreach (PackageItem item in packageItems)
        {
            if (!await _mailboxAccessDB.SendMail(1,
                                                userId,
                                                "Packge Item!",
                                                "A package item has arrived. Thanks for your purchase",
                                                item.ItemId,
                                                item.Quantity))
            {
                return ErrorState.FailedSendMail;
            }
        }

        return ErrorState.None;
    }
}
