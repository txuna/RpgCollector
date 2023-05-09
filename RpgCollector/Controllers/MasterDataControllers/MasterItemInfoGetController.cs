﻿using Microsoft.AspNetCore.Mvc;
using RpgCollector.Models.MasterModel;
using RpgCollector.RequestResponseModel.MasterItemGetInfoModel;
using RpgCollector.Services;
using ZLogger;

namespace RpgCollector.Controllers.MasterDataControllers
{
    [ApiController]
    public class MasterItemInfoGetController : Controller
    {
        IMasterDataDB _masterDataDB;
        ILogger<MasterItemInfoGetController> _logger;
        IAccountMemoryDB _accountMemoryDB;
        public MasterItemInfoGetController(IMasterDataDB masterDataDB, ILogger<MasterItemInfoGetController> logger, IAccountMemoryDB accountMemoryDB)
        {
            _masterDataDB = masterDataDB;
            _logger = logger;
            _accountMemoryDB = accountMemoryDB;
        }

        [Route("/Master/Item")]
        [HttpPost]
        public async Task<MasterItemGetInfoResponse> GetItemInfo(MasterItemGetInfoRequest masterItemGetInfoRequest)
        {
            string userName = HttpContext.Request.Headers["User-Name"];
            int userId = await _accountMemoryDB.GetUserId(userName);

            _logger.ZLogInformation($"[{userId}] Request /Master/Item");

            if(userId == -1)
            {
                return new MasterItemGetInfoResponse
                {
                    Error = RequestResponseModel.ErrorState.NoneExistName
                };
            }

            MasterItem? masterItem = _masterDataDB.GetMasterItem(masterItemGetInfoRequest.ItemId);

            if (masterItem == null)
            {
                return new MasterItemGetInfoResponse
                {
                    Error = RequestResponseModel.ErrorState.NoneExistItem
                };
            }

            MasterItemAttribute? masterItemAttribute = _masterDataDB.GetMasterItemAttribute(masterItem.AttributeId);
            MasterItemType? masterItemType = _masterDataDB.GetMasterItemType(masterItemAttribute.TypeId);

            return new MasterItemGetInfoResponse
            {
                Error = RequestResponseModel.ErrorState.None,
                MasterItem = masterItem,
                AttributeName = masterItemAttribute.AttributeName,
                TypeName = masterItemType.TypeName
            };
        }
    }
}
