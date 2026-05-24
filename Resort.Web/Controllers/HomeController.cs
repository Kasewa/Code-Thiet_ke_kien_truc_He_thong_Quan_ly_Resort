using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Resort.Business.Interfaces;
using Resort.Web.Controllers;
using Resort.Web.Models;

namespace Resort.Web.Controllers
{
    [Authorize]
    public class HomeController : BaseController
    {
        private readonly IRoomOperations _roomOps;
        private readonly IBookingOperations _bookingOps;
        private readonly IReportOperations _reportOps;
        private readonly INotificationOperations _notificationOps;

        public HomeController(
            IRoomOperations roomOps,
            IBookingOperations bookingOps,
            IReportOperations reportOps,
            INotificationOperations notificationOps)
        {
            _roomOps = roomOps;
            _bookingOps = bookingOps;
            _reportOps = reportOps;
            _notificationOps = notificationOps;
        }

        public async Task<IActionResult> Index()
        {
            var allRooms = await _roomOps.GetAllRoomsAsync();
            var todayCheckIns = await _bookingOps.GetTodayCheckInsAsync();
            var todayCheckOuts = await _bookingOps.GetTodayCheckOutsAsync();
            var unreadCount = await _notificationOps.GetUnreadCountAsync(CurrentUserId);
            var monthlyRevenue = await _reportOps.GetMonthlyRevenueAsync(DateTime.Now.Year, DateTime.Now.Month);
            var yearlyRevenue = await _reportOps.GetYearlyRevenueAsync(DateTime.Now.Year);

            var vm = new DashboardViewModel
            {
                TotalRooms = allRooms.Count,
                OccupiedRooms = allRooms.Count(r => r.Status == Resort.Model.Models.RoomStatus.Occupied),
                AvailableRooms = allRooms.Count(r => r.Status == Resort.Model.Models.RoomStatus.Available),
                MaintenanceRooms = allRooms.Count(r => r.Status == Resort.Model.Models.RoomStatus.Maintenance),
                TodayCheckIns = todayCheckIns.Count,
                TodayCheckOuts = todayCheckOuts.Count,
                UnreadNotifications = unreadCount,
                MonthlyRevenue = monthlyRevenue,
                YearlyRevenue = yearlyRevenue.Select(x => x.Revenue).ToList(),
                RecentCheckIns = todayCheckIns.Take(5).ToList(),
                RecentCheckOuts = todayCheckOuts.Take(5).ToList(),
            };

            return View(vm);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}
