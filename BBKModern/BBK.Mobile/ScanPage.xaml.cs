using BBK.Mobile.Models;
using BBK.Mobile.Services;

namespace BBK.Mobile;

public partial class ScanPage : ContentPage
{
    private readonly IBbkApiClient apiClient;

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
            MesPicker.ItemsSource = mesResult.Data?.ToList();
            PrinterPicker.ItemsSource = printerResult.Data?.ToList();
        }
        catch (Exception ex)
        {
            MessageLabel.Text = ex.Message;
        }
        finally
        {
            SetLoading(false);
        }
    }

    private async void OnPrintClicked(object? sender, EventArgs e)
    {
        if (Session.Current is null)
        {
            return;
        }

        var mes = MesPicker.SelectedItem as MesDto;
        var printer = PrinterPicker.SelectedItem as PrinterDto;
        if (mes is null || printer is null || !decimal.TryParse(WeightEntry.Text, out var weight))
        {
            MessageLabel.Text = "Vui lòng nhập đủ MES, máy in và trọng lượng";
            return;
        }

        try
        {
            SetLoading(true);
            var request = new PrintLabelRequest(
                string.IsNullOrWhiteSpace(RecipeEntry.Text) ? mes.RecipeCode : RecipeEntry.Text.Trim(),
                Session.Current.MachineNo,
                printer.Code,
                Session.Current.EmployeeNo,
                ReworkCheckBox.IsChecked ? "Y" : "N",
                weight,
                mes.PlanId,
                PalletEntry.Text?.Trim() ?? "",
                DeviceInfo.Current.Idiom.ToString(),
                OemCheckBox.IsChecked,
                Session.Current.DepartmentNo);

            var result = await apiClient.PrintLabelAsync(request);
            MessageLabel.TextColor = result.Success ? Colors.Green : Colors.Red;
            MessageLabel.Text = result.Message;
        }
        catch (Exception ex)
        {
            MessageLabel.TextColor = Colors.Red;
            MessageLabel.Text = ex.Message;
        }
        finally
        {
            SetLoading(false);
        }
    }

    private void SetLoading(bool isLoading)
    {
        LoadingIndicator.IsVisible = isLoading;
        LoadingIndicator.IsRunning = isLoading;
    }
}
