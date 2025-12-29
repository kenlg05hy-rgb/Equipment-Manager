namespace MedicalDeviceApi.Model
{
    public class DashboardDto
    {
        public int TotalDevices { get; set; }           // Tổng số thiết bị
        public decimal TotalValue { get; set; }         // Tổng giá trị tài sản
        public int GoodCount { get; set; }              // Số máy tốt
        public int BrokenCount { get; set; }            // Số máy hỏng/bảo trì
        public int OtherCount { get; set; }             // Trạng thái khác
    }
}