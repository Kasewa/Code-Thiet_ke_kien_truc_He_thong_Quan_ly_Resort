namespace Resort.Web.Configuration
{
    public class ApplicationSettings
    {
        public string ApplicationName { get; set; } = "Resort Management System";
        public string ApplicationTitle { get; set; } = "Resort TML";
        public string Version { get; set; } = "1.0.0";
        public string WebUrl { get; set; } = "https://localhost:7100";

        // Default seeded accounts
        public string AdminEmail { get; set; } = "admin@resort.vn";
        public string AdminPassword { get; set; } = "Admin@123";

        public string ManagerEmail { get; set; } = "manager@resort.vn";
        public string ManagerPassword { get; set; } = "Manager@123";

        public string ReceptionistEmail { get; set; } = "receptionist@resort.vn";
        public string ReceptionistPassword { get; set; } = "Resort@123";

        public string TechnicianEmail { get; set; } = "technician@resort.vn";
        public string TechnicianPassword { get; set; } = "Resort@123";
    }
}
