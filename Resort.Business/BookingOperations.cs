using Resort.Business.Interfaces;
using Resort.DataAccess;
using Resort.Model.Models;

namespace Resort.Business
{
    public class BookingOperations : IBookingOperations
    {
        private readonly IUnitOfWork _unitOfWork;

        public BookingOperations(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<Booking>> GetAllBookingsAsync()
        {
            var bookings = await _unitOfWork.Repository<Booking>().GetAllAsync();
            return bookings.OrderByDescending(b => b.CreatedDate).ToList();
        }

        public async Task<Booking?> GetBookingByIdAsync(string id)
        {
            return await _unitOfWork.Repository<Booking>().GetByIdAsync(id);
        }

        public async Task<List<Booking>> GetBookingsByStatusAsync(string status)
        {
            var bookings = await _unitOfWork.Repository<Booking>().FindAllAsync(b => b.Status == status);
            return bookings.OrderByDescending(b => b.CheckInDate).ToList();
        }

        public async Task<List<Booking>> GetBookingsByGuestIdAsync(string guestId)
        {
            var bookings = await _unitOfWork.Repository<Booking>().FindAllAsync(b => b.GuestId == guestId);
            return bookings.OrderByDescending(b => b.CreatedDate).ToList();
        }

        public async Task<List<Booking>> GetBookingsByDateRangeAsync(DateTime from, DateTime to)
        {
            var bookings = await _unitOfWork.Repository<Booking>()
                .FindAllAsync(b => b.CheckInDate >= from && b.CheckInDate <= to);
            return bookings.OrderBy(b => b.CheckInDate).ToList();
        }

        public async Task<List<Booking>> GetTodayCheckInsAsync()
        {
            var today = DateTime.UtcNow.Date;
            var bookings = await _unitOfWork.Repository<Booking>()
                .FindAllAsync(b =>
                    b.CheckInDate.Date == today &&
                    (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Pending));
            return bookings.ToList();
        }

        public async Task<List<Booking>> GetTodayCheckOutsAsync()
        {
            var today = DateTime.UtcNow.Date;
            var bookings = await _unitOfWork.Repository<Booking>()
                .FindAllAsync(b =>
                    b.CheckOutDate.Date == today &&
                    b.Status == BookingStatus.CheckedIn);
            return bookings.ToList();
        }

        public async Task<bool> CreateBookingAsync(Booking booking)
        {
            booking.Id = Guid.NewGuid().ToString();
            booking.CreatedDate = DateTime.UtcNow;
            booking.Status = BookingStatus.Pending;

            // Calculate total amount
            var room = await _unitOfWork.Repository<Room>().GetByIdAsync(booking.RoomId);
            if (room is null) return false;

            var nights = (booking.CheckOutDate - booking.CheckInDate).Days;
            booking.TotalAmount = room.PricePerNight * nights;

            await _unitOfWork.Repository<Booking>().AddAsync(booking);
            return await _unitOfWork.CommitAsync() > 0;
        }

        public async Task<bool> UpdateBookingAsync(Booking booking)
        {
            var existing = await _unitOfWork.Repository<Booking>().GetByIdAsync(booking.Id);
            if (existing is null) return false;

            existing.CheckInDate = booking.CheckInDate;
            existing.CheckOutDate = booking.CheckOutDate;
            existing.NumberOfGuests = booking.NumberOfGuests;
            existing.Deposit = booking.Deposit;
            existing.Notes = booking.Notes;
            existing.UpdatedBy = booking.UpdatedBy;
            existing.UpdatedDate = DateTime.UtcNow;

            // Recalculate total
            var room = await _unitOfWork.Repository<Room>().GetByIdAsync(existing.RoomId);
            if (room is not null)
            {
                var nights = (existing.CheckOutDate - existing.CheckInDate).Days;
                existing.TotalAmount = room.PricePerNight * nights;
            }

            await _unitOfWork.Repository<Booking>().UpdateAsync(existing);
            return await _unitOfWork.CommitAsync() > 0;
        }

        public async Task<bool> CheckInAsync(string bookingId, string updatedBy)
        {
            var booking = await _unitOfWork.Repository<Booking>().GetByIdAsync(bookingId);
            if (booking is null) return false;

            booking.Status = BookingStatus.CheckedIn;
            booking.ActualCheckIn = DateTime.UtcNow;
            booking.UpdatedBy = updatedBy;
            booking.UpdatedDate = DateTime.UtcNow;

            await _unitOfWork.Repository<Booking>().UpdateAsync(booking);

            // Update room status to Occupied
            await UpdateRoomStatusAsync(booking.RoomId, RoomStatus.Occupied, updatedBy);

            return await _unitOfWork.CommitAsync() > 0;
        }

        public async Task<bool> CheckOutAsync(string bookingId, string updatedBy)
        {
            var booking = await _unitOfWork.Repository<Booking>().GetByIdAsync(bookingId);
            if (booking is null) return false;

            // Add extra services to total
            var services = await _unitOfWork.Repository<BookingService>()
                .FindAllAsync(s => s.BookingId == bookingId);
            var extraServiceTotal = services.Sum(s => s.TotalPrice);

            var room = await _unitOfWork.Repository<Room>().GetByIdAsync(booking.RoomId);
            if (room is not null)
            {
                var nights = (booking.CheckOutDate - booking.CheckInDate).Days;
                booking.TotalAmount = (room.PricePerNight * nights) + extraServiceTotal;
            }

            booking.Status = BookingStatus.CheckedOut;
            booking.ActualCheckOut = DateTime.UtcNow;
            booking.UpdatedBy = updatedBy;
            booking.UpdatedDate = DateTime.UtcNow;

            await _unitOfWork.Repository<Booking>().UpdateAsync(booking);

            // Update room status to Cleaning
            await UpdateRoomStatusAsync(booking.RoomId, RoomStatus.Cleaning, updatedBy);

            return await _unitOfWork.CommitAsync() > 0;
        }

        public async Task<bool> CancelBookingAsync(string bookingId, string updatedBy)
        {
            var booking = await _unitOfWork.Repository<Booking>().GetByIdAsync(bookingId);
            if (booking is null) return false;

            booking.Status = BookingStatus.Cancelled;
            booking.UpdatedBy = updatedBy;
            booking.UpdatedDate = DateTime.UtcNow;

            await _unitOfWork.Repository<Booking>().UpdateAsync(booking);
            return await _unitOfWork.CommitAsync() > 0;
        }

        public async Task<bool> AddServiceToBookingAsync(BookingService bookingService)
        {
            bookingService.Id = Guid.NewGuid().ToString();
            bookingService.TotalPrice = bookingService.UnitPrice * bookingService.Quantity;
            bookingService.CreatedDate = DateTime.UtcNow;

            await _unitOfWork.Repository<BookingService>().AddAsync(bookingService);
            return await _unitOfWork.CommitAsync() > 0;
        }

        public async Task<List<BookingService>> GetBookingServicesAsync(string bookingId)
        {
            var services = await _unitOfWork.Repository<BookingService>()
                .FindAllAsync(s => s.BookingId == bookingId);
            return services.OrderBy(s => s.ServiceDate).ToList();
        }

        private async Task UpdateRoomStatusAsync(string roomId, string status, string updatedBy)
        {
            var room = await _unitOfWork.Repository<Room>().GetByIdAsync(roomId);
            if (room is null) return;
            room.Status = status;
            room.UpdatedBy = updatedBy;
            room.UpdatedDate = DateTime.UtcNow;
            await _unitOfWork.Repository<Room>().UpdateAsync(room);
        }
    }
}
