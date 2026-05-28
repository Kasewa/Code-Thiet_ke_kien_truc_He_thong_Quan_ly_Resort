using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Resort.Business.Interfaces;
using Resort.Model.Models;
using Resort.Web.Controllers;

namespace Resort.Web.Areas.Administration.Controllers
{
    [Area("Administration")]
    [Authorize(Roles = "Admin")]
    public class AdministrationController : BaseController
    {
        private readonly IMasterDataOperations _masterDataOps;
        private readonly IMasterDataCacheOperations _cacheOps;

        public AdministrationController(
            IMasterDataOperations masterDataOps,
            IMasterDataCacheOperations cacheOps)
        {
            _masterDataOps = masterDataOps;
            _cacheOps = cacheOps;
        }

        public async Task<IActionResult> Index()
        {
            var keys = await _masterDataOps.GetAllMasterKeysAsync();
            return View(keys);
        }

        public async Task<IActionResult> MasterDataValues(string keyId)
        {
            var keys = await _masterDataOps.GetAllMasterKeysAsync();
            var key = keys.FirstOrDefault(k => k.Id == keyId);
            if (key is null) return NotFound();

            var values = await _masterDataOps.GetAllMasterValuesByKeyAsync(key.Key);
            ViewBag.MasterKey = key;
            return View(values);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddValue(MasterDataValue model) 
        {
            model.CreatedBy = CurrentUserEmail;
            model.UpdatedBy = CurrentUserEmail;

            await _masterDataOps.InsertMasterValueAsync(model);
            await _cacheOps.CreateMasterDataCacheAsync();

            TempData["Success"] = "Thêm giá trị thành công!";

            return RedirectToAction(nameof(MasterDataValues), new { keyId = model.MasterDataKeyId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> RefreshCache()
        {
            await _cacheOps.CreateMasterDataCacheAsync();
            TempData["Success"] = "Làm mới cache thành công!";
            return RedirectToAction(nameof(Index));
        }
    }
}
