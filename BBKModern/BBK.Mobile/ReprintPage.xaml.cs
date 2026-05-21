using BBK.Mobile.Models;
using BBK.Mobile.Services;

namespace BBK.Mobile;

public partial class ReprintPage : ContentPage
{
    private readonly IBbkApiClient apiClient;
    private List<MesDto>? mesList;
    private List<BarcodeDto>? barcodeList;

    public ReprintPage()
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
            var mesResult = await apiClient.GetMesForReprintAsync(Session.Current.MachineNo);
            var printerResult = await apiClient.GetPrintersAsync();

            if (mesResult.Data is not null && mesResult.Data.Count > 0)
            {
                mesList = mesResult.Data.ToList();
                var mesDisplayItems = mesList.Select(m => $"{m.PlanId} - {m.RecipeCode}").ToList();
                MesPicker.ItemsSource = mesDisplayItems;
            }
            else
            {
                mesList = null;
                MesPicker.ItemsSource = null;
            }

            PrinterPicker.ItemsSource = printerResult.Data?.ToList();
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

    private async void OnMesSelected(object? sender, EventArgs e)
    {
        if (Session.Current is null || MesPicker.SelectedIndex < 0 || mesList is null) return;

        var selectedMes = mesList[MesPicker.SelectedIndex];
        RecipeEntry.Text = selectedMes.RecipeCode.Trim();

        try
        {
            SetLoading(true);
            var barcodeResult = await apiClient.GetBarcodesAsync(selectedMes.PlanId, Session.Current.MachineNo);
            if (barcodeResult.Data is not null && barcodeResult.Data.Count > 0)
            {
                barcodeList = barcodeResult.Data.ToList();
                var barcodeDisplayItems = barcodeList.Select(b => $"{b.Barcode} - {b.Weight}").ToList();
                BarcodePicker.ItemsSource = barcodeDisplayItems;
            }
            else
            {
                barcodeList = null;
                BarcodePicker.ItemsSource = null;
                await DisplayAlert("Thông Báo", "Không có mã tem của MES này", "OK");
            }
        }
        catch
        {
            MessageLabel.TextColor = Colors.Red;
            MessageLabel.Text = "Lỗi tải barcode!";
        }
        finally
        {
            SetLoading(false);
        }
    }

    private async void OnReprintClicked(object? sender, EventArgs e)
    {
        if (Session.Current is null) return;

        if (MesPicker.SelectedIndex < 0 || mesList is null)
        {
            await DisplayAlert("Thông Báo", "Vui lòng chọn MES", "OK");
            return;
        }

        if (BarcodePicker.SelectedIndex < 0 || barcodeList is null)
        {
            await DisplayAlert("Thông Báo", "Vui lòng chọn mã tem", "OK");
            return;
        }

        var printer = PrinterPicker.SelectedItem as PrinterDto;
        if (printer is null)
        {
            await DisplayAlert("Thông Báo", "Vui lòng chọn máy in", "OK");
            return;
        }

        var selectedMes = mesList[MesPicker.SelectedIndex];
        var selectedBarcode = barcodeList[BarcodePicker.SelectedIndex];

        try
        {
            SetLoading(true);
            var request = new ReprintLabelRequest(
                Session.Current.Name,
                Session.Current.MachineNo,
                selectedBarcode.Barcode,
                selectedMes.PlanId,
                selectedBarcode.Weight,
                printer.Name,
                RecipeEntry.Text?.Trim() ?? selectedMes.RecipeCode);

            var result = await apiClient.ReprintLabelAsync(request);
            if (result.Success)
            {
                await DisplayAlert("Thông Báo", "In lại tem thành công!", "OK");
            }
            else
            {
                await DisplayAlert("Thông Báo", result.Message ?? "Lỗi không in lại tem được!", "OK");
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

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    private void SetLoading(bool isLoading)
    {
        LoadingIndicator.IsVisible = isLoading;
        LoadingIndicator.IsRunning = isLoading;
    }
}
