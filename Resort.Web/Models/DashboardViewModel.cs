using Resort.Model.Models;

namespace Resort.Web.Models
{
    public class DashboardViewModel
    {
        public int TotalRooms { get; set; }
        public int OccupiedRooms { get; set; }
        public int AvailableRooms { get; set; }
        public int MaintenanceRooms { get; set; }
        public int TodayCheckIns { get; set; }
        public int TodayCheckOuts { get; set; }
        public int UnreadNotifications { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public List<decimal> YearlyRevenue { get; set; } = new();
        public List<Booking> RecentCheckIns { get; set; } = new();
        public List<Booking> RecentCheckOuts { get; set; } = new();

        public double OccupancyRate =>
            TotalRooms > 0 ? Math.Round((double)OccupiedRooms / TotalRooms * 100, 1) : 0;
    }
}
