using System.ComponentModel.DataAnnotations;

namespace MedicalDeviceApi.Models
{
    public class CreateDeviceDto
    {
        [Required(ErrorMessage = "Tên thiết bị không được để trống")]
        public string DeviceName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số Serial không được để trống")]
        public string SerialNumber { get; set; } = string.Empty;
        public string Status { get; set; } = "Mới";
        public string? Model { get; set; }
        public string? Origin { get; set; }
        public int? ManufactureYear { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public DateTime? WarrantyExpiry { get; set; }
        public decimal? Price { get; set; }
        public string? Supplier { get; set; }
        public string? Department { get; set; }
    }
}