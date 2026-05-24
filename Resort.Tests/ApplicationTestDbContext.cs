using Microsoft.EntityFrameworkCore;
using Resort.Model.Models;

namespace Resort.Tests
{
    /// <summary>
    /// DbContext dùng InMemory cho unit test (không phụ thuộc SQL Server).
    /// </summary>
    public class ApplicationTestDbContext : DbContext
    {
        public ApplicationTestDbContext(DbContextOptions<ApplicationTestDbContext> options)
            : base(options) { }

        public DbSet<Room> Rooms { get; set; }
        public DbSet<Guest> Guests { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<BookingService> BookingServices { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<RepairRequest> RepairRequests { get; set; }
        public DbSet<Staff> Staffs { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<MasterDataKey> MasterDataKeys { get; set; }
        public DbSet<MasterDataValue> MasterDataValues { get; set; }
    }
}
