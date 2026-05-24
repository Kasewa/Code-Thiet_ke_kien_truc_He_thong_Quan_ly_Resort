using Resort.Model.Models;

namespace Resort.Web.Areas.Bookings.Models
{
    public class BookingDetailsViewModel
    {
        public Booking Booking { get; set; } = null!;
        public Guest? Guest { get; set; }
        public Room? Room { get; set; }
        public List<BookingService> BookingServices { get; set; } = new();
        public List<Service> AvailableServices { get; set; } = new();

        public decimal RoomTotal => Room is null ? 0
            : Room.PricePerNight * Math.Max(1, (int)(Booking.CheckOutDate - Booking.CheckInDate).TotalDays);

        public decimal ServicesTotal => BookingServices.Sum(s => s.TotalPrice);

        public decimal GrandTotal => RoomTotal + ServicesTotal;
    }
}
