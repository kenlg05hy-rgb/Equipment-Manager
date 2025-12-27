using Microsoft.Data.SqlClient;
using MedicalDeviceApi.Interfaces;
using MedicalDeviceApi.Models;
using System.Data;

namespace MedicalDeviceApi.Repositories
{
    public class DeviceRepository : IDeviceRepository
    {
        private readonly string _connectionString;

        public DeviceRepository(IConfiguration configuration)
        {
            // Lấy connection string tại đây
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        public List<Device> GetAllDevices()
        {
            var devices = new List<Device>();

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                // Code cũ của bạn dùng Stored Procedure, rất tốt! Giữ nguyên.
                using (var cmd = new SqlCommand("sp_GetDevices", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var deviceId = reader.IsDBNull(reader.GetOrdinal("DeviceID")) ? 0 : reader.GetInt32(reader.GetOrdinal("DeviceID"));
                            var deviceName = reader.IsDBNull(reader.GetOrdinal("DeviceName")) ? string.Empty : reader.GetString(reader.GetOrdinal("DeviceName"));
                            var serialNumber = reader.IsDBNull(reader.GetOrdinal("SerialNumber")) ? string.Empty : reader.GetString(reader.GetOrdinal("SerialNumber"));
                            var status = reader.IsDBNull(reader.GetOrdinal("Status")) ? string.Empty : reader.GetString(reader.GetOrdinal("Status"));

                            devices.Add(new Device
                            {
                                DeviceID = deviceId,
                                DeviceName = deviceName,
                                SerialNumber = serialNumber,
                                Status = status
                            });
                        }
                    }
                }
            }
            return devices;
        }

        public void CreateDevice(CreateDeviceDto device)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                var sql = "INSERT INTO Devices (DeviceName, SerialNumber, Status) VALUES (@Name, @Serial, @Status)";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Name", device.DeviceName);
                    // Xử lý null cho SerialNumber
                    cmd.Parameters.AddWithValue("@Serial", device.SerialNumber ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Status", device.Status);

                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch (SqlException ex)
                    {
                        // Nếu là lỗi trùng lặp (Duplicate Key)
                        if (ex.Number == 2627 || ex.Number == 2601)
                        {
                            // Ném ra một lỗi tùy chỉnh hoặc lỗi C# cơ bản kèm thông báo rõ ràng
                            throw new DuplicateNameException($"Số Serial '{device.SerialNumber}' đã tồn tại.");
                        }

                        // Nếu là lỗi khác (mất mạng, sai tên bảng...), ném nguyên văn ra ngoài
                        throw;
                    }
                }
            }
        }

        public void UpdateDevice(int id, UpdateDeviceDto deviceDto)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            using var cmd = new SqlCommand(
                @"UPDATE Devices
                  SET DeviceName = @DeviceName,
                      Status = @Status
                  WHERE DeviceID = @DeviceID", conn);

            cmd.Parameters.AddWithValue("@DeviceName", deviceDto.DeviceName);
            cmd.Parameters.AddWithValue("@Status", deviceDto.Status);
            cmd.Parameters.AddWithValue("@DeviceID", id);

            int row = cmd.ExecuteNonQuery();

            if (row == 0) throw new KeyNotFoundException($"Device with ID {id} not found.");
        }

        public bool SoftDeleteDevice(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            using var cmd = new SqlCommand(
                @"UPDATE Devices
                  SET IsDeleted = 1
                  WHERE DeviceID = @DeviceID", conn);

            cmd.Parameters.AddWithValue("@DeviceID", id);

            int row = cmd.ExecuteNonQuery();

            return row > 0;
        }

        public Device? GetDeviceById(int DeviceID)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var cmd = new SqlCommand(
                @"SELECT * FROM Devices
          WHERE DeviceID = @id AND IsDeleted = 0", conn);

            cmd.Parameters.AddWithValue("@id", DeviceID);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) return null;

            return new Device
            {
                DeviceID = (int)reader["DeviceID"],
                DeviceName = reader["DeviceName"]?.ToString() ?? string.Empty,
                SerialNumber = reader["SerialNumber"]?.ToString() ?? string.Empty,
                Status = reader["Status"]?.ToString() ?? string.Empty
            };
        }

    }
}