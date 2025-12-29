using MedicalDeviceApi.Model;
using MedicalDeviceApi.Models;

namespace MedicalDeviceApi.Interfaces
{
    public interface IDeviceRepository
    {
        List<Device> GetAllDevices();
        void CreateDevice(CreateDeviceDto deviceDto);
        void UpdateDevice(int id, UpdateDeviceDto deviceDto);
        bool SoftDeleteDevice(int id);
        Device? GetDeviceById(int id);
        IEnumerable<Device> SearchDevices(string? status, string? keyword);
        List<MaintenanceDto> GetMaintenanceHistory(int deviceId);
        DashboardDto GetDashboardStats();
    }
}
