using System.Collections.ObjectModel;
using System.Net.Http.Json;

namespace MedicalDeviceApp
{
    [QueryProperty(nameof(Device), "DeviceObj")]
    public partial class DetailPage : ContentPage
    {
        private const string ApiUrl = "http://localhost:5244/api/Devices"; // Link API
        private DeviceModel _device = new();

        // Danh sách để hiển thị lên màn hình
        public ObservableCollection<MaintenanceModel> MaintenanceHistory { get; set; } = new();

        public DeviceModel Device
        {
            get => _device;
            set
            {
                _device = value;
                LoadData();
                LoadHistory();
            }
        }

        // Khởi tạo
        public DetailPage()
        {
            InitializeComponent();
            MaintenanceList.ItemsSource = MaintenanceHistory;
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

        // TẢI LỊCH SỬ BẢO TRÌ
        private async void LoadHistory()
        {
            if (_device == null) return;

            try
            {
                using HttpClient client = new HttpClient();
                var history = await client.GetFromJsonAsync<List<MaintenanceModel>>($"{ApiUrl}/{_device.DeviceID}/maintenance");

                MaintenanceHistory.Clear();
                if (history != null)
                {
                    foreach (var item in history) MaintenanceHistory.Add(item);
                }
            }
            catch (Exception)
            {
                await DisplayAlertAsync("Lỗi", "Không kết nối được API để tải lịch sử bảo trì!", "OK");
            }
        }

        // NÚT QUAY LẠI
        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}