using System.Collections.ObjectModel;
using System.Net.Http.Json;

namespace MedicalDeviceApp
{
	public partial class MainPage : ContentPage
	{
		private const string ApiUrl = "http://localhost:5244/api/Devices";
		public ObservableCollection<DeviceModel> DevicesCollection { get; set; } = new();

		// Biến lưu trạng thái: Đang sửa thiết bị nào? (null = đang thêm mới)
		private int? _editingDeviceId = null;

		public MainPage()
		{
			InitializeComponent();
			DeviceList.ItemsSource = DevicesCollection;
		}

		// --- 1. TẢI DANH SÁCH ---
		private async void OnLoadClicked(object sender, EventArgs e)
		{
			try
			{
				using HttpClient client = new HttpClient();
				var devices = await client.GetFromJsonAsync<List<DeviceModel>>(ApiUrl);
				DevicesCollection.Clear();
				foreach (var device in devices) DevicesCollection.Add(device);
			}
			catch (Exception ex)
			{
				await DisplayAlert("Lỗi", ex.Message, "OK");
			}
		}

		// --- 2. XỬ LÝ NÚT LƯU (Dùng chung cho THÊM và SỬA) ---
		private async void OnAddClicked(object sender, EventArgs e)
		{
			if (string.IsNullOrWhiteSpace(TxtName.Text)) return;

			var deviceData = new { deviceName = TxtName.Text, serialNumber = TxtSerial.Text, status = TxtStatus.Text };
			using HttpClient client = new HttpClient();

			try
			{
				HttpResponseMessage response;

				if (_editingDeviceId == null)
				{
					// === TRƯỜNG HỢP THÊM MỚI (POST) ===
					response = await client.PostAsJsonAsync(ApiUrl, deviceData);
				}
				else
				{
					// === TRƯỜNG HỢP CẬP NHẬT (PUT) ===
					// Gọi API: PUT /api/Devices/{id}
					response = await client.PutAsJsonAsync($"{ApiUrl}/{_editingDeviceId}", deviceData);
				}

				if (response.IsSuccessStatusCode)
				{
					await DisplayAlert("Thành công", _editingDeviceId == null ? "Đã thêm mới!" : "Đã cập nhật!", "OK");

					// Reset Form về trạng thái thêm mới
					ResetForm();

					// Tải lại danh sách để thấy thay đổi
					OnLoadClicked(null, null);
				}
				else
				{
					string error = await response.Content.ReadAsStringAsync();
					await DisplayAlert("Thất bại", error, "OK");
				}
			}
			catch (Exception ex) { await DisplayAlert("Lỗi", ex.Message, "OK"); }
		}

		// --- 3. CHỨC NĂNG XÓA (DELETE) ---
		private async void OnDeleteClicked(object sender, EventArgs e)
		{
			var button = sender as Button;
			int id = (int)button.CommandParameter; // Lấy ID từ nút bấm

			bool answer = await DisplayAlert("Xác nhận", "Bạn có chắc muốn xóa thiết bị này?", "Yes", "No");
			if (!answer) return;

			try
			{
				using HttpClient client = new HttpClient();
				var response = await client.DeleteAsync($"{ApiUrl}/{id}");

				if (response.IsSuccessStatusCode)
				{
					// Xóa trên giao diện ngay lập tức (Không cần gọi lại API Load)
					var itemToRemove = DevicesCollection.FirstOrDefault(x => x.DeviceID == id);
					if (itemToRemove != null) DevicesCollection.Remove(itemToRemove);
				}
				else
				{
					await DisplayAlert("Lỗi", "Không thể xóa", "OK");
				}
			}
			catch (Exception ex) { await DisplayAlert("Lỗi", ex.Message, "OK"); }
		}

		// --- 4. CHỨC NĂNG CHUẨN BỊ SỬA (Đưa dữ liệu lên Form) ---
		private void OnEditClicked(object sender, EventArgs e)
		{
			var button = sender as Button;
			var device = button.CommandParameter as DeviceModel; // Lấy cả cục Object

			// Đổ dữ liệu cũ lên ô nhập
			TxtName.Text = device.DeviceName;
			TxtSerial.Text = device.SerialNumber;
			TxtStatus.Text = device.Status;

			// Lưu lại ID đang sửa
			_editingDeviceId = device.DeviceID;

			// Đổi tên nút để người dùng biết
			(this.FindByName("LoadBtn") as Button).Text = "Hủy Bỏ Sửa"; // Tận dụng nút Load làm nút Hủy
		}

		// Hàm dọn dẹp Form
		private void ResetForm()
		{
			TxtName.Text = ""; TxtSerial.Text = ""; TxtStatus.Text = "";
			_editingDeviceId = null;
			(this.FindByName("LoadBtn") as Button).Text = "🔄 Tải Lại Danh Sách";
		}
	}

	public class DeviceModel
	{
		public int DeviceID { get; set; }
		public string DeviceName { get; set; }
		public string SerialNumber { get; set; }
		public string Status { get; set; }
	}
}