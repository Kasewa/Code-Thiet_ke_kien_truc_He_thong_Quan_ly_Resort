using System.ComponentModel.DataAnnotations;

namespace Resort.Model.Models
{
    /// <summary>Nhóm dữ liệu danh mục: RoomType, Nationality, Department, Position, v.v.</summary>
    public class MasterDataKey : BaseEntity, IAuditTracker
    {
        [Required]
        public string Key { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public string UpdatedBy { get; set; } = string.Empty;
        public DateTime? UpdatedDate { get; set; }

        public virtual ICollection<MasterDataValue> MasterDataValues { get; set; } = new List<MasterDataValue>();
    }

    public class MasterDataValue : BaseEntity, IAuditTracker
    {
        [Required]
        public string Value { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; } = true;

        [Required]
        public string MasterDataKeyId { get; set; } = string.Empty;

        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public string UpdatedBy { get; set; } = string.Empty;
        public DateTime? UpdatedDate { get; set; }

        public virtual MasterDataKey MasterDataKey { get; set; } = null!;
    }

    public static class MasterKeys
    {
        public const string RoomType = "RoomType";
        public const string RoomStatus = "RoomStatus";
        public const string ServiceType = "ServiceType";
        public const string RepairPriority = "RepairPriority";
        public const string Department = "Department";
        public const string Position = "Position";
        public const string Shift = "Shift";
        public const string Nationality = "Nationality";
        public const string BookingStatus = "BookingStatus";
    }
}
