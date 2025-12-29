namespace MedicalDeviceApp
{
	public partial class AppShell : Shell
	{
		public AppShell()
		{
			InitializeComponent();
			// Đăng ký trang con
			Routing.RegisterRoute(nameof(DetailPage), typeof(DetailPage));
			Routing.RegisterRoute(nameof(AddEditPage), typeof(AddEditPage));
		}
	}
}