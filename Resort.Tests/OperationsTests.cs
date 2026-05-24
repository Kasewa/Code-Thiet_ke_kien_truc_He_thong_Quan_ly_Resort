using Microsoft.EntityFrameworkCore;
using Resort.Business;
using Resort.DataAccess;
using Resort.Model.Models;
using Xunit;

namespace Resort.Tests
{
    /// <summary>
    /// Unit tests cho RoomOperations sử dụng InMemory database.
    /// </summary>
    public class RoomOperationsTests : IDisposable
    {
        private readonly ApplicationTestDbContext _context;
        private readonly UnitOfWork _unitOfWork;
        private readonly RoomOperations _roomOperations;

        public RoomOperationsTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationTestDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationTestDbContext(options);
            _unitOfWork = new UnitOfWork(_context);
            _roomOperations = new RoomOperations(_unitOfWork);
        }

        private Room CreateSampleRoom(string code = "P001", string status = "Available") => new()
        {
            Id = Guid.NewGuid().ToString(),
            Code = code,
            Name = "Test Room",
            RoomType = "Standard",
            PricePerNight = 500000,
            Capacity = 2,
            Status = status,
            CreatedBy = "test",
            CreatedDate = DateTime.UtcNow,
            UpdatedBy = "test",
            UpdatedDate = DateTime.UtcNow
        };

        [Fact]
        public async Task CreateRoomAsync_ShouldAddRoom()
        {
            var room = CreateSampleRoom();
            var result = await _roomOperations.CreateRoomAsync(room);

            Assert.True(result);
            var all = await _roomOperations.GetAllRoomsAsync();
            Assert.Single(all);
            Assert.Equal("P001", all[0].Code);
        }

        [Fact]
        public async Task GetAllRoomsAsync_ShouldReturnAllRooms()
        {
            await _roomOperations.CreateRoomAsync(CreateSampleRoom("P001"));
            await _roomOperations.CreateRoomAsync(CreateSampleRoom("P002"));

            var result = await _roomOperations.GetAllRoomsAsync();

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetRoomsByStatusAsync_ShouldFilterByStatus()
        {
            await _roomOperations.CreateRoomAsync(CreateSampleRoom("P001", "Available"));
            await _roomOperations.CreateRoomAsync(CreateSampleRoom("P002", "Occupied"));
            await _roomOperations.CreateRoomAsync(CreateSampleRoom("P003", "Available"));

            var available = await _roomOperations.GetRoomsByStatusAsync("Available");

            Assert.Equal(2, available.Count);
            Assert.All(available, r => Assert.Equal("Available", r.Status));
        }

        [Fact]
        public async Task UpdateRoomStatusAsync_ShouldChangeStatus()
        {
            var room = CreateSampleRoom("P001", "Available");
            await _roomOperations.CreateRoomAsync(room);

            var result = await _roomOperations.UpdateRoomStatusAsync(room.Id, "Occupied", "tester");

            Assert.True(result);
            var updated = await _roomOperations.GetRoomByIdAsync(room.Id);
            Assert.Equal("Occupied", updated?.Status);
        }

        [Fact]
        public async Task DeleteRoomAsync_ShouldRemoveRoom()
        {
            var room = CreateSampleRoom();
            await _roomOperations.CreateRoomAsync(room);

            var result = await _roomOperations.DeleteRoomAsync(room.Id);

            Assert.True(result);
            var all = await _roomOperations.GetAllRoomsAsync();
            Assert.Empty(all);
        }

        [Fact]
        public async Task GetRoomByIdAsync_WithInvalidId_ShouldReturnNull()
        {
            var result = await _roomOperations.GetRoomByIdAsync("nonexistent-id");
            Assert.Null(result);
        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
            _context.Dispose();
        }
    }

    /// <summary>
    /// Unit tests cho BookingOperations.
    /// </summary>
    public class BookingOperationsTests : IDisposable
    {
        private readonly ApplicationTestDbContext _context;
        private readonly UnitOfWork _unitOfWork;
        private readonly BookingOperations _bookingOperations;

