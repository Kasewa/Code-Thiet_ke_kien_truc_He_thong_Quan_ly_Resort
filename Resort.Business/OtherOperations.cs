using Resort.Business.Interfaces;
using Resort.DataAccess;
using Resort.Model.Models;

namespace Resort.Business
{
    // ── SERVICES ─────────────────────────────────────────────────────────────
    public class ServiceOperations : IServiceOperations
    {
        private readonly IUnitOfWork _unitOfWork;
        public ServiceOperations(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        public async Task<List<Service>> GetAllServicesAsync()
        {
            var services = await _unitOfWork.Repository<Service>().GetAllAsync();
            return services.OrderBy(s => s.ServiceType).ThenBy(s => s.Name).ToList();
        }

        public async Task<Service?> GetServiceByIdAsync(string id)
            => await _unitOfWork.Repository<Service>().GetByIdAsync(id);

        public async Task<List<Service>> GetActiveServicesAsync()
        {
            var services = await _unitOfWork.Repository<Service>().FindAllAsync(s => s.IsActive);
            return services.OrderBy(s => s.Name).ToList();
        }

        public async Task<List<Service>> GetServicesByTypeAsync(string serviceType)
        {
            var services = await _unitOfWork.Repository<Service>().FindAllAsync(s => s.ServiceType == serviceType && s.IsActive);
            return services.OrderBy(s => s.Name).ToList();
        }

        public async Task<bool> CreateServiceAsync(Service service)
        {
            service.Id = Guid.NewGuid().ToString();
            service.CreatedDate = DateTime.UtcNow;
            await _unitOfWork.Repository<Service>().AddAsync(service);
            return await _unitOfWork.CommitAsync() > 0;
        }

        public async Task<bool> UpdateServiceAsync(Service service)
        {
            var existing = await _unitOfWork.Repository<Service>().GetByIdAsync(service.Id);
            if (existing is null) return false;

            existing.Code = service.Code;
            existing.Name = service.Name;
            existing.Description = service.Description;
            existing.Price = service.Price;
            existing.Unit = service.Unit;
            existing.ServiceType = service.ServiceType;
            existing.IsActive = service.IsActive;
            existing.UpdatedBy = service.UpdatedBy;
            existing.UpdatedDate = DateTime.UtcNow;

            await _unitOfWork.Repository<Service>().UpdateAsync(existing);
            return await _unitOfWork.CommitAsync() > 0;
        }

        public async Task<bool> DeleteServiceAsync(string id)
        {
            var service = await _unitOfWork.Repository<Service>().GetByIdAsync(id);
            if (service is null) return false;
            await _unitOfWork.Repository<Service>().DeleteAsync(service);
            return await _unitOfWork.CommitAsync() > 0;
        }

        public async Task<bool> ToggleServiceStatusAsync(string id, string updatedBy)
        {
            var service = await _unitOfWork.Repository<Service>().GetByIdAsync(id);
            if (service is null) return false;
            service.IsActive = !service.IsActive;
            service.UpdatedBy = updatedBy;
            service.UpdatedDate = DateTime.UtcNow;
            await _unitOfWork.Repository<Service>().UpdateAsync(service);
            return await _unitOfWork.CommitAsync() > 0;
        }
    }

