using BBK.Mobile.Models;
using BBK.Mobile.Services;

namespace BBK.Mobile;

public partial class MainPage : ContentPage
{
	private readonly IBbkApiClient apiClient;

	public MainPage()
	{
		apiClient = IPlatformApplication.Current!.Services.GetRequiredService<IBbkApiClient>();
		InitializeComponent();
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		await LoadMachinesAsync();
	}

	private async Task LoadMachinesAsync()
	{
		try
		{
			SetLoading(true);
			var result = await apiClient.GetMachinesAsync();
			if (!result.Success || result.Data is null)
			{
				MessageLabel.Text = result.Message;
				return;
			}

			MachinePicker.ItemsSource = result.Data.ToList();
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

	private async void OnLoginClicked(object? sender, EventArgs e)
	{
		MessageLabel.Text = "";
		var machine = MachinePicker.SelectedItem as MachineDto;
		if (machine is null)
		{
			MessageLabel.Text = "Vui lòng chọn máy";
			return;
		}

		try
		{
			SetLoading(true);
			var result = await apiClient.LoginAsync(new LoginRequest(EmployeeEntry.Text?.Trim() ?? ""));
			if (!result.Success || result.Data is null)
			{
				MessageLabel.Text = result.Message;
				return;
			}

			Session.Current = new AppSession(machine.Code, result.Data.EmployeeNo, result.Data.Name, result.Data.DepartmentNo);
			await Shell.Current.GoToAsync(nameof(MenuPage));
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

	private void SetLoading(bool isLoading)
	{
		LoadingIndicator.IsVisible = isLoading;
		LoadingIndicator.IsRunning = isLoading;
	}
}
