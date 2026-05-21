using BBK.Mobile.Models;
using BBK.Mobile.Services;

namespace BBK.Mobile;

public partial class ScanPage : ContentPage
{
    private readonly IBbkApiClient apiClient;
    private List<MesDto>? mesList;

    public ScanPage()
    {
        apiClient = IPlatformApplication.Current!.Services.GetRequiredService<IBbkApiClient>();
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        if (Session.Current is null)
        {
            await Shell.Current.GoToAsync("//MainPage");
            return;
        }

        try
        {
            SetLoading(true);
            var mesResult = await apiClient.GetMesAsync(Session.Current.MachineNo);
            var printerResult = await apiClient.GetPrintersAsync();

            if (mesResult.Data is null || mesResult.Data.Count == 0)
            {
                await DisplayAlert("Thông Báo", "Vui lòng tạo mes", "OK");
                return;
            }

            mesList = mesResult.Data.ToList();
            var mesDisplayItems = mesList.Select(m => $"{m.PlanId} - {m.RecipeCode}").ToList();
            MesPicker.ItemsSource = mesDisplayItems;
            PrinterPicker.ItemsSource = printerResult.Data?.ToList();
            PalletEntry.Focus();
        }
        catch
        {
            MessageLabel.TextColor = Colors.Red;
            MessageLabel.Text = "Không kết nối được Server!";
        }
        finally
        {
            SetLoading(false);
        }
    }

    private void OnMesSelected(object? sender, EventArgs e)
    {
        if (MesPicker.SelectedIndex < 0 || mesList is null) return;

        var selected = mesList[MesPicker.SelectedIndex];
        RecipeEntry.Text = selected.RecipeCode.Trim();

        var recipe = RecipeEntry.Text.Trim();
        if (recipe.Length == 7 || recipe.Length == 8)
        {
            var suffixChar = recipe.Length > 6 ? recipe.Substring(6, 1) : "";
            var lastTwo = recipe.Length >= 2 ? recipe.Substring(recipe.Length - 2, 2) : "";

            if (suffixChar == "9" || lastTwo == "RM")
            {
                SanXuatRadio.IsVisible = true;
                CanDaoRadio.IsVisible = true;
                SanXuatRadio.IsChecked = false;
                CanDaoRadio.IsChecked = false;
            }
            else
            {
                SanXuatRadio.IsVisible = false;
                CanDaoRadio.IsVisible = false;
            }
        }
        else
        {
            SanXuatRadio.IsVisible = false;
            CanDaoRadio.IsVisible = false;
        }

        PalletEntry.Focus();
    }

    private void OnPalletTextChanged(object? sender, TextChangedEventArgs e)
    {
        if ((e.NewTextValue?.Length ?? 0) >= 6)
        {
            PalletEntry.IsEnabled = false;
            WeightEntry.Focus();
        }
    }

    private void OnSanXuatCheckedChanged(object? sender, CheckedChangedEventArgs e)
    {
        if (e.Value)
        {
            CanDaoRadio.IsChecked = false;
        }
    }

    private void OnCanDaoCheckedChanged(object? sender, CheckedChangedEventArgs e)
    {
        if (e.Value)
        {
            SanXuatRadio.IsChecked = false;
        }
    }

