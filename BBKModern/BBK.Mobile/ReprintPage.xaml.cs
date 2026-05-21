using BBK.Mobile.Models;
using BBK.Mobile.Services;

namespace BBK.Mobile;

public partial class ReprintPage : ContentPage
{
    private readonly IBbkApiClient apiClient;

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

    private async void OnReprintClicked(object? sender, EventArgs e)
    {
        if (Session.Current is null)
        {
            return;
        }

        var mes = MesPicker.SelectedItem as MesDto;
        var printer = PrinterPicker.SelectedItem as PrinterDto;
        if (mes is null || printer is null || !decimal.TryParse(WeightEntry.Text, out var weight))
        {
            MessageLabel.Text = "Vui lòng nhập đủ thông tin";
            return;
        }

        try
        {
            SetLoading(true);
            var request = new ReprintLabelRequest(
                Session.Current.Name,
                Session.Current.MachineNo,
                BarcodeEntry.Text?.Trim() ?? "",
                mes.PlanId,
                weight,
                printer.Code,
                string.IsNullOrWhiteSpace(RecipeEntry.Text) ? mes.RecipeCode : RecipeEntry.Text.Trim());

            var result = await apiClient.ReprintLabelAsync(request);
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
