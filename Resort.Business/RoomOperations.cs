using Resort.Business.Interfaces;
using Resort.DataAccess;
using Resort.Model.Models;

namespace Resort.Business
{
    public class RoomOperations : IRoomOperations
    {
        private readonly IUnitOfWork _unitOfWork;

        public RoomOperations(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<Room>> GetAllRoomsAsync()
        {
            var rooms = await _unitOfWork.Repository<Room>().GetAllAsync();
            return rooms.OrderBy(r => r.Code).ToList();
        }

        public async Task<Room?> GetRoomByIdAsync(string id)
        {
            return await _unitOfWork.Repository<Room>().GetByIdAsync(id);
        }

        public async Task<List<Room>> GetRoomsByStatusAsync(string status)
        {
            var rooms = await _unitOfWork.Repository<Room>().FindAllAsync(r => r.Status == status);
            return rooms.OrderBy(r => r.Code).ToList();
        }

        public async Task<List<Room>> GetRoomsByTypeAsync(string roomType)
        {
            var rooms = await _unitOfWork.Repository<Room>().FindAllAsync(r => r.RoomType == roomType);
            return rooms.OrderBy(r => r.Code).ToList();
        }

        public async Task<bool> CreateRoomAsync(Room room)
        {
            room.Id = Guid.NewGuid().ToString();
            room.CreatedDate = DateTime.UtcNow;
            await _unitOfWork.Repository<Room>().AddAsync(room);
            return await _unitOfWork.CommitAsync() > 0;
        }

        public async Task<bool> UpdateRoomAsync(Room room)
        {
            var existing = await _unitOfWork.Repository<Room>().GetByIdAsync(room.Id);
            if (existing is null) return false;

            existing.Code = room.Code;
            existing.Name = room.Name;
            existing.RoomType = room.RoomType;
            existing.PricePerNight = room.PricePerNight;
            existing.Capacity = room.Capacity;
            existing.Floor = room.Floor;
            existing.Description = room.Description;
            existing.Amenities = room.Amenities;
            existing.Status = room.Status;
            existing.UpdatedBy = room.UpdatedBy;
            existing.UpdatedDate = DateTime.UtcNow;

            await _unitOfWork.Repository<Room>().UpdateAsync(existing);
            return await _unitOfWork.CommitAsync() > 0;
        }

        public async Task<bool> DeleteRoomAsync(string id)
        {
            var room = await _unitOfWork.Repository<Room>().GetByIdAsync(id);
            if (room is null) return false;

            await _unitOfWork.Repository<Room>().DeleteAsync(room);
            return await _unitOfWork.CommitAsync() > 0;
        }

        public async Task<bool> UpdateRoomStatusAsync(string roomId, string status, string updatedBy)
        {
            var room = await _unitOfWork.Repository<Room>().GetByIdAsync(roomId);
            if (room is null) return false;

            room.Status = status;
            room.UpdatedBy = updatedBy;
            room.UpdatedDate = DateTime.UtcNow;

            await _unitOfWork.Repository<Room>().UpdateAsync(room);
            return await _unitOfWork.CommitAsync() > 0;
        }

        public async Task<List<Room>> GetAvailableRoomsAsync(DateTime checkIn, DateTime checkOut)
        {
            // Get all rooms that are Available status
            var allAvailable = await _unitOfWork.Repository<Room>()
                .FindAllAsync(r => r.Status == RoomStatus.Available);

            // Get rooms with overlapping bookings
            var overlappingBookings = await _unitOfWork.Repository<Booking>()
                .FindAllAsync(b =>
                    b.Status != BookingStatus.Cancelled &&
                    b.Status != BookingStatus.CheckedOut &&
                    b.CheckInDate < checkOut &&
                    b.CheckOutDate > checkIn);

            var bookedRoomIds = overlappingBookings.Select(b => b.RoomId).ToHashSet();

            return allAvailable
                .Where(r => !bookedRoomIds.Contains(r.Id))
                .OrderBy(r => r.Code)
                .ToList();
        }
    }
}
