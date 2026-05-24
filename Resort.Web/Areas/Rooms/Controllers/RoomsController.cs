using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Resort.Business.Interfaces;
using Resort.Model.Models;
using Resort.Web.Controllers;

namespace Resort.Web.Areas.Rooms.Controllers
{
    [Area("Rooms")]
    [Authorize(Roles = "Admin,Manager")]
    public class RoomsController : BaseController
    {
        private readonly IRoomOperations _roomOps;
        private readonly IMasterDataCacheOperations _masterData;

        public RoomsController(IRoomOperations roomOps, IMasterDataCacheOperations masterData)
        {
            _roomOps = roomOps;
            _masterData = masterData;
        }

        public async Task<IActionResult> Index(string? status, string? type)
        {
            var rooms = string.IsNullOrEmpty(status) && string.IsNullOrEmpty(type)
                ? await _roomOps.GetAllRoomsAsync()
                : !string.IsNullOrEmpty(status)
                    ? await _roomOps.GetRoomsByStatusAsync(status)
                    : await _roomOps.GetRoomsByTypeAsync(type!);

            var cache = await _masterData.GetMasterDataCacheAsync();
            ViewBag.RoomTypes = cache.GetValueOrDefault("RoomType", new List<string>());
            ViewBag.RoomStatuses = cache.GetValueOrDefault("RoomStatus", new List<string>());
            ViewBag.FilterStatus = status;
            ViewBag.FilterType = type;

            return View(rooms);
        }

        public async Task<IActionResult> Create()
        {
            var cache = await _masterData.GetMasterDataCacheAsync();
            ViewBag.RoomTypes = cache.GetValueOrDefault("RoomType", new List<string>());
            return View(new Room());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Room room)
        {
            if (!ModelState.IsValid)
            {
                var cache = await _masterData.GetMasterDataCacheAsync();
                ViewBag.RoomTypes = cache.GetValueOrDefault("RoomType", new List<string>());
                return View(room);
            }

            room.CreatedBy = CurrentUserEmail;
            var success = await _roomOps.CreateRoomAsync(room);
            if (success)
            {
                TempData["Success"] = "Tạo phòng thành công!";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(string.Empty, "Tạo phòng thất bại.");
            return View(room);
        }

        public async Task<IActionResult> Edit(string id)
        {
            var room = await _roomOps.GetRoomByIdAsync(id);
            if (room is null) return NotFound();

            var cache = await _masterData.GetMasterDataCacheAsync();
            ViewBag.RoomTypes = cache.GetValueOrDefault("RoomType", new List<string>());
            ViewBag.RoomStatuses = cache.GetValueOrDefault("RoomStatus", new List<string>());

            return View(room);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Room room)
        {
            if (!ModelState.IsValid)
            {
                var cache = await _masterData.GetMasterDataCacheAsync();
                ViewBag.RoomTypes = cache.GetValueOrDefault("RoomType", new List<string>());
                ViewBag.RoomStatuses = cache.GetValueOrDefault("RoomStatus", new List<string>());
                return View(room);
            }

            room.UpdatedBy = CurrentUserEmail;
            var success = await _roomOps.UpdateRoomAsync(room);
            if (success)
            {
                TempData["Success"] = "Cập nhật phòng thành công!";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(string.Empty, "Cập nhật thất bại.");
            return View(room);
        }

        public async Task<IActionResult> Details(string id)
        {
            var room = await _roomOps.GetRoomByIdAsync(id);
            if (room is null) return NotFound();
            return View(room);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            await _roomOps.DeleteRoomAsync(id);
            TempData["Success"] = "Xóa phòng thành công!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(string id, string status)
        {
            await _roomOps.UpdateRoomStatusAsync(id, status, CurrentUserEmail);
            TempData["Success"] = "Cập nhật trạng thái thành công!";
            return RedirectToAction(nameof(Index));
        }
    }
}
