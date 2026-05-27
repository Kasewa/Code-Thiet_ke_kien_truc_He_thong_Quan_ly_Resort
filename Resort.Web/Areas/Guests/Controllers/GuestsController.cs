using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Resort.Business.Interfaces;
using Resort.Model.Models;
using Resort.Web.Controllers;

namespace Resort.Web.Areas.Guests.Controllers
{
    [Area("Guests")]
    [Authorize(Roles = "Admin,Manager,Receptionist")]
    public class GuestsController : BaseController
    {
        private readonly IGuestOperations _guestOps;
        private readonly IMasterDataCacheOperations _masterData;

        public GuestsController(IGuestOperations guestOps, IMasterDataCacheOperations masterData)
        {
            _guestOps = guestOps;
            _masterData = masterData;
        }

        public async Task<IActionResult> Index(string? search, string? nationality)
        {
            var guests = !string.IsNullOrEmpty(search)
                ? await _guestOps.SearchGuestsAsync(search)
                : !string.IsNullOrEmpty(nationality)
                    ? await _guestOps.GetGuestsByNationalityAsync(nationality)
                    : await _guestOps.GetAllGuestsAsync();

            var cache = await _masterData.GetMasterDataCacheAsync();
            ViewBag.Nationalities = cache.GetValueOrDefault("Nationality", new List<string>());
            ViewBag.Search = search;
            ViewBag.FilterNationality = nationality;

            return View(guests);
        }

        public async Task<IActionResult> Create()
        {
            var cache = await _masterData.GetMasterDataCacheAsync();
            ViewBag.Nationalities = cache.GetValueOrDefault("Nationality", new List<string>());

            // Auto-generate Code gợi ý
            var allGuests = await _guestOps.GetAllGuestsAsync();
            var nextCode = $"KH{(allGuests.Count + 1):D3}";

            return View(new Guest { Code = nextCode, Nationality = "Việt Nam" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Guest guest)
        {
            if (string.IsNullOrWhiteSpace(guest.Code))
            {
                var allGuests = await _guestOps.GetAllGuestsAsync();
                guest.Code = $"KH{(allGuests.Count + 1):D3}";
                ModelState.Remove(nameof(guest.Code));
            }

            if (!ModelState.IsValid)
            {
                var cache = await _masterData.GetMasterDataCacheAsync();
                ViewBag.Nationalities = cache.GetValueOrDefault("Nationality", new List<string>());
                return View(guest);
            }

            guest.CreatedBy = CurrentUserEmail;
            var success = await _guestOps.CreateGuestAsync(guest);
            if (success)
            {
                TempData["Success"] = "Thêm khách hàng thành công!";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(string.Empty, "Thêm khách hàng thất bại.");
            return View(guest);
        }

        public async Task<IActionResult> Edit(string id)
        {
            var guest = await _guestOps.GetGuestByIdAsync(id);
            if (guest is null) return NotFound();

            var cache = await _masterData.GetMasterDataCacheAsync();
            ViewBag.Nationalities = cache.GetValueOrDefault("Nationality", new List<string>());
            return View(guest);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guest guest)
        {
            if (!ModelState.IsValid)
            {
                var cache = await _masterData.GetMasterDataCacheAsync();
                ViewBag.Nationalities = cache.GetValueOrDefault("Nationality", new List<string>());
                return View(guest);
            }

            guest.UpdatedBy = CurrentUserEmail;
            var success = await _guestOps.UpdateGuestAsync(guest);
            if (success)
            {
                TempData["Success"] = "Cập nhật khách hàng thành công!";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(string.Empty, "Cập nhật thất bại.");
            return View(guest);
        }

        public async Task<IActionResult> Details(string id)
        {
            var guest = await _guestOps.GetGuestByIdAsync(id);
            if (guest is null) return NotFound();
            return View(guest);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var success = await _guestOps.DeleteGuestAsync(id);
                if (success)
                    TempData["Success"] = "Xóa khách hàng thành công!";
                else
                    TempData["Error"] = "Không tìm thấy khách hàng.";
            }
            catch (Exception)
            {
                TempData["Error"] = "Không thể xóa: khách hàng này đã có lịch sử đặt phòng.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