        public BookingOperationsTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationTestDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationTestDbContext(options);
            _unitOfWork = new UnitOfWork(_context);
            _bookingOperations = new BookingOperations(_unitOfWork);
        }

        private Room SeedRoom()
        {
            var room = new Room
            {
                Id = Guid.NewGuid().ToString(),
                Code = "P001",
                Name = "Test Room",
                RoomType = "Standard",
                PricePerNight = 500000,
                Capacity = 2,
                Status = "Available",
                CreatedBy = "test",
                CreatedDate = DateTime.UtcNow,
                UpdatedBy = "test",
                UpdatedDate = DateTime.UtcNow
            };
            _context.Rooms.Add(room);
            _context.SaveChanges();
            return room;
        }

        private Guest SeedGuest()
        {
            var guest = new Guest
            {
                Id = Guid.NewGuid().ToString(),
                Code = "KH001",
                FullName = "Test Guest",
                IdCard = "012345678901",
                CreatedBy = "test",
                CreatedDate = DateTime.UtcNow,
                UpdatedBy = "test",
                UpdatedDate = DateTime.UtcNow
            };
            _context.Guests.Add(guest);
            _context.SaveChanges();
            return guest;
        }

        [Fact]
        public async Task CreateBookingAsync_ShouldCalculateTotalAmount()
        {
            var room = SeedRoom();
            var guest = SeedGuest();

            var booking = new Booking
            {
                Id = Guid.NewGuid().ToString(),
                Code = "DP001",
                GuestId = guest.Id,
                RoomId = room.Id,
                CheckInDate = DateTime.Today,
                CheckOutDate = DateTime.Today.AddDays(3),
                NumberOfGuests = 2,
                CreatedBy = "test",
                UpdatedBy = "test"
            };

            var result = await _bookingOperations.CreateBookingAsync(booking);

            Assert.True(result);
            var saved = await _bookingOperations.GetBookingByIdAsync(booking.Id);
            // 500000 * 3 nights = 1,500,000
            Assert.Equal(1_500_000m, saved?.TotalAmount);
        }

        [Fact]
        public async Task CheckInAsync_ShouldUpdateStatusAndRoomOccupied()
        {
            var room = SeedRoom();
            var guest = SeedGuest();

            var booking = new Booking
            {
                Id = Guid.NewGuid().ToString(),
                Code = "DP001",
                GuestId = guest.Id,
                RoomId = room.Id,
                CheckInDate = DateTime.Today,
                CheckOutDate = DateTime.Today.AddDays(2),
                Status = "Confirmed",
                CreatedBy = "test",
                UpdatedBy = "test"
            };
            _context.Bookings.Add(booking);
            _context.SaveChanges();

            var result = await _bookingOperations.CheckInAsync(booking.Id, "receptionist");

            Assert.True(result);
            var updated = await _bookingOperations.GetBookingByIdAsync(booking.Id);
            Assert.Equal("CheckedIn", updated?.Status);
            Assert.NotNull(updated?.ActualCheckIn);
        }

        [Fact]
        public async Task CancelBookingAsync_ShouldSetStatusCancelled()
        {
            var room = SeedRoom();
            var guest = SeedGuest();

            var booking = new Booking
            {
                Id = Guid.NewGuid().ToString(),
                Code = "DP001",
                GuestId = guest.Id,
                RoomId = room.Id,
                CheckInDate = DateTime.Today,
                CheckOutDate = DateTime.Today.AddDays(2),
                Status = "Pending",
                CreatedBy = "test",
                UpdatedBy = "test"
            };
            _context.Bookings.Add(booking);
            _context.SaveChanges();

            var result = await _bookingOperations.CancelBookingAsync(booking.Id, "manager");

            Assert.True(result);
            var updated = await _bookingOperations.GetBookingByIdAsync(booking.Id);
            Assert.Equal("Cancelled", updated?.Status);
        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
            _context.Dispose();
        }
    }
}
