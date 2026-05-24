using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace Resort.Utilities
{
    public static class SessionExtensions
    {
        public static void SetSession(this ISession session, string key, object value)
        {
            session.Set(key, Encoding.UTF8.GetBytes(JsonSerializer.Serialize(value)));
        }

        public static T? GetSession<T>(this ISession session, string key)
        {
            if (session.TryGetValue(key, out byte[]? value))
                return JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(value));
            return default;
        }
    }

    public class CurrentUser
    {
        public const string SessionKey = "CurrentUser";

        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();

        public bool IsAdmin => Roles.Contains(Resort.Model.Constants.AdminRole);
        public bool IsManager => Roles.Contains(Resort.Model.Constants.ManagerRole);
        public bool IsReceptionist => Roles.Contains(Resort.Model.Constants.ReceptionistRole);
        public bool IsTechnician => Roles.Contains(Resort.Model.Constants.TechnicianRole);
    }
}
