using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Resort.Model;
using Resort.Model.Models;
using Resort.Web.Configuration;

namespace Resort.Web.Data
{
    public interface IIdentitySeed
    {
        Task SeedAsync();
    }

    public class IdentitySeed : IIdentitySeed
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly ApplicationSettings _settings;

        public IdentitySeed(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context,
            IOptions<ApplicationSettings> options)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _settings = options.Value;
        }

        public async Task SeedAsync()
        {
            // Seed roles
            var roles = new[] { Constants.AdminRole, Constants.ManagerRole, Constants.ReceptionistRole, Constants.TechnicianRole };
            foreach (var role in roles)
                if (!await _roleManager.RoleExistsAsync(role))
                    await _roleManager.CreateAsync(new IdentityRole(role));

            // Seed default users
            await EnsureUserAsync(_settings.AdminEmail, _settings.AdminPassword, Constants.AdminRole);
            await EnsureUserAsync(_settings.ManagerEmail, _settings.ManagerPassword, Constants.ManagerRole);
            await EnsureUserAsync(_settings.ReceptionistEmail, _settings.ReceptionistPassword, Constants.ReceptionistRole);
            await EnsureUserAsync(_settings.TechnicianEmail, _settings.TechnicianPassword, Constants.TechnicianRole);

            // Seed master data
            await SeedMasterDataAsync();

            // Seed resort data
            await SeedRoomsAsync();
            await SeedGuestsAsync();
            await SeedServicesAsync();

            await _context.SaveChangesAsync();
        }

        private async Task EnsureUserAsync(string email, string password, string role)
        {
            if (string.IsNullOrWhiteSpace(email)) return;

            var user = await _userManager.FindByEmailAsync(email);
            if (user is null)
            {
                user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
                var result = await _userManager.CreateAsync(user, password);
                if (!result.Succeeded) return;
            }

            if (!await _userManager.IsInRoleAsync(user, role))
                await _userManager.AddToRoleAsync(user, role);
        }

        private async Task SeedMasterDataAsync()
        {
            await EnsureMasterKeyValuesAsync("RoomType", new[] { "Standard", "Deluxe", "Suite", "Villa" });
            await EnsureMasterKeyValuesAsync("RoomStatus", new[] { "Available", "Occupied", "Maintenance", "Cleaning" });
            await EnsureMasterKeyValuesAsync("ServiceType", new[] { "Ăn uống", "Chăm sóc", "Giải trí", "Vận chuyển", "Tiện ích" });
            await EnsureMasterKeyValuesAsync("RepairPriority", new[] { "Urgent", "Normal", "Low" });
            await EnsureMasterKeyValuesAsync("Department", new[] { "Quản lý", "Lễ tân", "Kỹ thuật", "Nhà hàng", "Spa", "Bảo vệ", "Buồng phòng" });
            await EnsureMasterKeyValuesAsync("Position", new[] { "Quản lý", "Trưởng nhóm", "Nhân viên lễ tân", "Kỹ thuật viên", "Nhân viên nhà hàng", "Nhân viên Spa", "Nhân viên bảo vệ" });
            await EnsureMasterKeyValuesAsync("Shift", new[] { "Ca sáng (6:00-14:00)", "Ca chiều (14:00-22:00)", "Ca đêm (22:00-6:00)" });
            await EnsureMasterKeyValuesAsync("Nationality", new[] { "Việt Nam", "Mỹ", "Trung Quốc", "Nhật Bản", "Hàn Quốc", "Pháp", "Đức", "Anh", "Úc", "Khác" });
        }

        private async Task EnsureMasterKeyValuesAsync(string keyName, string[] values)
        {
            var key = await _context.MasterDataKeys.FirstOrDefaultAsync(k => k.Key == keyName);
            if (key is null)
            {
                key = new MasterDataKey
                {
                    Id = Guid.NewGuid().ToString(),
                    Key = keyName,
                    Description = keyName,
                    IsActive = true,
                    CreatedBy = "System",
                    CreatedDate = DateTime.UtcNow,
                    UpdatedBy = "System",
                    UpdatedDate = DateTime.UtcNow
                };
                _context.MasterDataKeys.Add(key);
                await _context.SaveChangesAsync();
            }

            for (int i = 0; i < values.Length; i++)
            {
                var v = values[i];
                var exists = await _context.MasterDataValues.AnyAsync(mv => mv.MasterDataKeyId == key.Id && mv.Value == v);
                if (!exists)
                {
                    _context.MasterDataValues.Add(new MasterDataValue
                    {
                        Id = Guid.NewGuid().ToString(),
                        Value = v,
                        Description = v,
                        DisplayOrder = i + 1,
                        IsActive = true,
                        MasterDataKeyId = key.Id,
                        CreatedBy = "System",
                        CreatedDate = DateTime.UtcNow,
                        UpdatedBy = "System",
                        UpdatedDate = DateTime.UtcNow
                    });
                }
            }
        }

        private async Task SeedRoomsAsync()
        {
            if (await _context.Rooms.AnyAsync()) return;

            var rooms = new List<Room>
            {
                new() { Id = Guid.NewGuid().ToString(), Code = "P101", Name = "Phòng Biển Xanh", RoomType = "Standard", PricePerNight = 800000,   Capacity = 2, Floor = "1", Amenities = "WiFi,TV,Điều hòa,Mini bar",                Status = RoomStatus.Available,    CreatedBy = "System", CreatedDate = DateTime.UtcNow, UpdatedBy = "System", UpdatedDate = DateTime.UtcNow },
                new() { Id = Guid.NewGuid().ToString(), Code = "P102", Name = "Phòng Hướng Vườn", RoomType = "Standard", PricePerNight = 750000,  Capacity = 2, Floor = "1", Amenities = "WiFi,TV,Điều hòa",                              Status = RoomStatus.Occupied,     CreatedBy = "System", CreatedDate = DateTime.UtcNow, UpdatedBy = "System", UpdatedDate = DateTime.UtcNow },
                new() { Id = Guid.NewGuid().ToString(), Code = "P201", Name = "Phòng Deluxe Biển", RoomType = "Deluxe",  PricePerNight = 1500000, Capacity = 2, Floor = "2", Amenities = "WiFi,TV,Điều hòa,Bồn tắm,Ban công",             Status = RoomStatus.Available,    CreatedBy = "System", CreatedDate = DateTime.UtcNow, UpdatedBy = "System", UpdatedDate = DateTime.UtcNow },
                new() { Id = Guid.NewGuid().ToString(), Code = "P202", Name = "Phòng Deluxe Vườn", RoomType = "Deluxe",  PricePerNight = 1400000, Capacity = 2, Floor = "2", Amenities = "WiFi,TV,Điều hòa,Bồn tắm",                      Status = RoomStatus.Maintenance,  CreatedBy = "System", CreatedDate = DateTime.UtcNow, UpdatedBy = "System", UpdatedDate = DateTime.UtcNow },
                new() { Id = Guid.NewGuid().ToString(), Code = "P301", Name = "Suite Hoàng Gia",    RoomType = "Suite",   PricePerNight = 3000000, Capacity = 4, Floor = "3", Amenities = "WiFi,TV,Điều hòa,Bồn tắm,Phòng khách,Bếp nhỏ", Status = RoomStatus.Available,    CreatedBy = "System", CreatedDate = DateTime.UtcNow, UpdatedBy = "System", UpdatedDate = DateTime.UtcNow },
                new() { Id = Guid.NewGuid().ToString(), Code = "P302", Name = "Suite Gia Đình",     RoomType = "Suite",   PricePerNight = 2800000, Capacity = 4, Floor = "3", Amenities = "WiFi,TV,Điều hòa,Bồn tắm,Phòng khách",         Status = RoomStatus.Available,    CreatedBy = "System", CreatedDate = DateTime.UtcNow, UpdatedBy = "System", UpdatedDate = DateTime.UtcNow },
                new() { Id = Guid.NewGuid().ToString(), Code = "V001", Name = "Villa Bãi Biển",     RoomType = "Villa",   PricePerNight = 8000000, Capacity = 6, Floor = "G", Amenities = "WiFi,TV,Điều hòa,Hồ bơi riêng,Bếp,BBQ",       Status = RoomStatus.Available,    CreatedBy = "System", CreatedDate = DateTime.UtcNow, UpdatedBy = "System", UpdatedDate = DateTime.UtcNow },
                new() { Id = Guid.NewGuid().ToString(), Code = "V002", Name = "Villa Rừng Dừa",     RoomType = "Villa",   PricePerNight = 7500000, Capacity = 6, Floor = "G", Amenities = "WiFi,TV,Điều hòa,Hồ bơi riêng,Bếp",           Status = RoomStatus.Occupied,     CreatedBy = "System", CreatedDate = DateTime.UtcNow, UpdatedBy = "System", UpdatedDate = DateTime.UtcNow },
            };

            _context.Rooms.AddRange(rooms);
        }

        private async Task SeedGuestsAsync()
        {
            if (await _context.Guests.AnyAsync()) return;

            var guests = new List<Guest>
            {
                new() { Id = Guid.NewGuid().ToString(), Code = "KH001", FullName = "Nguyễn Văn An",    IdCard = "079199001234", Nationality = "Việt Nam", Gender = "Nam", DateOfBirth = new DateTime(1990, 5, 15), Phone = "0912345678", Email = "nvan@email.com",  Address = "Hà Nội",    CreatedBy = "System", CreatedDate = DateTime.UtcNow, UpdatedBy = "System", UpdatedDate = DateTime.UtcNow },
                new() { Id = Guid.NewGuid().ToString(), Code = "KH002", FullName = "Trần Thị Bích",    IdCard = "079199002345", Nationality = "Việt Nam", Gender = "Nữ",  DateOfBirth = new DateTime(1995, 8, 22), Phone = "0923456789", Email = "ttbich@email.com", Address = "TP.HCM",    CreatedBy = "System", CreatedDate = DateTime.UtcNow, UpdatedBy = "System", UpdatedDate = DateTime.UtcNow },
                new() { Id = Guid.NewGuid().ToString(), Code = "KH003", FullName = "James Wilson",      IdCard = "US12345678",   Nationality = "Mỹ",      Gender = "Nam", DateOfBirth = new DateTime(1985, 3, 10), Phone = "0934567890", Email = "jwilson@email.com", Address = "New York",  CreatedBy = "System", CreatedDate = DateTime.UtcNow, UpdatedBy = "System", UpdatedDate = DateTime.UtcNow },
                new() { Id = Guid.NewGuid().ToString(), Code = "KH004", FullName = "Lê Thị Cẩm Nhung", IdCard = "079199004567", Nationality = "Việt Nam", Gender = "Nữ",  DateOfBirth = new DateTime(1992, 11, 5), Phone = "0945678901", Email = "ltcn@email.com",   Address = "Đà Nẵng",  CreatedBy = "System", CreatedDate = DateTime.UtcNow, UpdatedBy = "System", UpdatedDate = DateTime.UtcNow },
                new() { Id = Guid.NewGuid().ToString(), Code = "KH005", FullName = "Yuki Tanaka",       IdCard = "JP98765432",   Nationality = "Nhật Bản", Gender = "Nữ",  DateOfBirth = new DateTime(1988, 7, 18), Phone = "0956789012", Email = "ytanaka@email.com", Address = "Tokyo",    CreatedBy = "System", CreatedDate = DateTime.UtcNow, UpdatedBy = "System", UpdatedDate = DateTime.UtcNow },
            };

            _context.Guests.AddRange(guests);
        }

        private async Task SeedServicesAsync()
        {
            if (await _context.Services.AnyAsync()) return;

            var services = new List<Service>
            {
                new() { Id = Guid.NewGuid().ToString(), Code = "DV001", Name = "Phục vụ ăn sáng",   Description = "Buffet sáng tại nhà hàng",       Price = 250000, Unit = "/người", ServiceType = "Ăn uống",     IsActive = true, CreatedBy = "System", CreatedDate = DateTime.UtcNow, UpdatedBy = "System", UpdatedDate = DateTime.UtcNow },
                new() { Id = Guid.NewGuid().ToString(), Code = "DV002", Name = "Dịch vụ Spa",        Description = "Massage toàn thân 60 phút",       Price = 600000, Unit = "/lần",   ServiceType = "Chăm sóc",    IsActive = true, CreatedBy = "System", CreatedDate = DateTime.UtcNow, UpdatedBy = "System", UpdatedDate = DateTime.UtcNow },
                new() { Id = Guid.NewGuid().ToString(), Code = "DV003", Name = "Hồ bơi riêng",       Description = "Thuê hồ bơi villa theo giờ",      Price = 500000, Unit = "/giờ",   ServiceType = "Giải trí",    IsActive = true, CreatedBy = "System", CreatedDate = DateTime.UtcNow, UpdatedBy = "System", UpdatedDate = DateTime.UtcNow },
                new() { Id = Guid.NewGuid().ToString(), Code = "DV004", Name = "Đưa đón sân bay",    Description = "Xe đón từ sân bay Phú Quốc",      Price = 350000, Unit = "/lượt",  ServiceType = "Vận chuyển",  IsActive = true, CreatedBy = "System", CreatedDate = DateTime.UtcNow, UpdatedBy = "System", UpdatedDate = DateTime.UtcNow },
                new() { Id = Guid.NewGuid().ToString(), Code = "DV005", Name = "Minibar",             Description = "Đồ uống cao cấp tại phòng",       Price = 120000, Unit = "/món",   ServiceType = "Ăn uống",     IsActive = true, CreatedBy = "System", CreatedDate = DateTime.UtcNow, UpdatedBy = "System", UpdatedDate = DateTime.UtcNow },
                new() { Id = Guid.NewGuid().ToString(), Code = "DV006", Name = "Giặt ủi",            Description = "Giặt quần áo trong ngày",          Price = 80000,  Unit = "/kg",    ServiceType = "Tiện ích",    IsActive = false, CreatedBy = "System", CreatedDate = DateTime.UtcNow, UpdatedBy = "System", UpdatedDate = DateTime.UtcNow },
            };

            _context.Services.AddRange(services);
        }
    }
}
