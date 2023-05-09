using Microsoft.AspNetCore.Mvc;
using RpgCollector.RequestResponseModel.PaymentModel;
using RpgCollector.RequestResponseModel;
using RpgCollector.Services;
using RpgCollector.Models;
using ZLogger;
using RpgCollector.Models.PackageItemModel;

namespace RpgCollector.Controllers.PackageControllers;

[ApiController]
public class PackageBuyController : Controller
{
    IPackagePaymentDB _packagePaymentDB; 
    IMailboxAccessDB _mailboxAccessDB;
    ILogger<PackageBuyController> _logger;
    IMasterDataDB _masterDataDB;

    public PackageBuyController(IPackagePaymentDB packagePaymentDB, 
                                IMailboxAccessDB mailboxAccessDB, 
                                ILogger<PackageBuyController> logger,
                                IMasterDataDB masterDataDB) 
    {
        _packagePaymentDB = packagePaymentDB;
        _mailboxAccessDB = mailboxAccessDB;
        _logger = logger;
        _masterDataDB = masterDataDB;
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
        int userId = Convert.ToInt32(HttpContext.Items["User-Id"]);

        _logger.ZLogInformation($"[{userId} Request 'Buy Package'");

        ErrorState Error = await Verify(packageBuyRequest); 

        if(Error != ErrorState.None)
        {
            _logger.ZLogInformation($"[{userId}] Invalid PackageId : {packageBuyRequest.PackageId} or ReceiptId : {packageBuyRequest.ReceiptId}");

            return new PackageBuyResponse
            {
                Error = Error
            };
        }

        Error = await Buy(packageBuyRequest, userId);

        if(Error != ErrorState.None)
        {
            _logger.ZLogInformation($"[{userId}] Cannot buy This PackageId : {packageBuyRequest.PackageId}");

            return new PackageBuyResponse
            {
                Error = Error
            };
        }

        Error = await SendPackageToMail(packageBuyRequest, userId);

        if(Error != ErrorState.None)
        {
            _logger.ZLogInformation($"[{userId}] Failed Send Mail This PackageId : {packageBuyRequest.PackageId} To Player");

            Error = await UndoBuy(packageBuyRequest); 
            
            if(Error != ErrorState.None)
            {
                _logger.ZLogInformation($"[{userId}] Failed Undo Player Payment ReceiptId : {packageBuyRequest.ReceiptId}");
            }
        }

        _logger.ZLogInformation($"[{userId}] Success Send mail This PackageId : {packageBuyRequest.PackageId}");

        return new PackageBuyResponse
        {
            Error = Error
        };
    }

    async Task<ErrorState> Buy(PackageBuyRequest packageBuyRequest, int userId)
    {
        if(!await _packagePaymentDB.BuyPackage(packageBuyRequest.ReceiptId, packageBuyRequest.PackageId, userId))
        {
            return ErrorState.FailedBuyPackage;
        }

        return ErrorState.None;
    }

    async Task<ErrorState> Verify(PackageBuyRequest packageBuyRequest)
    {
        if (!await _packagePaymentDB.VerifyReceipt(packageBuyRequest.ReceiptId))
        {
            return ErrorState.InvalidReceipt;
        }
        
        if(_masterDataDB.GetMasterPackage(packageBuyRequest.PackageId).Length == 0)
        {
            return ErrorState.InvalidPackage;
        }

        return ErrorState.None;
    }

    async Task<ErrorState> SendPackageToMail(PackageBuyRequest packageBuyRequest, int userId)
    {
        MasterPackage[] masterPackages = _masterDataDB.GetMasterPackage(packageBuyRequest.PackageId);

        if (masterPackages.Length == 0)
        {
            return ErrorState.NoneExistPackgeId;
        }

        foreach (MasterPackage item in masterPackages)
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

    async Task<ErrorState> UndoBuy(PackageBuyRequest packageBuyRequest)
    {
        if(!await _packagePaymentDB.UndoBuyPackage(packageBuyRequest.ReceiptId))
        {
            return ErrorState.FailedUndoPaymentLog;
        }

        return ErrorState.None;
    }
}
