using Microsoft.AspNetCore.Mvc;
using RpgCollector.RequestResponseModel;
using RpgCollector.Services;
using RpgCollector.Models;
using ZLogger;
using RpgCollector.Models.PackageItemModel;
using RpgCollector.RequestResponseModel.PaymentReqRes;

namespace RpgCollector.Controllers.PackageControllers;

[ApiController]
public class PackageGiveController : Controller
{
    IPackagePaymentDB _packagePaymentDB; 
    IMailboxAccessDB _mailboxAccessDB;
    ILogger<PackageGiveController> _logger;
    IMasterDataDB _masterDataDB;

    public PackageGiveController(IPackagePaymentDB packagePaymentDB, 
                                IMailboxAccessDB mailboxAccessDB, 
                                ILogger<PackageGiveController> logger,
                                IMasterDataDB masterDataDB) 
    {
        _packagePaymentDB = packagePaymentDB;
        _mailboxAccessDB = mailboxAccessDB;
        _logger = logger;
        _masterDataDB = masterDataDB;
    }

    [Route("/Package/Buy")]
    [HttpPost]
    public async Task<PackageBuyResponse> PaymentPackage(PackageBuyRequest packageBuyRequest)
    {
        int userId = Convert.ToInt32(HttpContext.Items["User-Id"]);

        _logger.ZLogDebug($"[{userId}] Request /Package/Buy");

        ErrorCode Error = await Verify(packageBuyRequest); 

        if(Error != ErrorCode.None)
        {
            _logger.ZLogDebug($"[{userId}] Invalid PackageId : {packageBuyRequest.PackageId} or ReceiptId : {packageBuyRequest.ReceiptId}");

            return new PackageBuyResponse
            {
                Error = Error
            };
        }

        Error = await ReportReceipt(packageBuyRequest, userId);

        if(Error != ErrorCode.None)
        {
            _logger.ZLogDebug($"[{userId}] Cannot buy This PackageId : {packageBuyRequest.PackageId}");

            return new PackageBuyResponse
            {
                Error = Error
            };
        }

        Error = await SendPackageToMail(packageBuyRequest, userId);

        if(Error != ErrorCode.None)
        {
            _logger.ZLogDebug($"[{userId}] Failed Send Mail This PackageId : {packageBuyRequest.PackageId} To Player");

            Error = await UndoReportPackage(packageBuyRequest); 
            
            if(Error != ErrorCode.None)
            {
                _logger.ZLogInformation($"[{userId}] Failed Undo Player Payment ReceiptId : {packageBuyRequest.ReceiptId}");
            }
        }

        _logger.ZLogDebug($"[{userId}] Success Send mail This PackageId : {packageBuyRequest.PackageId}");

        return new PackageBuyResponse
        {
            Error = Error
        };
    }

    async Task<ErrorCode> ReportReceipt(PackageBuyRequest packageBuyRequest, int userId)
    {
        if(await _packagePaymentDB.ReportReceipt(packageBuyRequest.ReceiptId, packageBuyRequest.PackageId, userId) == false)
        {
            return ErrorCode.FailedBuyPackage;
        }

        return ErrorCode.None;
    }

    async Task<ErrorCode> Verify(PackageBuyRequest packageBuyRequest)
    {
        if (await _packagePaymentDB.VerifyReceipt(packageBuyRequest.ReceiptId) == false)
        {
            return ErrorCode.InvalidReceipt;
        }
        
        if(_masterDataDB.GetMasterPackage(packageBuyRequest.PackageId).Length == 0)
        {
            return ErrorCode.InvalidPackage;
        }

        return ErrorCode.None;
    }

    async Task<ErrorCode> SendPackageToMail(PackageBuyRequest packageBuyRequest, int userId)
    {
        MasterPackage[] masterPackages = _masterDataDB.GetMasterPackage(packageBuyRequest.PackageId);

        if (masterPackages.Length == 0)
        {
            return ErrorCode.NoneExistPackgeId;
        }

        object[][] values = new object[masterPackages.Length][];

        int index = 0;
        foreach (MasterPackage item in masterPackages)
        {
            values[index] = new object[] { 1, 
                                           userId,
                                           "Packge Item!",
                                           "A package item has arrived. Thanks for your purchase",
                                            0, 0, item.ItemId, item.Quantity, 0, DateTime.Now.AddDays(30)
            };
            index += 1;
        }

        if(await _mailboxAccessDB.SendMultipleMail(values) == false)
        {
            return ErrorCode.FailedAddMailItemToPlayer;
        }

        return ErrorCode.None;
    }

    async Task<ErrorCode> UndoReportPackage(PackageBuyRequest packageBuyRequest)
    {
        if(await _packagePaymentDB.UndoReportPackage(packageBuyRequest.ReceiptId) == false)
        {
            return ErrorCode.FailedUndoPaymentLog;
        }

        return ErrorCode.None;
    }
}
