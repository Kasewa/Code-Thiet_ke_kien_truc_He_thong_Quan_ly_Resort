namespace Resort.Web.Services
{
    public interface INavigationService
    {
        List<NavItem> GetNavItemsForRoles(IEnumerable<string> roles);
    }

    public class NavItem
    {
        public string Title { get; set; } = string.Empty;
        public string Area { get; set; } = string.Empty;
        public string Controller { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public List<string> AllowedRoles { get; set; } = new();
    }

    public class NavigationService : INavigationService
    {
        private static readonly List<NavItem> _allItems = new()
        {
            new NavItem { Title = "Dashboard",          Area = "",                  Controller = "Home",         Action = "Index",     Icon = "bi-speedometer2",    AllowedRoles = new() { "Admin", "Manager", "Receptionist", "Technician" } },
            new NavItem { Title = "Quản lý phòng",      Area = "Rooms",             Controller = "Rooms",        Action = "Index",     Icon = "bi-door-open",       AllowedRoles = new() { "Admin", "Manager" } },
            new NavItem { Title = "Quản lý khách",      Area = "Guests",            Controller = "Guests",       Action = "Index",     Icon = "bi-people",          AllowedRoles = new() { "Admin", "Manager", "Receptionist" } },
            new NavItem { Title = "Đặt phòng",          Area = "Bookings",          Controller = "Bookings",     Action = "Index",     Icon = "bi-calendar-check",  AllowedRoles = new() { "Admin", "Manager", "Receptionist" } },
            new NavItem { Title = "Dịch vụ",            Area = "Services",          Controller = "Services",     Action = "Index",     Icon = "bi-cup-straw",       AllowedRoles = new() { "Admin", "Manager" } },
            new NavItem { Title = "Sửa chữa",           Area = "Repairs",           Controller = "Repairs",      Action = "Index",     Icon = "bi-tools",           AllowedRoles = new() { "Admin", "Manager", "Technician" } },
            new NavItem { Title = "Nhân viên",          Area = "Staff",             Controller = "Staff",        Action = "Index",     Icon = "bi-person-badge",    AllowedRoles = new() { "Admin", "Manager" } },
            new NavItem { Title = "Thông báo",          Area = "Notifications",     Controller = "Notifications",Action = "Index",     Icon = "bi-bell",            AllowedRoles = new() { "Admin", "Manager", "Receptionist", "Technician" } },
            new NavItem { Title = "Báo cáo",            Area = "Reports",           Controller = "Reports",      Action = "Index",     Icon = "bi-bar-chart-line",  AllowedRoles = new() { "Admin", "Manager" } },
            new NavItem { Title = "Quản trị hệ thống",  Area = "Administration",    Controller = "Administration",Action = "Index",    Icon = "bi-gear",            AllowedRoles = new() { "Admin" } },
        };

        public List<NavItem> GetNavItemsForRoles(IEnumerable<string> roles)
        {
            var roleSet = roles.ToHashSet();
            return _allItems
                .Where(item => item.AllowedRoles.Any(r => roleSet.Contains(r)))
                .ToList();
        }
    }
}
