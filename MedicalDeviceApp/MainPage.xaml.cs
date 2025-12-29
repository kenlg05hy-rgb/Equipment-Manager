using System.Collections.ObjectModel;
using System.Net.Http.Json;

namespace MedicalDeviceApp
{
	public partial class MainPage : ContentPage
	{
		// Link API
		private const string ApiUrl = "http://localhost:5244/api/Devices";

		public ObservableCollection<DeviceModel> DevicesCollection { get; set; } = new();
		private int? _editingDeviceId = null;

		public MainPage()
		{
			InitializeComponent();
			DeviceList.ItemsSource = DevicesCollection;
			// Load dữ liệu khi mở App
			OnLoadClicked(null, null);
		}

		// TẢI DANH SÁCH
		private async void OnLoadClicked(object sender, EventArgs e)
		{
			try
			{
				using HttpClient client = new HttpClient();
				var devices = await client.GetFromJsonAsync<List<DeviceModel>>(ApiUrl);

				DevicesCollection.Clear();
				if (devices != null)
				{
					foreach (var device in devices) DevicesCollection.Add(device);
				}
			}
			catch (Exception ex)
			{
				await DisplayAlertAsync("Lỗi", "Không kết nối được API: " + ex.Message, "OK");
			}
		}

		// LƯU
		private async void OnAddClicked(object sender, EventArgs e)
		{
			// Validate
			if (string.IsNullOrWhiteSpace(TxtName.Text) || string.IsNullOrWhiteSpace(TxtSerial.Text))
			{
				await DisplayAlertAsync("Thiếu thông tin", "Vui lòng nhập Tên và Số Serial!", "OK");
				return;
			}

			// Xử lý giá tiền
			decimal.TryParse(TxtPrice.Text, out decimal price);

			var deviceData = new
			{
				deviceName = TxtName.Text,
				serialNumber = TxtSerial.Text,
				status = TxtStatus.Text ?? "Mới",

				model = TxtModel.Text,
				origin = TxtOrigin.Text,
				price = price,
				supplier = TxtSupplier.Text,
				department = TxtDepartment.Text,
				purchaseDate = PickPurchaseDate.Date
			};

			using HttpClient client = new HttpClient();
			try
			{
				HttpResponseMessage response;
				if (_editingDeviceId == null)
				{
					// Post
					response = await client.PostAsJsonAsync(ApiUrl, deviceData);
				}
				else
				{
					// Put
					response = await client.PutAsJsonAsync($"{ApiUrl}/{_editingDeviceId}", deviceData);
				}

				if (response.IsSuccessStatusCode)
				{
					await DisplayAlertAsync("Thành công", "Đã lưu hồ sơ thiết bị!", "OK");
					ResetForm();
					OnLoadClicked(null, null);
				}
				else
				{
					string error = await response.Content.ReadAsStringAsync();
					await DisplayAlertAsync("Thất bại", error, "OK");
				}
			}
			catch (Exception ex) { await DisplayAlertAsync("Lỗi", ex.Message, "OK"); }
		}

		// XÓA
		private async void OnDeleteClicked(object sender, EventArgs e)
		{
			if (sender is Button button && button.CommandParameter is int id)
			{
				bool answer = await DisplayAlertAsync("Xác nhận", "Bạn muốn xóa thiết bị này?", "Yes", "No");
				if (!answer) return;

				try
				{
					using HttpClient client = new HttpClient();
					var response = await client.DeleteAsync($"{ApiUrl}/{id}");

					if (response.IsSuccessStatusCode)
					{
						OnLoadClicked(null, null);
					}
				}
				catch (Exception ex) { await DisplayAlertAsync("Lỗi", ex.Message, "OK"); }
			}
		}

		// SỬA
		private void OnEditClicked(object sender, EventArgs e)
		{
			if (sender is Button button && button.CommandParameter is DeviceModel device)
			{
				// Điền thông tin cũ vào ô nhập
				TxtName.Text = device.DeviceName ?? string.Empty;
				TxtSerial.Text = device.SerialNumber ?? string.Empty;
				TxtStatus.Text = device.Status ?? string.Empty;

				TxtModel.Text = device.Model ?? string.Empty;
				TxtOrigin.Text = device.Origin ?? string.Empty;
				TxtPrice.Text = device.Price?.ToString("0") ?? string.Empty; // Bỏ số thập phân thừa
				TxtSupplier.Text = device.Supplier ?? string.Empty;
				TxtDepartment.Text = device.Department ?? string.Empty;

				if (device.PurchaseDate.HasValue)
					PickPurchaseDate.Date = device.PurchaseDate.Value;

				_editingDeviceId = device.DeviceID;

				// Đổi nút lưu thành Cập nhật
				BtnSave.Text = "💾 Cập Nhật Hồ Sơ";
				BtnSave.BackgroundColor = Color.FromArgb("#FFC107");
				BtnSave.TextColor = Colors.Black;

				LoadBtn.Text = "Hủy Bỏ Sửa";
			}
		}

		// TÌM KIẾM
		private async void OnSearchPressed(object sender, EventArgs e)
		{
			string keyword = TxtSearch.Text;
			if (string.IsNullOrWhiteSpace(keyword)) { OnLoadClicked(null, null); return; }

			try
			{
				using HttpClient client = new HttpClient();
				var devices = await client.GetFromJsonAsync<List<DeviceModel>>($"{ApiUrl}/search?keyword={keyword}");
				DevicesCollection.Clear();
				if (devices != null) foreach (var d in devices) DevicesCollection.Add(d);
			}
			catch { }
		}

		private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
		{
			if (string.IsNullOrWhiteSpace(e.NewTextValue)) OnLoadClicked(null, null);
		}

		private async void OnDetailClicked(object sender, EventArgs e)
		{
			if (sender is Button btn && btn.CommandParameter is DeviceModel device)
			{
				var navParam = new Dictionary<string, object> { { "DeviceObj", device } };
				await Shell.Current.GoToAsync(nameof(DetailPage), navParam);
			}
		}

		private void ResetForm()
		{
			TxtName.Text = ""; TxtSerial.Text = ""; TxtStatus.Text = "";
			TxtModel.Text = ""; TxtOrigin.Text = ""; TxtPrice.Text = "";
			TxtSupplier.Text = ""; TxtDepartment.Text = "";
			PickPurchaseDate.Date = DateTime.Now;

			_editingDeviceId = null;
			LoadBtn.Text = "🔄 Tải Lại Danh Sách";

			BtnSave.Text = "+ Lưu Hồ Sơ Thiết Bị";
			BtnSave.BackgroundColor = Color.FromArgb("#00C853");
			BtnSave.TextColor = Colors.White;
		}
	}
}
