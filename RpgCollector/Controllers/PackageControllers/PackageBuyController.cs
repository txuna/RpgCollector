using Microsoft.AspNetCore.Mvc;
using RpgCollector.RequestResponseModel.PaymentModel;
using RpgCollector.RequestResponseModel;
using RpgCollector.Services;
using RpgCollector.Models.PackgeItemData;
using RpgCollector.Models;

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
     * 패키지가 존재하는지 확인 
     * 구매가 정상적으로 이뤄진다면 메일로 발송
     */
    [Route("/Package/Buy")]
    [HttpPost]
    public async Task<PackageBuyResponse> BuyPackage(PackageBuyRequest packageBuyRequest)
    {
        int userId;
        ErrorState Error = await Verify(packageBuyRequest); 

        if(Error != ErrorState.None)
        {
            return new PackageBuyResponse
            {
                Error = Error
            };
        }

        (Error, userId) = await Buy(packageBuyRequest);
        if(Error != ErrorState.None)
        {
            return new PackageBuyResponse
            {
                Error = Error
            };
        }

        Error = await SendPackageToMail(packageBuyRequest, userId);

        return new PackageBuyResponse
        {
            Error = Error
        };
    }

    async Task<(ErrorState, int)> Buy(PackageBuyRequest packageBuyRequest)
    {
        string userName = HttpContext.Request.Headers["User-Name"];
        int userId = await _accountDB.GetUserId(userName);

        if(userId == -1)
        {
            return (ErrorState.NoneExistName, -1);
        }

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
