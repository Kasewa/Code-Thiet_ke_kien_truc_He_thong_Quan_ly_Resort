using Resort.Business.Interfaces;
using Resort.DataAccess;
using Resort.Model.Models;

namespace Resort.Business
{
    public class GuestOperations : IGuestOperations
    {
        private readonly IUnitOfWork _unitOfWork;

        public GuestOperations(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<Guest>> GetAllGuestsAsync()
        {
            var guests = await _unitOfWork.Repository<Guest>().GetAllAsync();
            return guests.OrderBy(g => g.FullName).ToList();
        }

        public async Task<Guest?> GetGuestByIdAsync(string id)
        {
            return await _unitOfWork.Repository<Guest>().GetByIdAsync(id);
        }

        public async Task<List<Guest>> SearchGuestsAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return await GetAllGuestsAsync();

            var lower = keyword.ToLower();
            var guests = await _unitOfWork.Repository<Guest>().FindAllAsync(g =>
                g.FullName.ToLower().Contains(lower) ||
                g.IdCard.Contains(keyword) ||
                g.Phone.Contains(keyword) ||
                g.Email.ToLower().Contains(lower));

            return guests.OrderBy(g => g.FullName).ToList();
        }

        public async Task<List<Guest>> GetGuestsByNationalityAsync(string nationality)
        {
            var guests = await _unitOfWork.Repository<Guest>().FindAllAsync(g => g.Nationality == nationality);
            return guests.OrderBy(g => g.FullName).ToList();
        }

        public async Task<bool> CreateGuestAsync(Guest guest)
        {
            guest.Id = Guid.NewGuid().ToString();
            guest.CreatedDate = DateTime.UtcNow;
            await _unitOfWork.Repository<Guest>().AddAsync(guest);
            return await _unitOfWork.CommitAsync() > 0;
        }

        public async Task<bool> UpdateGuestAsync(Guest guest)
        {
            var existing = await _unitOfWork.Repository<Guest>().GetByIdAsync(guest.Id);
            if (existing is null) return false;

            existing.Code = guest.Code;
            existing.FullName = guest.FullName;
            existing.IdCard = guest.IdCard;
            existing.Nationality = guest.Nationality;
            existing.Gender = guest.Gender;
            existing.DateOfBirth = guest.DateOfBirth;
            existing.Phone = guest.Phone;
            existing.Email = guest.Email;
            existing.Address = guest.Address;
            existing.Notes = guest.Notes;
            existing.UpdatedBy = guest.UpdatedBy;
            existing.UpdatedDate = DateTime.UtcNow;

            await _unitOfWork.Repository<Guest>().UpdateAsync(existing);
            return await _unitOfWork.CommitAsync() > 0;
        }

        public async Task<bool> DeleteGuestAsync(string id)
        {
            var guest = await _unitOfWork.Repository<Guest>().GetByIdAsync(id);
            if (guest is null) return false;

            await _unitOfWork.Repository<Guest>().DeleteAsync(guest);
            return await _unitOfWork.CommitAsync() > 0;
        }
    }
}
