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
			if (DeviceList is not null)
			{
				DeviceList.ItemsSource = DevicesCollection;
			}
		}

		// --- 1. TẢI DANH SÁCH ---
		private async void OnLoadClicked(object sender, EventArgs e)
		{
			try
			{
				using HttpClient client = new HttpClient();
				var devices = await client.GetFromJsonAsync<List<DeviceModel>>(ApiUrl);
				DevicesCollection.Clear();
				if (devices is { Count: > 0 })
				{
					foreach (var device in devices) DevicesCollection.Add(device);
				}
			}
			catch (Exception ex)
			{
				await DisplayAlertAsync("Lỗi", ex.Message, "OK");
			}
		}

		// --- 2. XỬ LÝ NÚT LƯU (Dùng chung cho THÊM và SỬA) ---
		private async void OnAddClicked(object sender, EventArgs e)
		{
			if (string.IsNullOrWhiteSpace(TxtName?.Text)) return;

			var deviceData = new
			{
				deviceName = TxtName?.Text ?? string.Empty,
				serialNumber = TxtSerial?.Text ?? string.Empty,
				status = TxtStatus?.Text ?? string.Empty
			};
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
					await DisplayAlertAsync("Thành công", _editingDeviceId == null ? "Đã thêm mới!" : "Đã cập nhật!", "OK");

					// Reset Form về trạng thái thêm mới
					ResetForm();

					// Tải lại danh sách để thấy thay đổi
					OnLoadClicked(this, EventArgs.Empty);
				}
				else
				{
					string error = await response.Content.ReadAsStringAsync();
					await DisplayAlertAsync("Thất bại", error, "OK");
				}
			}
			catch (Exception ex) { await DisplayAlertAsync("Lỗi", ex.Message, "OK"); }
		}

		// --- 3. CHỨC NĂNG XÓA (DELETE) ---
		private async void OnDeleteClicked(object sender, EventArgs e)
		{
			if (sender is not Button { CommandParameter: int id }) return;

			bool answer = await DisplayAlertAsync("Xác nhận", "Bạn có chắc muốn xóa thiết bị này?", "Yes", "No");
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
					await DisplayAlertAsync("Lỗi", "Không thể xóa", "OK");
				}
			}
			catch (Exception ex) { await DisplayAlertAsync("Lỗi", ex.Message, "OK"); }
		}

		// --- 4. CHỨC NĂNG CHUẨN BỊ SỬA (Đưa dữ liệu lên Form) ---
		private void OnEditClicked(object sender, EventArgs e)
		{
			if (sender is not Button { CommandParameter: DeviceModel device }) return;

			// Đổ dữ liệu cũ lên ô nhập
			if (TxtName is not null) TxtName.Text = device.DeviceName;
			if (TxtSerial is not null) TxtSerial.Text = device.SerialNumber;
			if (TxtStatus is not null) TxtStatus.Text = device.Status;

			// Lưu lại ID đang sửa
			_editingDeviceId = device.DeviceID;

			// Đổi tên nút để người dùng biết
			if (this.FindByName<Button>("LoadBtn") is Button loadBtn)
			{
				loadBtn.Text = "Hủy Bỏ Sửa"; // Tận dụng nút Load làm nút Hủy
			}
		}

		// Hàm dọn dẹp Form
		private void ResetForm()
		{
			if (TxtName is not null) TxtName.Text = string.Empty;
			if (TxtSerial is not null) TxtSerial.Text = string.Empty;
			if (TxtStatus is not null) TxtStatus.Text = string.Empty;
			_editingDeviceId = null;
			if (this.FindByName<Button>("LoadBtn") is Button loadBtn)
			{
				loadBtn.Text = "🔄 Tải Lại Danh Sách";
			}
		}

		// --- 5. CHỨC NĂNG TÌM KIẾM (SEARCH) ---

		// Sự kiện khi bấm nút Tìm hoặc Enter trên bàn phím
		private async void OnSearchPressed(object sender, EventArgs e)
		{
			string keyword = TxtSearch?.Text ?? string.Empty; // Lấy chữ người dùng nhập

			// Nếu ô tìm kiếm trống -> Tải lại tất cả (như nút Refresh)
			if (string.IsNullOrWhiteSpace(keyword))
			{
				OnLoadClicked(this, EventArgs.Empty);
				return;
			}

			try
			{
				using HttpClient client = new HttpClient();

				// Gọi API Search đã viết ở Day 10
				// URL mẫu: http://localhost:5244/api/Devices/search?keyword=X-Quang
				string searchUrl = $"{ApiUrl}/search?keyword={keyword}";

				var devices = await client.GetFromJsonAsync<List<DeviceModel>>(searchUrl);

				// Cập nhật danh sách hiển thị
				DevicesCollection.Clear();
				if (devices is { Count: > 0 })
				{
					foreach (var d in devices) DevicesCollection.Add(d);
				}
				else
				{
					// Nếu không tìm thấy gì thì thông báo nhẹ hoặc để danh sách trống
					// await DisplayAlertAsync("Kết quả", "Không tìm thấy thiết bị nào!", "OK");
				}
			}
			catch (Exception ex)
			{
				await DisplayAlertAsync("Lỗi Tìm Kiếm", ex.Message, "OK");
			}
		}

		// Sự kiện: Khi xóa trắng ô tìm kiếm thì tự load lại danh sách gốc
		private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
		{
			if (string.IsNullOrWhiteSpace(e.NewTextValue))
			{
				OnLoadClicked(this, EventArgs.Empty);
			}
		}
	}

	public class DeviceModel
	{
		public int DeviceID { get; set; }
		public string DeviceName { get; set; } = string.Empty;
		public string SerialNumber { get; set; } = string.Empty;
		public string Status { get; set; } = string.Empty;
	}
}