    // ── REPAIRS ──────────────────────────────────────────────────────────────
    public class RepairOperations : IRepairOperations
    {
        private readonly IUnitOfWork _unitOfWork;
        public RepairOperations(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        public async Task<List<RepairRequest>> GetAllRepairRequestsAsync()
        {
            var requests = await _unitOfWork.Repository<RepairRequest>().GetAllAsync();
            return requests.OrderByDescending(r => r.CreatedDate).ToList();
        }

        public async Task<RepairRequest?> GetRepairRequestByIdAsync(string id)
            => await _unitOfWork.Repository<RepairRequest>().GetByIdAsync(id);

        public async Task<List<RepairRequest>> GetRepairRequestsByStatusAsync(string status)
        {
            var requests = await _unitOfWork.Repository<RepairRequest>().FindAllAsync(r => r.Status == status);
            return requests.OrderByDescending(r => r.CreatedDate).ToList();
        }

        public async Task<List<RepairRequest>> GetRepairRequestsByTechnicianAsync(string technicianId)
        {
            var requests = await _unitOfWork.Repository<RepairRequest>().FindAllAsync(r => r.TechnicianId == technicianId);
            return requests.OrderByDescending(r => r.CreatedDate).ToList();
        }

        public async Task<bool> CreateRepairRequestAsync(RepairRequest request)
        {
            request.Id = Guid.NewGuid().ToString();
            request.CreatedDate = DateTime.UtcNow;
            request.Status = RepairStatus.Pending;
            await _unitOfWork.Repository<RepairRequest>().AddAsync(request);
            return await _unitOfWork.CommitAsync() > 0;
        }

        public async Task<bool> UpdateRepairRequestAsync(RepairRequest request)
        {
            var existing = await _unitOfWork.Repository<RepairRequest>().GetByIdAsync(request.Id);
            if (existing is null) return false;

            existing.Description = request.Description;
            existing.Priority = request.Priority;
            existing.Notes = request.Notes;
            existing.UpdatedBy = request.UpdatedBy;
            existing.UpdatedDate = DateTime.UtcNow;

            await _unitOfWork.Repository<RepairRequest>().UpdateAsync(existing);
            return await _unitOfWork.CommitAsync() > 0;
        }

        public async Task<bool> AssignTechnicianAsync(string requestId, string technicianId, string updatedBy)
        {
            var request = await _unitOfWork.Repository<RepairRequest>().GetByIdAsync(requestId);
            if (request is null) return false;

            request.TechnicianId = technicianId;
            request.Status = RepairStatus.InProgress;
            request.UpdatedBy = updatedBy;
            request.UpdatedDate = DateTime.UtcNow;

            await _unitOfWork.Repository<RepairRequest>().UpdateAsync(request);
            return await _unitOfWork.CommitAsync() > 0;
        }

        public async Task<bool> UpdateRepairStatusAsync(string requestId, string status, string updatedBy)
        {
            var request = await _unitOfWork.Repository<RepairRequest>().GetByIdAsync(requestId);
            if (request is null) return false;

            request.Status = status;
            request.UpdatedBy = updatedBy;
            request.UpdatedDate = DateTime.UtcNow;
            if (status == RepairStatus.Completed)
                request.CompletedDate = DateTime.UtcNow;

            await _unitOfWork.Repository<RepairRequest>().UpdateAsync(request);
            return await _unitOfWork.CommitAsync() > 0;
        }
    }

    // ── STAFF ─────────────────────────────────────────────────────────────────
    public class StaffOperations : IStaffOperations
    {
        private readonly IUnitOfWork _unitOfWork;
        public StaffOperations(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        public async Task<List<Staff>> GetAllStaffAsync()
        {
            var staff = await _unitOfWork.Repository<Staff>().GetAllAsync();
            return staff.OrderBy(s => s.FullName).ToList();
        }

        public async Task<Staff?> GetStaffByIdAsync(string id)
            => await _unitOfWork.Repository<Staff>().GetByIdAsync(id);

        public async Task<Staff?> GetStaffByUserIdAsync(string userId)
            => await _unitOfWork.Repository<Staff>().FindAsync(s => s.UserId == userId);

        public async Task<List<Staff>> GetStaffByDepartmentAsync(string department)
        {
            var staff = await _unitOfWork.Repository<Staff>().FindAllAsync(s => s.Department == department && s.IsActive);
            return staff.OrderBy(s => s.FullName).ToList();
        }

        public async Task<bool> CreateStaffAsync(Staff staff)
        {
            staff.Id = Guid.NewGuid().ToString();
            staff.CreatedDate = DateTime.UtcNow;
            await _unitOfWork.Repository<Staff>().AddAsync(staff);
            return await _unitOfWork.CommitAsync() > 0;
        }

        public async Task<bool> UpdateStaffAsync(Staff staff)
        {
            var existing = await _unitOfWork.Repository<Staff>().GetByIdAsync(staff.Id);
            if (existing is null) return false;

            existing.Code = staff.Code;
            existing.FullName = staff.FullName;
            existing.IdCard = staff.IdCard;
            existing.DateOfBirth = staff.DateOfBirth;
            existing.Gender = staff.Gender;
            existing.Position = staff.Position;
            existing.Department = staff.Department;
            existing.Phone = staff.Phone;
            existing.Email = staff.Email;
            existing.Salary = staff.Salary;
            existing.Shift = staff.Shift;
            existing.IsActive = staff.IsActive;
            existing.UpdatedBy = staff.UpdatedBy;
            existing.UpdatedDate = DateTime.UtcNow;

            await _unitOfWork.Repository<Staff>().UpdateAsync(existing);
            return await _unitOfWork.CommitAsync() > 0;
        }

        public async Task<bool> DeactivateStaffAsync(string id, string updatedBy)
        {
            var staff = await _unitOfWork.Repository<Staff>().GetByIdAsync(id);
            if (staff is null) return false;

            staff.IsActive = false;
            staff.UpdatedBy = updatedBy;
            staff.UpdatedDate = DateTime.UtcNow;

            await _unitOfWork.Repository<Staff>().UpdateAsync(staff);
            return await _unitOfWork.CommitAsync() > 0;
        }
    }

