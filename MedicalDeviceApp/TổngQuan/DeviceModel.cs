namespace MedicalDeviceApp
{
    public class DeviceModel
    {
        public int DeviceID { get; set; }
        public string DeviceName { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string Origin { get; set; } = string.Empty;
        public decimal? Price { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public string Supplier { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
    }
}