using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Resort.Business.Interfaces;
using Resort.Model.Models;
using Resort.Web.Controllers;

namespace Resort.Web.Areas.Repairs.Controllers
{
    [Area("Repairs")]
    [Authorize(Roles = "Admin,Manager,Technician")]
    public class RepairsController : BaseController
    {
        private readonly IRepairOperations _repairOps;
        private readonly IRoomOperations _roomOps;
        private readonly IStaffOperations _staffOps;
        private readonly IMasterDataCacheOperations _masterData;

        public RepairsController(
            IRepairOperations repairOps,
            IRoomOperations roomOps,
            IStaffOperations staffOps,
            IMasterDataCacheOperations masterData)
        {
            _repairOps = repairOps;
            _roomOps = roomOps;
            _staffOps = staffOps;
            _masterData = masterData;
        }

        public async Task<IActionResult> Index(string? status)
        {
            var requests = string.IsNullOrEmpty(status)
                ? await _repairOps.GetAllRepairRequestsAsync()
                : await _repairOps.GetRepairRequestsByStatusAsync(status);

            var rooms = await _roomOps.GetAllRoomsAsync();
            ViewBag.RoomMap = rooms.ToDictionary(r => r.Id, r => r.Code + " - " + r.Name);
            ViewBag.FilterStatus = status;
            return View(requests);
        }

        public async Task<IActionResult> Create()
        {
            var rooms = await _roomOps.GetAllRoomsAsync();
            var cache = await _masterData.GetMasterDataCacheAsync();
            ViewBag.Rooms = rooms;
            ViewBag.Priorities = cache.GetValueOrDefault("RepairPriority", new List<string>());
            return View(new RepairRequest());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RepairRequest request)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Rooms = await _roomOps.GetAllRoomsAsync();
                var cache = await _masterData.GetMasterDataCacheAsync();
                ViewBag.Priorities = cache.GetValueOrDefault("RepairPriority", new List<string>());
                return View(request);
            }
            request.CreatedBy = CurrentUserEmail;
            request.UpdatedBy = CurrentUserEmail;
            await _repairOps.CreateRepairRequestAsync(request);
            TempData["Success"] = "Tạo yêu cầu sửa chữa thành công!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Assign(string id, string technicianId)
        {
            await _repairOps.AssignTechnicianAsync(id, technicianId, CurrentUserEmail);
            TempData["Success"] = "Phân công kỹ thuật viên thành công!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(string id, string status)
        {
            await _repairOps.UpdateRepairStatusAsync(id, status, CurrentUserEmail);
            TempData["Success"] = "Cập nhật trạng thái thành công!";
            return RedirectToAction(nameof(Index));
        }
    }
}
