namespace MedicalDeviceApi.Models
{
    public class Device
    {
        public int DeviceID { get; set; }
    public string DeviceName { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    }
}
