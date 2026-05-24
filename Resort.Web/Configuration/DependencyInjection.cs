using Resort.Business;
using Resort.Business.Interfaces;
using Resort.DataAccess;
using Resort.Web.Data;
using Resort.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using StackExchange.Redis;
using System.Net.Sockets;

namespace Resort.Web.Configuration
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddResortServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Cookie settings
            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Identity/Account/Login";
                options.AccessDeniedPath = "/Identity/Account/AccessDenied";
                options.ExpireTimeSpan = TimeSpan.FromHours(8);
            });

            // Cache — try Redis, fall back to in-memory
            var redisConn = configuration["CacheSettings:ConnectionString"];
            if (!string.IsNullOrWhiteSpace(redisConn) && CanConnectRedis(redisConn))
            {
                services.AddStackExchangeRedisCache(o => o.Configuration = redisConn);
            }
            else
            {
                services.AddDistributedMemoryCache();
            }

            services.AddMemoryCache();

            // Session
            services.AddSession(o =>
            {
                o.IdleTimeout = TimeSpan.FromMinutes(30);
                o.Cookie.HttpOnly = true;
                o.Cookie.IsEssential = true;
            });

            // Unit of Work
            services.AddScoped<IUnitOfWork>(sp =>
                new UnitOfWork(sp.GetRequiredService<ApplicationDbContext>()));

            // Seeds
            services.AddScoped<IIdentitySeed, IdentitySeed>();

            // Business operations
            services.AddScoped<IRoomOperations, RoomOperations>();
            services.AddScoped<IGuestOperations, GuestOperations>();
            services.AddScoped<IBookingOperations, BookingOperations>();
            services.AddScoped<IServiceOperations, ServiceOperations>();
            services.AddScoped<IRepairOperations, RepairOperations>();
            services.AddScoped<IStaffOperations, StaffOperations>();
            services.AddScoped<INotificationOperations, NotificationOperations>();
            services.AddScoped<IReportOperations, ReportOperations>();
            services.AddScoped<IMasterDataOperations, MasterDataOperations>();
            services.AddScoped<IMasterDataCacheOperations, MasterDataCacheOperations>();

            // App services
            services.AddScoped<INavigationService, NavigationService>();

            return services;
        }

        private static bool CanConnectRedis(string connectionString)
        {
            try
            {
                var parts = connectionString.Split(':');
                var host = parts[0];
                var port = parts.Length > 1 && int.TryParse(parts[1], out var p) ? p : 6379;
                using var tcp = new TcpClient();
                return tcp.ConnectAsync(host, port).Wait(200);
            }
            catch { return false; }
        }
    }
}
