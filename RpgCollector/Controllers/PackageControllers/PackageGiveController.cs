using Microsoft.AspNetCore.Mvc;
using RpgCollector.RequestResponseModel.PaymentModel;
using RpgCollector.RequestResponseModel;
using RpgCollector.Services;
using RpgCollector.Models;
using ZLogger;
using RpgCollector.Models.PackageItemModel;

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

        ErrorCode Error = await Verify(packageBuyRequest); 

        if(Error != ErrorCode.None)
        {
            _logger.ZLogInformation($"[{userId}] Invalid PackageId : {packageBuyRequest.PackageId} or ReceiptId : {packageBuyRequest.ReceiptId}");

            return new PackageBuyResponse
            {
                Error = Error
            };
        }

        Error = await ReportReceipt(packageBuyRequest, userId);

        if(Error != ErrorCode.None)
        {
            _logger.ZLogInformation($"[{userId}] Cannot buy This PackageId : {packageBuyRequest.PackageId}");

            return new PackageBuyResponse
            {
                Error = Error
            };
        }

        Error = await SendPackageToMail(packageBuyRequest, userId);

        if(Error != ErrorCode.None)
        {
            _logger.ZLogInformation($"[{userId}] Failed Send Mail This PackageId : {packageBuyRequest.PackageId} To Player");

            Error = await UndoReportPackage(packageBuyRequest); 
            
            if(Error != ErrorCode.None)
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

    async Task<ErrorCode> ReportReceipt(PackageBuyRequest packageBuyRequest, int userId)
    {
        if(!await _packagePaymentDB.ReportReceipt(packageBuyRequest.ReceiptId, packageBuyRequest.PackageId, userId))
        {
            return ErrorCode.FailedBuyPackage;
        }

        return ErrorCode.None;
    }

    async Task<ErrorCode> Verify(PackageBuyRequest packageBuyRequest)
    {
        if (!await _packagePaymentDB.VerifyReceipt(packageBuyRequest.ReceiptId))
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

        foreach (MasterPackage item in masterPackages)
        {
            if (!await _mailboxAccessDB.SendMail(1,
                                                userId,
                                                "Packge Item!",
                                                "A package item has arrived. Thanks for your purchase",
                                                item.ItemId,
                                                item.Quantity))
            {
                return ErrorCode.FailedSendMail;
            }
        }

        return ErrorCode.None;
    }

    async Task<ErrorCode> UndoReportPackage(PackageBuyRequest packageBuyRequest)
    {
        if(!await _packagePaymentDB.UndoReportPackage(packageBuyRequest.ReceiptId))
        {
            return ErrorCode.FailedUndoPaymentLog;
        }

        return ErrorCode.None;
    }
}
