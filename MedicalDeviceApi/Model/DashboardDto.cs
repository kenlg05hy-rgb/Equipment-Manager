namespace MedicalDeviceApi.Model
{
    public class DashboardDto
    {
        public int TotalDevices { get; set; }
        public decimal TotalValue { get; set; }
        public int GoodCount { get; set; }
        public int BrokenCount { get; set; }
        public int OtherCount { get; set; }
    }
}