using System.ComponentModel.DataAnnotations;

namespace MedicalDeviceApi.Models
{
    public class UpdateDeviceDto
    {
        public required string DeviceName { get; set; }

        public required string Status { get; set; }
    }
}
