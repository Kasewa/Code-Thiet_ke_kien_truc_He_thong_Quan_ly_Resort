using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Resort.Business.Interfaces;
using Resort.Model.Models;
using Resort.Web.Controllers;

namespace Resort.Web.Areas.Services.Controllers
{
    [Area("Services")]
    [Authorize(Roles = "Admin,Manager")]
    public class ServicesController : BaseController
    {
        private readonly IServiceOperations _serviceOps;
        private readonly IMasterDataCacheOperations _masterData;

        public ServicesController(IServiceOperations serviceOps, IMasterDataCacheOperations masterData)
        {
            _serviceOps = serviceOps;
            _masterData = masterData;
        }

        public async Task<IActionResult> Index(string? type)
        {
            var services = string.IsNullOrEmpty(type)
                ? await _serviceOps.GetAllServicesAsync()
                : await _serviceOps.GetServicesByTypeAsync(type);

            var cache = await _masterData.GetMasterDataCacheAsync();
            ViewBag.ServiceTypes = cache.GetValueOrDefault("ServiceType", new List<string>());
            ViewBag.FilterType = type;
            return View(services);
        }

        public async Task<IActionResult> Create()
        {
            var cache = await _masterData.GetMasterDataCacheAsync();
            ViewBag.ServiceTypes = cache.GetValueOrDefault("ServiceType", new List<string>());
            var allServices = await _serviceOps.GetAllServicesAsync();
            var nextSvcCode = $"DV{(allServices.Count + 1):D3}";
            return View(new Service { Code = nextSvcCode });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Service service)
        {
            if (!ModelState.IsValid)
            {
                var cache = await _masterData.GetMasterDataCacheAsync();
                ViewBag.ServiceTypes = cache.GetValueOrDefault("ServiceType", new List<string>());
                return View(service);
            }
            service.CreatedBy = CurrentUserEmail;
            service.UpdatedBy = CurrentUserEmail;
            await _serviceOps.CreateServiceAsync(service);
            TempData["Success"] = "Thêm dịch vụ thành công!";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(string id)
        {
            var service = await _serviceOps.GetServiceByIdAsync(id);
            if (service is null) return NotFound();
            var cache = await _masterData.GetMasterDataCacheAsync();
            ViewBag.ServiceTypes = cache.GetValueOrDefault("ServiceType", new List<string>());
            return View(service);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Service service)
        {
            if (!ModelState.IsValid)
            {
                var cache = await _masterData.GetMasterDataCacheAsync();
                ViewBag.ServiceTypes = cache.GetValueOrDefault("ServiceType", new List<string>());
                return View(service);
            }
            service.UpdatedBy = CurrentUserEmail;
            await _serviceOps.UpdateServiceAsync(service);
            TempData["Success"] = "Cập nhật dịch vụ thành công!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                await _serviceOps.DeleteServiceAsync(id);
                TempData["Success"] = "Xóa dịch vụ thành công!";
            }
            catch (Exception)
            {
                TempData["Error"] = "Không thể xóa: dịch vụ này đã được sử dụng trong đặt phòng.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(string id)
        {
            await _serviceOps.ToggleServiceStatusAsync(id, CurrentUserEmail);
            TempData["Success"] = "Đổi trạng thái thành công!";
            return RedirectToAction(nameof(Index));
        }
    }
}
