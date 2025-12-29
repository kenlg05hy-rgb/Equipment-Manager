namespace MedicalDeviceApp
{
    public class DashboardModel
    {
        public int TotalDevices { get; set; }
        public decimal TotalValue { get; set; }
        public int GoodCount { get; set; }
        public int BrokenCount { get; set; }
        public int OtherCount { get; set; }

        public string FormattedValue => TotalValue.ToString("N0") + " VNĐ";
    }
}
