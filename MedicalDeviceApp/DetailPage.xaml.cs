using System.Collections.ObjectModel;
using System.Net.Http.Json;

namespace MedicalDeviceApp
{
    [QueryProperty(nameof(Device), "DeviceObj")]
    public partial class DetailPage : ContentPage
    {
        private const string ApiUrl = "http://localhost:5244/api/Devices"; // Link API gốc
        private DeviceModel _device;

        // Danh sách để hiển thị lên màn hình
        public ObservableCollection<MaintenanceModel> MaintenanceHistory { get; set; } = new();

        public DeviceModel Device
        {
            get => _device;
            set
            {
                _device = value;
                LoadData();       // Hiện thông tin thiết bị
                LoadHistory();    // Hiện lịch sử bảo trì (MỚI)
            }
        }

        public DetailPage()
        {
            InitializeComponent();
            MaintenanceList.ItemsSource = MaintenanceHistory; // Gán dữ liệu vào List
        }

        private void LoadData()
        {
            if (_device == null) return;
            LblName.Text = _device.DeviceName;
            LblSerial.Text = _device.SerialNumber;
            LblModel.Text = _device.Model;
            LblOrigin.Text = _device.Origin;
            LblPrice.Text = (_device.Price ?? 0).ToString("N0") + " VNĐ";
            LblDate.Text = _device.PurchaseDate?.ToString("dd/MM/yyyy");
            LblSupplier.Text = _device.Supplier;
            LblDept.Text = _device.Department;
            LblStatus.Text = _device.Status;
        }

        // HÀM MỚI: Gọi API lấy lịch sử bảo trì
        private async void LoadHistory()
        {
            if (_device == null) return;

            try
            {
                using HttpClient client = new HttpClient();
                // Gọi API: /api/Devices/{id}/maintenance
                var history = await client.GetFromJsonAsync<List<MaintenanceModel>>($"{ApiUrl}/{_device.DeviceID}/maintenance");

                MaintenanceHistory.Clear();
                if (history != null)
                {
                    foreach (var item in history) MaintenanceHistory.Add(item);
                }
            }
            catch (Exception)
            {
                // Nếu lỗi thì thôi không hiện gì, hoặc có thể hiện thông báo nhỏ
            }
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}