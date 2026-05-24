using Resort.Model.Models;

namespace Resort.Business.Interfaces
{
    // ── ROOMS ────────────────────────────────────────────────────────────────
    public interface IRoomOperations
    {
        Task<List<Room>> GetAllRoomsAsync();
        Task<Room?> GetRoomByIdAsync(string id);
        Task<List<Room>> GetRoomsByStatusAsync(string status);
        Task<List<Room>> GetRoomsByTypeAsync(string roomType);
        Task<bool> CreateRoomAsync(Room room);
        Task<bool> UpdateRoomAsync(Room room);
        Task<bool> DeleteRoomAsync(string id);
        Task<bool> UpdateRoomStatusAsync(string roomId, string status, string updatedBy);
        Task<List<Room>> GetAvailableRoomsAsync(DateTime checkIn, DateTime checkOut);
    }

    // ── GUESTS ───────────────────────────────────────────────────────────────
    public interface IGuestOperations
    {
        Task<List<Guest>> GetAllGuestsAsync();
        Task<Guest?> GetGuestByIdAsync(string id);
        Task<List<Guest>> SearchGuestsAsync(string keyword);
        Task<List<Guest>> GetGuestsByNationalityAsync(string nationality);
        Task<bool> CreateGuestAsync(Guest guest);
        Task<bool> UpdateGuestAsync(Guest guest);
        Task<bool> DeleteGuestAsync(string id);
    }

    // ── BOOKINGS ─────────────────────────────────────────────────────────────
    public interface IBookingOperations
    {
        Task<List<Booking>> GetAllBookingsAsync();
        Task<Booking?> GetBookingByIdAsync(string id);
        Task<List<Booking>> GetBookingsByStatusAsync(string status);
        Task<List<Booking>> GetBookingsByGuestIdAsync(string guestId);
        Task<List<Booking>> GetBookingsByDateRangeAsync(DateTime from, DateTime to);
        Task<List<Booking>> GetTodayCheckInsAsync();
        Task<List<Booking>> GetTodayCheckOutsAsync();
        Task<bool> CreateBookingAsync(Booking booking);
        Task<bool> UpdateBookingAsync(Booking booking);
        Task<bool> CheckInAsync(string bookingId, string updatedBy);
        Task<bool> CheckOutAsync(string bookingId, string updatedBy);
        Task<bool> CancelBookingAsync(string bookingId, string updatedBy);
        Task<bool> AddServiceToBookingAsync(BookingService bookingService);
        Task<List<BookingService>> GetBookingServicesAsync(string bookingId);
    }

    // ── SERVICES ─────────────────────────────────────────────────────────────
    public interface IServiceOperations
    {
        Task<List<Service>> GetAllServicesAsync();
        Task<Service?> GetServiceByIdAsync(string id);
        Task<List<Service>> GetActiveServicesAsync();
        Task<List<Service>> GetServicesByTypeAsync(string serviceType);
        Task<bool> CreateServiceAsync(Service service);
        Task<bool> UpdateServiceAsync(Service service);
        Task<bool> DeleteServiceAsync(string id);
        Task<bool> ToggleServiceStatusAsync(string id, string updatedBy);
    }

    // ── REPAIRS ──────────────────────────────────────────────────────────────
    public interface IRepairOperations
    {
        Task<List<RepairRequest>> GetAllRepairRequestsAsync();
        Task<RepairRequest?> GetRepairRequestByIdAsync(string id);
        Task<List<RepairRequest>> GetRepairRequestsByStatusAsync(string status);
        Task<List<RepairRequest>> GetRepairRequestsByTechnicianAsync(string technicianId);
        Task<bool> CreateRepairRequestAsync(RepairRequest request);
        Task<bool> UpdateRepairRequestAsync(RepairRequest request);
        Task<bool> AssignTechnicianAsync(string requestId, string technicianId, string updatedBy);
        Task<bool> UpdateRepairStatusAsync(string requestId, string status, string updatedBy);
    }

    // ── STAFF ─────────────────────────────────────────────────────────────────
    public interface IStaffOperations
    {
        Task<List<Staff>> GetAllStaffAsync();
        Task<Staff?> GetStaffByIdAsync(string id);
        Task<Staff?> GetStaffByUserIdAsync(string userId);
        Task<List<Staff>> GetStaffByDepartmentAsync(string department);
        Task<bool> CreateStaffAsync(Staff staff);
        Task<bool> UpdateStaffAsync(Staff staff);
        Task<bool> DeactivateStaffAsync(string id, string updatedBy);
    }

    // ── NOTIFICATIONS ─────────────────────────────────────────────────────────
    public interface INotificationOperations
    {
        Task<List<Notification>> GetNotificationsForUserAsync(string userId);
        Task<int> GetUnreadCountAsync(string userId);
        Task<bool> CreateNotificationAsync(Notification notification);
        Task<bool> MarkAsReadAsync(string notificationId, string userId);
        Task<bool> MarkAllAsReadAsync(string userId);
        Task<bool> BroadcastNotificationAsync(Notification notification);
    }

    // ── MASTER DATA ───────────────────────────────────────────────────────────
    public interface IMasterDataOperations
    {
        Task<List<MasterDataKey>> GetAllMasterKeysAsync();
        Task<List<MasterDataValue>> GetAllMasterValuesByKeyAsync(string key);
        Task<bool> InsertMasterKeyAsync(MasterDataKey key);
        Task<bool> UpdateMasterKeyAsync(string id, MasterDataKey key);
        Task<bool> InsertMasterValueAsync(MasterDataValue value);
        Task<bool> UpdateMasterValueAsync(string id, MasterDataValue value);
        Task<bool> UploadBulkMasterDataAsync(List<MasterDataValue> values);
    }

    // ── REPORTS ───────────────────────────────────────────────────────────────
    public interface IReportOperations
    {
        Task<decimal> GetMonthlyRevenueAsync(int year, int month);
        Task<List<(int Month, decimal Revenue)>> GetYearlyRevenueAsync(int year);
        Task<int> GetOccupancyCountAsync(DateTime date);
        Task<Dictionary<string, int>> GetRoomTypeDistributionAsync();
    }

    // ── MASTER DATA CACHE ─────────────────────────────────────────────────────
    public interface IMasterDataCacheOperations
    {
        Task<Dictionary<string, List<string>>> GetMasterDataCacheAsync();
        Task CreateMasterDataCacheAsync();
    }
}
