using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Resort.Business.Interfaces;
using Resort.Model.Models;
using Resort.Web.Controllers;

namespace Resort.Web.Areas.Staff.Controllers
{
    [Area("Staff")]
    [Authorize(Roles = "Admin,Manager")]
    public class StaffController : BaseController
    {
        private readonly IStaffOperations _staffOps;
        private readonly IMasterDataCacheOperations _masterData;

        public StaffController(IStaffOperations staffOps, IMasterDataCacheOperations masterData)
        {
            _staffOps = staffOps;
            _masterData = masterData;
        }

        public async Task<IActionResult> Index(string? department)
        {
            var staff = string.IsNullOrEmpty(department)
                ? await _staffOps.GetAllStaffAsync()
                : await _staffOps.GetStaffByDepartmentAsync(department);

            var cache = await _masterData.GetMasterDataCacheAsync();
            ViewBag.Departments = cache.GetValueOrDefault("Department", new List<string>());
            ViewBag.FilterDepartment = department;
            return View(staff);
        }

        public async Task<IActionResult> Create()
        {
            var cache = await _masterData.GetMasterDataCacheAsync();
            ViewBag.Departments = cache.GetValueOrDefault("Department", new List<string>());
            ViewBag.Positions = cache.GetValueOrDefault("Position", new List<string>());
            ViewBag.Shifts = cache.GetValueOrDefault("Shift", new List<string>());
            return View(new StaffMember());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StaffMember staffMember)
        {
            if (!ModelState.IsValid)
            {
                var cache = await _masterData.GetMasterDataCacheAsync();
                ViewBag.Departments = cache.GetValueOrDefault("Department", new List<string>());
                ViewBag.Positions = cache.GetValueOrDefault("Position", new List<string>());
                ViewBag.Shifts = cache.GetValueOrDefault("Shift", new List<string>());
                return View(staffMember);
            }
            var staff = staffMember.ToStaff();
            staff.CreatedBy = CurrentUserEmail;
            staff.UpdatedBy = CurrentUserEmail;
            await _staffOps.CreateStaffAsync(staff);
            TempData["Success"] = "Thêm nhân viên thành công!";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(string id)
        {
            var staff = await _staffOps.GetStaffByIdAsync(id);
            if (staff is null) return NotFound();
            var cache = await _masterData.GetMasterDataCacheAsync();
            ViewBag.Departments = cache.GetValueOrDefault("Department", new List<string>());
            ViewBag.Positions = cache.GetValueOrDefault("Position", new List<string>());
            ViewBag.Shifts = cache.GetValueOrDefault("Shift", new List<string>());
            return View(StaffMember.FromStaff(staff));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(StaffMember staffMember)
        {
            if (!ModelState.IsValid)
            {
                var cache = await _masterData.GetMasterDataCacheAsync();
                ViewBag.Departments = cache.GetValueOrDefault("Department", new List<string>());
                ViewBag.Positions = cache.GetValueOrDefault("Position", new List<string>());
                ViewBag.Shifts = cache.GetValueOrDefault("Shift", new List<string>());
                return View(staffMember);
            }
            var staff = staffMember.ToStaff();
            staff.UpdatedBy = CurrentUserEmail;
            await _staffOps.UpdateStaffAsync(staff);
            TempData["Success"] = "Cập nhật nhân viên thành công!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(string id)
        {
            await _staffOps.DeactivateStaffAsync(id, CurrentUserEmail);
            TempData["Success"] = "Đã cho nhân viên nghỉ việc!";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// ViewModel wrapper để tránh conflict tên với namespace Staff.
    /// </summary>
    public class StaffMember
    {
        public string Id { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string IdCard { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public string Gender { get; set; } = "Nam";
        public string Position { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public decimal Salary { get; set; }
        public string Shift { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public string? UserId { get; set; }

        public Resort.Model.Models.Staff ToStaff() => new()
        {
            Id = Id,
            Code = Code,
            FullName = FullName,
            IdCard = IdCard,
            DateOfBirth = DateOfBirth,
            Gender = Gender,
            Position = Position,
            Department = Department,
            Phone = Phone,
            Email = Email,
            Salary = Salary,
            Shift = Shift,
            IsActive = IsActive,
            UserId = UserId
        };

        public static StaffMember FromStaff(Resort.Model.Models.Staff s) => new()
        {
            Id = s.Id,
            Code = s.Code,
            FullName = s.FullName,
            IdCard = s.IdCard,
            DateOfBirth = s.DateOfBirth,
            Gender = s.Gender,
            Position = s.Position,
            Department = s.Department,
            Phone = s.Phone,
            Email = s.Email,
            Salary = s.Salary,
            Shift = s.Shift,
            IsActive = s.IsActive,
            UserId = s.UserId
        };
    }
}
