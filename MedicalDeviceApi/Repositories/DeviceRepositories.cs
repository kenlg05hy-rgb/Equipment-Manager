using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using MedicalDeviceApi.Interfaces;
using MedicalDeviceApi.Model;
using MedicalDeviceApi.Models;
using System.Data;

namespace MedicalDeviceApi.Repositories
{
    public class DeviceRepository : IDeviceRepository
    {
        private readonly string _connectionString;

        public DeviceRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        // LẤY DANH SÁCH
        public List<Device> GetAllDevices()
        {
            var devices = new List<Device>();

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                var sql = "SELECT * FROM Devices WHERE IsDeleted = 0 ORDER BY CreatedAt DESC";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            devices.Add(MapReaderToDevice(reader));
                        }
                    }
                }
            }
            return devices;
        }

        // THÊM THIẾT BỊ MỚI
        public void CreateDevice(CreateDeviceDto device)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                var sql = @"INSERT INTO Devices 
                           (DeviceName, SerialNumber, Status, Model, Origin, ManufactureYear, PurchaseDate, WarrantyExpiry, Price, Supplier, Department) 
                           VALUES 
                           (@Name, @Serial, @Status, @Model, @Origin, @ManYear, @BuyDate, @Warranty, @Price, @Supplier, @Dept)";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    // Các trường bắt buộc
                    cmd.Parameters.AddWithValue("@Name", device.DeviceName);
                    cmd.Parameters.AddWithValue("@Serial", device.SerialNumber);
                    cmd.Parameters.AddWithValue("@Status", device.Status ?? "Mới");

                    // Các trường tùy chọn
                    cmd.Parameters.AddWithValue("@Model", (object?)device.Model ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Origin", (object?)device.Origin ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ManYear", (object?)device.ManufactureYear ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@BuyDate", (object?)device.PurchaseDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Warranty", (object?)device.WarrantyExpiry ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Price", (object?)device.Price ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Supplier", (object?)device.Supplier ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Dept", (object?)device.Department ?? DBNull.Value);

                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch (SqlException ex)
                    {
                        if (ex.Number == 2627 || ex.Number == 2601)
                            throw new DuplicateNameException($"Số Serial '{device.SerialNumber}' đã tồn tại.");
                        throw;
                    }
                }
            }
        }

        // LẤY THIẾT BỊ THEO ID
        public Device? GetDeviceById(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var cmd = new SqlCommand("SELECT * FROM Devices WHERE DeviceID = @id AND IsDeleted = 0", conn);
            cmd.Parameters.AddWithValue("@id", id);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) return null;
            return MapReaderToDevice(reader);
        }

        // CẬP NHẬT THIẾT BỊ
        public void UpdateDevice(int id, UpdateDeviceDto deviceDto)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var cmd = new SqlCommand("UPDATE Devices SET DeviceName = @Name, Status = @Status WHERE DeviceID = @Id", conn);
            cmd.Parameters.AddWithValue("@Name", deviceDto.DeviceName);
            cmd.Parameters.AddWithValue("@Status", deviceDto.Status);
            cmd.Parameters.AddWithValue("@Id", id);
            if (cmd.ExecuteNonQuery() == 0) throw new KeyNotFoundException();
        }

        // XÓA THIẾT BỊ
        public bool SoftDeleteDevice(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var cmd = new SqlCommand("UPDATE Devices SET IsDeleted = 1 WHERE DeviceID = @Id", conn);
            cmd.Parameters.AddWithValue("@Id", id);
            return cmd.ExecuteNonQuery() > 0;
        }

        // TÌM KIẾM THIẾT BỊ
        public IEnumerable<Device> SearchDevices(string? status, string? keyword)
        {
            var devices = new List<Device>();
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var sql = @"SELECT * FROM Devices WHERE IsDeleted = 0 
                        AND (@status IS NULL OR Status = @status)
                        AND (@keyword IS NULL OR DeviceName LIKE '%' + @keyword + '%')";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@status", (object?)status ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@keyword", (object?)keyword ?? DBNull.Value);

            using var reader = cmd.ExecuteReader();
            while (reader.Read()) devices.Add(MapReaderToDevice(reader));
            return devices;
        }

        // HÀM PHỤ TRỢ
        private Device MapReaderToDevice(SqlDataReader reader)
        {
            return new Device
            {
                DeviceID = (int)reader["DeviceID"],
                DeviceName = reader["DeviceName"].ToString()!,
                SerialNumber = reader["SerialNumber"].ToString()!,
                Status = reader["Status"].ToString()!,
                CreatedAt = reader["CreatedAt"] != DBNull.Value ? (DateTime)reader["CreatedAt"] : DateTime.MinValue,
                IsDeleted = (bool)reader["IsDeleted"],

                // Map các cột mới (xử lý null an toàn)
                Model = reader["Model"] as string,
                Origin = reader["Origin"] as string,
                ManufactureYear = reader["ManufactureYear"] as int?,
                PurchaseDate = reader["PurchaseDate"] as DateTime?,
                WarrantyExpiry = reader["WarrantyExpiry"] as DateTime?,
                Price = reader["Price"] as decimal?,
                Supplier = reader["Supplier"] as string,
                Department = reader["Department"] as string
            };
        }

        // LẤY LỊCH SỬ BẢO TRÌ
        public List<MaintenanceDto> GetMaintenanceHistory(int deviceId)
        {
            var list = new List<MaintenanceDto>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("sp_GetMaintenanceByDevice", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@DeviceID", deviceId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new MaintenanceDto
                            {
                                MaintenanceDate = reader.GetDateTime(reader.GetOrdinal("MaintenanceDate")),
                                Description = reader.GetString(reader.GetOrdinal("Description")),
                                Cost = reader.GetDecimal(reader.GetOrdinal("Cost"))
                            });
                        }
                    }
                }
            }
            return list;
        }

        // LẤY THỐNG KÊ
        public DashboardDto GetDashboardStats()
        {
            var stats = new DashboardDto();

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                using (var cmd = new SqlCommand("SELECT COUNT(*), SUM(Price) FROM Devices WHERE IsDeleted = 0 OR IsDeleted IS NULL", conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            stats.TotalDevices = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                            stats.TotalValue = reader.IsDBNull(1) ? 0 : reader.GetDecimal(1);
                        }
                    }
                }

                // Đếm theo trạng thái (Tốt / Hỏng / Khác)
                var sqlStatus = @"
            SELECT 
                SUM(CASE WHEN Status LIKE N'%Tốt%' THEN 1 ELSE 0 END) as Good,
                SUM(CASE WHEN Status LIKE N'%Hỏng%' OR Status LIKE N'%Bảo trì%' THEN 1 ELSE 0 END) as Broken
            FROM Devices WHERE IsDeleted = 0 OR IsDeleted IS NULL";

                using (var cmd = new SqlCommand(sqlStatus, conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            stats.GoodCount = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                            stats.BrokenCount = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
                            // Các trạng thái còn lại
                            stats.OtherCount = stats.TotalDevices - stats.GoodCount - stats.BrokenCount;
                        }
                    }
                }
            }
            return stats;
        }
    }
}