using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Resort.Business.Interfaces;
using Resort.Web.Controllers;

namespace Resort.Web.Areas.Reports.Controllers
{
    [Area("Reports")]
    [Authorize(Roles = "Admin,Manager")]
    public class ReportsController : BaseController
    {
        private readonly IReportOperations _reportOps;

        public ReportsController(IReportOperations reportOps)
        {
            _reportOps = reportOps;
        }

        public async Task<IActionResult> Index(int? year)
        {
            var targetYear = year ?? DateTime.Now.Year;
            var yearly = await _reportOps.GetYearlyRevenueAsync(targetYear);
            var roomDist = await _reportOps.GetRoomTypeDistributionAsync();

            // Pass as separate lists — avoids ValueTuple cast issues from ViewBag
            ViewBag.Year = targetYear;
            ViewBag.MonthLabels = Enumerable.Range(1, 12).Select(m => $"T{m}").ToList();
            ViewBag.RevenueValues = yearly.Select(x => x.Revenue).ToList();
            ViewBag.RoomTypeLabels = roomDist.Keys.ToList();
            ViewBag.RoomTypeCounts = roomDist.Values.ToList();
            ViewBag.TotalRevenue = yearly.Sum(x => x.Revenue);
            // For table
            ViewBag.MonthlyTable = yearly.Select(x => new { Month = x.Month, Revenue = x.Revenue }).ToList();

            return View();
        }
    }
}
