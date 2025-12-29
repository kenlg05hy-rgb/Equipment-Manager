using System.Net.Http.Json;

namespace MedicalDeviceApp
{
    [QueryProperty(nameof(Device), "DeviceObj")]
    public partial class AddEditPage : ContentPage
    {
        private const string ApiUrl = "http://localhost:5244/api/Devices";
        private DeviceModel _device;

        public DeviceModel Device
        {
            get => _device;
            set { _device = value; if (_device != null) LoadDataForEdit(); }
        }

        public AddEditPage() { InitializeComponent(); }

        private void LoadDataForEdit()
        {
            LblHeader.Text = "Chỉnh Sửa Thiết Bị";
            TxtName.Text = _device.DeviceName;
            TxtSerial.Text = _device.SerialNumber;
            TxtModel.Text = _device.Model;
            TxtOrigin.Text = _device.Origin;
            TxtPrice.Text = _device.Price?.ToString("0");
            TxtSupplier.Text = _device.Supplier;
            TxtDepartment.Text = _device.Department;
            TxtStatus.Text = _device.Status;
            if (_device.PurchaseDate.HasValue) PickPurchaseDate.Date = _device.PurchaseDate.Value;
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtName.Text) || string.IsNullOrWhiteSpace(TxtSerial.Text))
            {
                await DisplayAlert("Lỗi", "Vui lòng nhập Tên và Serial", "OK"); return;
            }
            decimal.TryParse(TxtPrice.Text, out decimal price);
            var deviceData = new DeviceModel
            {
                DeviceID = _device?.DeviceID ?? 0,
                DeviceName = TxtName.Text,
                SerialNumber = TxtSerial.Text,
                Model = TxtModel.Text,
                Origin = TxtOrigin.Text,
                Price = price,
                Supplier = TxtSupplier.Text,
                Department = TxtDepartment.Text,
                Status = TxtStatus.Text ?? "Mới",
                PurchaseDate = PickPurchaseDate.Date
            };

            using HttpClient client = new HttpClient();
            HttpResponseMessage response;
            try
            {
                if (_device == null) response = await client.PostAsJsonAsync(ApiUrl, deviceData);
                else response = await client.PutAsJsonAsync($"{ApiUrl}/{_device.DeviceID}", deviceData);

                if (response.IsSuccessStatusCode)
                {
                    await DisplayAlert("Thành công", "Đã lưu dữ liệu!", "OK");
                    await Shell.Current.GoToAsync("..");
                }
                else await DisplayAlert("Lỗi API", await response.Content.ReadAsStringAsync(), "OK");
            }
            catch (Exception ex) { await DisplayAlert("Lỗi", ex.Message, "OK"); }
        }

        private async void OnCancelClicked(object sender, EventArgs e) { await Shell.Current.GoToAsync(".."); }
    }
}