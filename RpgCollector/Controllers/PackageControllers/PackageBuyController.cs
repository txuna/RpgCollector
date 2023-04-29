using Microsoft.AspNetCore.Mvc;
using RpgCollector.RequestResponseModel.PaymentModel;
using RpgCollector.RequestResponseModel;
using RpgCollector.Services;
using RpgCollector.Models.PackgeItemData;

namespace RpgCollector.Controllers.PackageControllers;

[ApiController]
public class PackageBuyController : Controller
{
    IPackagePaymentDB _packagePaymentDB; 
    IAccountDB _accountDB;
    IMailboxAccessDB _mailboxAccessDB;
    public PackageBuyController(IPackagePaymentDB packagePaymentDB, IAccountDB accountDB, IMailboxAccessDB mailboxAccessDB) 
    {
        _packagePaymentDB = packagePaymentDB;
        _accountDB = accountDB;
        _mailboxAccessDB = mailboxAccessDB;
    }
    /*
     * 클라이언트가 어떤 패키지를 샀는지 확인 및 보낸 영수증 ID 중복 검증 
     */
    [Route("/Package/Buy")]
    [HttpPost]
    public async Task<PackageBuyResponse> BuyPackage(PackageBuyRequest packageBuyRequest)
    {
        if(!await _packagePaymentDB.VerifyReceipt(packageBuyRequest.ReceiptId))
        {
            return new PackageBuyResponse
            {
                Error = ErrorState.InvalidReceipt
            };
        }

        if(!await _packagePaymentDB.VertifyPackageId(packageBuyRequest.PackageId))
        {
            return new PackageBuyResponse
            {
                Error = ErrorState.InvalidPackage
            };
        }

        string userName = HttpContext.Request.Headers["User-Name"];
        int userId = await _accountDB.GetUserId(userName);
        if(userId == -1)
        {
            return new PackageBuyResponse
            {
                Error = ErrorState.NoneExistName
            };
        }

        if(!await _packagePaymentDB.BuyPackage(packageBuyRequest.ReceiptId, packageBuyRequest.PackageId, userId))
        {
            return new PackageBuyResponse
            {
                Error = ErrorState.FailedConnectMysql
            };
        }

        PackageItem[]? packageItems = await _packagePaymentDB.GetPackageItems(packageBuyRequest.PackageId);
        if(packageItems == null)
        {
            return new PackageBuyResponse
            {
                Error = ErrorState.NoneExistPackgeId
            };
        }

        foreach(PackageItem item in packageItems)
        {
            if(!await _mailboxAccessDB.SendMail(1, 
                                                userId, 
                                                "Packge Item!", 
                                                "A package item has arrived. Thanks for your purchase",
                                                item.ItemId, 
                                                item.Quantity))
            {
                return new PackageBuyResponse
                {
                    Error = ErrorState.FailedSendMail
                };
            }
        }

        return new PackageBuyResponse
        {
            Error = ErrorState.None
        };
    }

}
