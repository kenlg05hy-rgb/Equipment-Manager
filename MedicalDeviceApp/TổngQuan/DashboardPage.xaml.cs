using System.Net.Http.Json;

namespace MedicalDeviceApp
{
    public partial class DashboardPage : ContentPage
    {
        private const string ApiUrl = "http://localhost:5244/api/Devices/stats"; // Link API

        public DashboardPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            LoadStats();
        }

        private async void OnRefreshClicked(object sender, EventArgs e)
        {
            LoadStats();
        }

        private async void LoadStats()
        {
            try
            {
                using HttpClient client = new HttpClient();
                var stats = await client.GetFromJsonAsync<DashboardModel>(ApiUrl);

                if (stats != null)
                {
                    LblTotalDevices.Text = stats.TotalDevices.ToString();
                    LblTotalValue.Text = stats.FormattedValue;
                    LblGood.Text = stats.GoodCount.ToString();
                    LblBroken.Text = stats.BrokenCount.ToString();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Lỗi", "Không tải được thống kê: " + ex.Message, "OK");
            }
        }
    }
}