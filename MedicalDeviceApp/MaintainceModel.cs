namespace MedicalDeviceApp
{
    public class MaintenanceModel
    {
        public DateTime MaintenanceDate { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Cost { get; set; }

        // Property phụ để hiển thị ngày đẹp hơn trên giao diện
        public string FormattedDate => MaintenanceDate.ToString("dd/MM/yyyy");
        // Property phụ để hiển thị tiền đẹp hơn
        public string FormattedCost => Cost.ToString("N0") + " VNĐ";
    }
}