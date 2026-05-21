namespace BBK.Mobile;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // XÓA HOẶC COMMENT DÒNG NÀY:
        // Routing.RegisterRoute(nameof(MainPage), typeof(MainPage)); 

        // Giữ lại các trang con (các trang không khai báo trong file XAML)
        Routing.RegisterRoute(nameof(MenuPage), typeof(MenuPage));
        Routing.RegisterRoute(nameof(ScanPage), typeof(ScanPage));
        Routing.RegisterRoute(nameof(ReprintPage), typeof(ReprintPage));
    }
}