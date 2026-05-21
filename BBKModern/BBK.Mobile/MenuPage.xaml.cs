using BBK.Mobile.Models;

namespace BBK.Mobile;

public partial class MenuPage : ContentPage
{
    public MenuPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        UserLabel.Text = Session.Current is null
            ? "Chưa đăng nhập"
            : $"{Session.Current.Name} - Máy {Session.Current.MachineNo}";
    }

    private async void OnScanClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(ScanPage));
    }

    private async void OnReprintClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(ReprintPage));
    }

    private async void OnLogoutClicked(object? sender, EventArgs e)
    {
        Session.Current = null;
        await Shell.Current.GoToAsync("//MainPage");
    }
}
