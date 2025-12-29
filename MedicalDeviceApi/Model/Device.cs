namespace MedicalDeviceApi.Models
{
    public class Device
    {
        public int DeviceID { get; set; }
        public string DeviceName { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsDeleted { get; set; }
        public string? Model { get; set; }
        public string? Origin { get; set; }
        public int? ManufactureYear { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public DateTime? WarrantyExpiry { get; set; }
        public decimal? Price { get; set; }
        public string? Supplier { get; set; }
        public string? Department { get; set; }
    }

    public class MaintenanceDto
    {
        public DateTime MaintenanceDate { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Cost { get; set; }
    }
}