    // ── NOTIFICATIONS ─────────────────────────────────────────────────────────
    public class NotificationOperations : INotificationOperations
    {
        private readonly IUnitOfWork _unitOfWork;
        public NotificationOperations(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        public async Task<List<Notification>> GetNotificationsForUserAsync(string userId)
        {
            var notifications = await _unitOfWork.Repository<Notification>()
                .FindAllAsync(n => n.RecipientId == userId || string.IsNullOrEmpty(n.RecipientId));
            return notifications.OrderByDescending(n => n.CreatedDate).ToList();
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            var notifications = await _unitOfWork.Repository<Notification>()
                .FindAllAsync(n => (n.RecipientId == userId || string.IsNullOrEmpty(n.RecipientId)) && !n.IsRead);
            return notifications.Count();
        }

        public async Task<bool> CreateNotificationAsync(Notification notification)
        {
            notification.Id = Guid.NewGuid().ToString();
            notification.CreatedDate = DateTime.UtcNow;
            await _unitOfWork.Repository<Notification>().AddAsync(notification);
            return await _unitOfWork.CommitAsync() > 0;
        }

        public async Task<bool> MarkAsReadAsync(string notificationId, string userId)
        {
            var notification = await _unitOfWork.Repository<Notification>().GetByIdAsync(notificationId);
            if (notification is null) return false;
            notification.IsRead = true;
            notification.UpdatedBy = userId;
            notification.UpdatedDate = DateTime.UtcNow;
            await _unitOfWork.Repository<Notification>().UpdateAsync(notification);
            return await _unitOfWork.CommitAsync() > 0;
        }

        public async Task<bool> MarkAllAsReadAsync(string userId)
        {
            var notifications = await _unitOfWork.Repository<Notification>()
                .FindAllAsync(n => (n.RecipientId == userId || string.IsNullOrEmpty(n.RecipientId)) && !n.IsRead);

            foreach (var n in notifications)
            {
                n.IsRead = true;
                n.UpdatedBy = userId;
                n.UpdatedDate = DateTime.UtcNow;
                await _unitOfWork.Repository<Notification>().UpdateAsync(n);
            }

            return await _unitOfWork.CommitAsync() > 0;
        }

        public async Task<bool> BroadcastNotificationAsync(Notification notification)
        {
            notification.RecipientId = string.Empty;  // empty = broadcast
            return await CreateNotificationAsync(notification);
        }
    }

    // ── REPORTS ───────────────────────────────────────────────────────────────
    public class ReportOperations : IReportOperations
    {
        private readonly IUnitOfWork _unitOfWork;
        public ReportOperations(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        public async Task<decimal> GetMonthlyRevenueAsync(int year, int month)
        {
            var bookings = await _unitOfWork.Repository<Booking>()
                .FindAllAsync(b =>
                    b.Status == BookingStatus.CheckedOut &&
                    b.ActualCheckOut.HasValue &&
                    b.ActualCheckOut.Value.Year == year &&
                    b.ActualCheckOut.Value.Month == month);

            return bookings.Sum(b => b.TotalAmount);
        }

        public async Task<List<(int Month, decimal Revenue)>> GetYearlyRevenueAsync(int year)
        {
            var result = new List<(int Month, decimal Revenue)>();
            for (int m = 1; m <= 12; m++)
            {
                var revenue = await GetMonthlyRevenueAsync(year, m);
                result.Add((m, revenue));
            }
            return result;
        }

        public async Task<int> GetOccupancyCountAsync(DateTime date)
        {
            var bookings = await _unitOfWork.Repository<Booking>()
                .FindAllAsync(b =>
                    b.Status == BookingStatus.CheckedIn &&
                    b.CheckInDate.Date <= date.Date &&
                    b.CheckOutDate.Date >= date.Date);
            return bookings.Count();
        }

        public async Task<Dictionary<string, int>> GetRoomTypeDistributionAsync()
        {
            var rooms = await _unitOfWork.Repository<Room>().GetAllAsync();
            return rooms
                .GroupBy(r => r.RoomType)
                .ToDictionary(g => g.Key, g => g.Count());
        }
    }
}
