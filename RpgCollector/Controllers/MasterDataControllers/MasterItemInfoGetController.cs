using Microsoft.AspNetCore.Mvc;
using RpgCollector.Models.MasterModel;
using RpgCollector.RequestResponseModel.MasterItemGetInfoModel;
using RpgCollector.Services;

namespace RpgCollector.Controllers.MasterDataControllers
{
    [ApiController]
    public class MasterItemInfoGetController : Controller
    {
        IMasterDataDB _masterDataDB { get; set; }
        ILogger<MasterItemInfoGetController> _logger;
        public MasterItemInfoGetController(IMasterDataDB masterDataDB, ILogger<MasterItemInfoGetController> logger)
        {
            _masterDataDB = masterDataDB;
            _logger = logger;
        }

        [Route("/Master/Item")]
        [HttpPost]
        public MasterItemGetInfoResponse GetItemInfo(MasterItemGetInfoRequest masterItemGetInfoRequest)
        {
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
