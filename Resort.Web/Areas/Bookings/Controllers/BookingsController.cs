using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Resort.Business.Interfaces;
using Resort.Model.Models;
using Resort.Web.Areas.Bookings.Models;
using Resort.Web.Controllers;

namespace Resort.Web.Areas.Bookings.Controllers
{
    [Area("Bookings")]
    [Authorize(Roles = "Admin,Manager,Receptionist")]
    public class BookingsController : BaseController
    {
        private readonly IBookingOperations _bookingOps;
        private readonly IRoomOperations _roomOps;
        private readonly IGuestOperations _guestOps;
        private readonly IServiceOperations _serviceOps;

        public BookingsController(
            IBookingOperations bookingOps,
            IRoomOperations roomOps,
            IGuestOperations guestOps,
            IServiceOperations serviceOps)
        {
            _bookingOps = bookingOps;
            _roomOps = roomOps;
            _guestOps = guestOps;
            _serviceOps = serviceOps;
        }

        public async Task<IActionResult> Index(string? status)
        {
            var bookings = string.IsNullOrEmpty(status)
                ? await _bookingOps.GetAllBookingsAsync()
                : await _bookingOps.GetBookingsByStatusAsync(status);

            var allGuests = await _guestOps.GetAllGuestsAsync();
            var allRooms = await _roomOps.GetAllRoomsAsync();

            ViewBag.GuestMap = allGuests.ToDictionary(g => g.Id, g => g.FullName);
            ViewBag.RoomMap = allRooms.ToDictionary(r => r.Id, r => r.Code + " - " + r.Name);
            ViewBag.FilterStatus = status;

            return View(bookings);
        }

        public async Task<IActionResult> Create()
        {
            var guests = await _guestOps.GetAllGuestsAsync();
            var rooms = await _roomOps.GetAllRoomsAsync();
            ViewBag.Guests = guests;
            ViewBag.Rooms = rooms.Where(r => r.Status == RoomStatus.Available).ToList();

            // Auto-generate Code gợi ý
            var allBookings = await _bookingOps.GetAllBookingsAsync();
            var nextCode = $"DP{(allBookings.Count + 1):D3}";

            return View(new Booking
            {
                Code = nextCode,
                CheckInDate = DateTime.Today,
                CheckOutDate = DateTime.Today.AddDays(1)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Booking booking)
        {
            // Auto-generate Code nếu bỏ trống
            if (string.IsNullOrWhiteSpace(booking.Code))
            {
                var allBookings = await _bookingOps.GetAllBookingsAsync();
                booking.Code = $"DP{(allBookings.Count + 1):D3}";
                ModelState.Remove(nameof(booking.Code));
            }

            // Validate ngày
            if (booking.CheckOutDate <= booking.CheckInDate)
            {
                ModelState.AddModelError(nameof(booking.CheckOutDate), "Ngày check-out phải sau ngày check-in.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Guests = await _guestOps.GetAllGuestsAsync();
                ViewBag.Rooms = (await _roomOps.GetAllRoomsAsync()).Where(r => r.Status == RoomStatus.Available).ToList();
                return View(booking);
            }

            booking.CreatedBy = CurrentUserEmail;
            var success = await _bookingOps.CreateBookingAsync(booking);
            if (success)
            {
                TempData["Success"] = "Tạo đặt phòng thành công!";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(string.Empty, "Tạo đặt phòng thất bại. Vui lòng kiểm tra lại thông tin phòng.");
            ViewBag.Guests = await _guestOps.GetAllGuestsAsync();
            ViewBag.Rooms = (await _roomOps.GetAllRoomsAsync()).Where(r => r.Status == RoomStatus.Available).ToList();
            return View(booking);
        }

        public async Task<IActionResult> Details(string id)
        {
            var booking = await _bookingOps.GetBookingByIdAsync(id);
            if (booking is null) return NotFound();

            var guest = await _guestOps.GetGuestByIdAsync(booking.GuestId);
            var room = await _roomOps.GetRoomByIdAsync(booking.RoomId);
            var bookingServices = await _bookingOps.GetBookingServicesAsync(id);
            var allServices = await _serviceOps.GetActiveServicesAsync();

            var serviceMap = allServices.ToDictionary(s => s.Id, s => s.Name);

            var vm = new BookingDetailsViewModel
            {
                Booking = booking,
                Guest = guest,
                Room = room,
                BookingServices = bookingServices,
                AvailableServices = allServices
            };

            ViewBag.ServiceMap = serviceMap;
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckIn(string id)
        {
            var success = await _bookingOps.CheckInAsync(id, CurrentUserEmail);
            TempData[success ? "Success" : "Error"] = success ? "Check-in thành công!" : "Check-in thất bại.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckOut(string id)
        {
            var success = await _bookingOps.CheckOutAsync(id, CurrentUserEmail);
            TempData[success ? "Success" : "Error"] = success ? "Check-out thành công!" : "Check-out thất bại.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(string id)
        {
            await _bookingOps.CancelBookingAsync(id, CurrentUserEmail);
            TempData["Success"] = "Hủy đặt phòng thành công!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddService(string bookingId, string serviceId, int quantity)
        {
            var service = await _serviceOps.GetServiceByIdAsync(serviceId);
            if (service is null) return BadRequest();

            var bs = new BookingService
            {
                BookingId = bookingId,
                ServiceId = serviceId,
                Quantity = quantity,
                UnitPrice = service.Price,
                ServiceDate = DateTime.UtcNow,
                CreatedBy = CurrentUserEmail,
                UpdatedBy = CurrentUserEmail
            };

            await _bookingOps.AddServiceToBookingAsync(bs);
            TempData["Success"] = "Thêm dịch vụ thành công!";
            return RedirectToAction(nameof(Details), new { id = bookingId });
        }
    }
}
