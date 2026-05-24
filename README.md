# Resort Management System

**Nhóm: Lâm Thành Niên – Tăng Đại Minh**  
**Môn: Kiến trúc và Thiết kế Phần mềm (Lý thuyết)**

---

## Kiến trúc hệ thống

```
ResortManagement/
├── Resort.Model/           — Entities, BaseTypes, Constants
├── Resort.DataAccess/      — IRepository, IUnitOfWork, Repository, UnitOfWork
├── Resort.Business/        — Business Interfaces + Operations
├── Resort.Utilities/       — SessionExtensions, CurrentUser
├── Resort.Web/             — ASP.NET Core MVC, Areas, Identity
│   ├── Areas/
│   │   ├── Rooms/          — Quản lý phòng
│   │   ├── Guests/         — Quản lý khách
│   │   ├── Bookings/       — Đặt phòng / Check-in / Check-out
│   │   ├── Services/       — Dịch vụ
│   │   ├── Repairs/        — Sửa chữa
│   │   ├── Staff/          — Nhân viên
│   │   ├── Notifications/  — Thông báo
│   │   ├── Reports/        — Báo cáo doanh thu
│   │   └── Administration/ — Quản trị / Master Data
│   ├── Data/               — ApplicationDbContext, IdentitySeed
│   ├── Configuration/      — ApplicationSettings, DependencyInjection
│   └── Controllers/        — HomeController (Dashboard)
└── Resort.Tests/           — xUnit tests (InMemory DB)
```

### Design Patterns áp dụng
- **Repository Pattern** — `IRepository<T>` / `Repository<T>`
- **Unit of Work** — `IUnitOfWork` / `UnitOfWork`
- **Dependency Injection** — tất cả services đăng ký qua DI container
- **Interface Segregation** — mỗi module có interface riêng
- **Areas** — tách biệt từng module theo MVC Areas

---

## Yêu cầu

- **.NET 8 SDK** — https://dotnet.microsoft.com/download/dotnet/8.0
- **SQL Server LocalDB** (đi kèm Visual Studio) hoặc SQL Server Express
- **Visual Studio 2022** (v17.8+) hoặc VS Code + C# Dev Kit

---

## Hướng dẫn chạy

### Cách 1: Visual Studio 2022

1. Mở file `ResortManagement.sln`
2. Set `Resort.Web` làm Startup Project
3. Mở **Package Manager Console**, chạy:
   ```
   Add-Migration InitialCreate -Project Resort.Web -StartupProject Resort.Web
   Update-Database -Project Resort.Web -StartupProject Resort.Web
   ```
4. Nhấn **F5** — trình duyệt tự mở

### Cách 2: .NET CLI

```bash
cd ResortManagement/Resort.Web

# Tạo migration (lần đầu)
dotnet ef migrations add InitialCreate

# Tạo database
dotnet ef database update

# Chạy ứng dụng
dotnet run
```

Truy cập: `https://localhost:7100`

---

## Tài khoản mặc định

| Email | Mật khẩu | Vai trò |
|---|---|---|
| `admin@resort.vn` | `Admin@123` | Admin |
| `manager@resort.vn` | `Manager@123` | Manager |
| `receptionist@resort.vn` | `Resort@123` | Receptionist |
| `technician@resort.vn` | `Resort@123` | Technician |

---

## 📋 Các module chức năng

| Module | Vai trò truy cập | Chức năng |
|---|---|---|
| Dashboard | Tất cả | Thống kê, biểu đồ doanh thu |
| Quản lý phòng | Admin, Manager | CRUD phòng, lọc, cập nhật trạng thái |
| Quản lý khách | Admin, Manager, Receptionist | CRUD khách, tìm kiếm, lọc quốc tịch |
| Đặt phòng | Admin, Manager, Receptionist | Đặt phòng, check-in, check-out, thêm dịch vụ, hóa đơn |
| Dịch vụ | Admin, Manager | CRUD dịch vụ, bật/tắt trạng thái |
| Sửa chữa | Admin, Manager, Technician | Tạo yêu cầu, phân công, cập nhật trạng thái |
| Nhân viên | Admin, Manager | CRUD nhân viên, quản lý ca làm |
| Thông báo | Tất cả | Gửi, nhận, đánh dấu đã đọc |
| Báo cáo | Admin, Manager | Doanh thu theo tháng/năm, biểu đồ |
| Quản trị | Admin | Master Data, làm mới cache |

---

## Unit Tests

```bash
cd ResortManagement
dotnet test
```

Test coverage: `RoomOperations`, `BookingOperations` (InMemory DB, không cần SQL Server)
