// File: Models/CreateDeviceDto.cs
namespace MedicalDeviceApi.Models // Chú ý Namespace này
{
    public class CreateDeviceDto
    {
        public string DeviceName { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}