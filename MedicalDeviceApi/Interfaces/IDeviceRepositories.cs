using MedicalDeviceApi.Models;

namespace MedicalDeviceApi.Interfaces
{
    public interface IDeviceRepository
    {
        List<Device> GetAllDevices();
        void CreateDevice(CreateDeviceDto deviceDto);
        void UpdateDevice(int id, UpdateDeviceDto deviceDto);
        bool SoftDeleteDevice(int id);
    }
}