    private async void OnPrintClicked(object? sender, EventArgs e)
    {
        if (Session.Current is null) return;

        if (string.IsNullOrWhiteSpace(PalletEntry.Text))
        {
            await DisplayAlert("Thông Báo", "Vui lòng quét mã PALET!", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(WeightEntry.Text))
        {
            await DisplayAlert("Thông Báo", "Vui lòng nhập trọng lượng KEO!", "OK");
            return;
        }

        if (MesPicker.SelectedIndex < 0 || mesList is null)
        {
            await DisplayAlert("Thông Báo", "Vui lòng chọn MES", "OK");
            return;
        }

        var printer = PrinterPicker.SelectedItem as PrinterDto;
        if (printer is null)
        {
            await DisplayAlert("Thông Báo", "Vui lòng chọn máy in", "OK");
            return;
        }

        if (!decimal.TryParse(WeightEntry.Text, out var weight))
        {
            await DisplayAlert("Thông Báo", "Trọng lượng không hợp lệ!", "OK");
            return;
        }

        var selectedMes = mesList[MesPicker.SelectedIndex];

        // ReworkFlag logic from BBKV2:
        // sanxuat checked → "N", candao checked → "Y", cả 2 unchecked → "Y"
        string reworkFlag;
        if (SanXuatRadio.IsChecked)
            reworkFlag = "N";
        else if (CanDaoRadio.IsChecked)
            reworkFlag = "Y";
        else
            reworkFlag = "Y";

        try
        {
            SetLoading(true);
            var request = new PrintLabelRequest(
                RecipeEntry.Text?.Trim() ?? selectedMes.RecipeCode,
                Session.Current.MachineNo,
                printer.Name,
                Session.Current.EmployeeNo,
                reworkFlag,
                weight,
                selectedMes.PlanId,
                PalletEntry.Text?.Trim() ?? "",
                GetDeviceId(),
                OemCheckBox.IsChecked,
                Session.Current.DepartmentNo);

            var result = await apiClient.PrintLabelAsync(request);
            if (result.Success)
            {
                PalletEntry.Text = "";
                WeightEntry.Text = "";
                PalletEntry.IsEnabled = true;
                PalletEntry.Focus();
                await DisplayAlert("Thông Báo", "In Tem Thành Công!", "OK");
            }
            else
            {
                await DisplayAlert("Thông Báo", result.Message, "OK");
            }
        }
        catch
        {
            await DisplayAlert("Thông Báo", "Không kết nối được Server!", "OK");
        }
        finally
        {
            SetLoading(false);
        }
    }

    private async void OnFinishClicked(object? sender, EventArgs e)
    {
        if (Session.Current is null || MesPicker.SelectedIndex < 0 || mesList is null) return;

        var confirmed = await DisplayAlert("Xác Nhận", "Bạn có chắc chắn kết thúc mẻ keo này?", "Có", "Không");
        if (!confirmed) return;

        var selectedMes = mesList[MesPicker.SelectedIndex];

        try
        {
            SetLoading(true);
            var result = await apiClient.FinishAsync(new FinishRequest(
                RecipeEntry.Text?.Trim() ?? selectedMes.RecipeCode,
                selectedMes.PlanId,
                Session.Current.MachineNo));

            if (result.Success)
            {
                // Reload MES list
                var mesResult = await apiClient.GetMesAsync(Session.Current.MachineNo);
                if (mesResult.Data is not null)
                {
                    mesList = mesResult.Data.ToList();
                    var mesDisplayItems = mesList.Select(m => $"{m.PlanId} - {m.RecipeCode}").ToList();
                    MesPicker.ItemsSource = mesDisplayItems;
                }
                PalletEntry.Text = "";
                PalletEntry.IsEnabled = true;
                WeightEntry.Text = "";
                await DisplayAlert("Thông Báo", "Kết thúc mẻ thành công!", "OK");
            }
            else
            {
                await DisplayAlert("Thông Báo", "Lỗi Rồi!", "OK");
            }
        }
        catch
        {
            await DisplayAlert("Thông Báo", "Không kết nối được Server!", "OK");
        }
        finally
        {
            SetLoading(false);
        }
    }

    private void OnResetClicked(object? sender, EventArgs e)
    {
        PalletEntry.IsEnabled = true;
        PalletEntry.Text = "";
        WeightEntry.Text = "";
        PalletEntry.Focus();
    }

    private static string GetDeviceId()
    {
#if ANDROID
        try
        {
            var all = Java.Util.Collections.List(Java.Net.NetworkInterface.NetworkInterfaces);
            foreach (Java.Net.NetworkInterface nif in all)
            {
                if (nif.Name != "wlan0") continue;
                var macBytes = nif.GetHardwareAddress();
                if (macBytes is null) return "";
                var sb = new System.Text.StringBuilder();
                foreach (var b in macBytes)
                {
                    var hex = (b & 0xFF).ToString("x2");
                    sb.Append(hex);
                    sb.Append(':');
                }
                if (sb.Length > 0) sb.Length--;
                return sb.ToString();
            }
        }
        catch { }
        return "02:00:00:00:00:00";
#else
        return DeviceInfo.Current.Idiom.ToString();
#endif
    }

    private void SetLoading(bool isLoading)
    {
        LoadingIndicator.IsVisible = isLoading;
        LoadingIndicator.IsRunning = isLoading;
    }
}
