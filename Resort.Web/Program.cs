using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Resort.Business.Interfaces;
using Resort.Web.Configuration;
using Resort.Web.Data;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// EF Core + SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// ASP.NET Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
    {
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders()
    .AddDefaultUI();

// Configuration
builder.Services.AddOptions();
builder.Services.Configure<ApplicationSettings>(builder.Configuration.GetSection("ApplicationSettings"));

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Resort feature services (DI, cache, operations, navigation)
builder.Services.AddResortServices(builder.Configuration);

var app = builder.Build();

// Migrate + seed on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var dbContext = services.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync();

        var seed = services.GetRequiredService<IIdentitySeed>();
        await seed.SeedAsync();

        // Warm up master data cache
        _ = Task.Run(async () =>
        {
            try
            {
                using var cacheScope = app.Services.CreateScope();
                var cacheOps = cacheScope.ServiceProvider.GetRequiredService<IMasterDataCacheOperations>();
                await cacheOps.CreateMasterDataCacheAsync();
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Master data cache warm-up failed.");
            }
        });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error during startup initialization.");
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// Areas route
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

// Default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
