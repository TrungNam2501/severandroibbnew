using BBK.Mobile.Models;
using BBK.Mobile.Services;

namespace BBK.Mobile;

public partial class MainPage : ContentPage
{
	private readonly IBbkApiClient apiClient;
	private bool isLoggingIn;

	public MainPage()
	{
		apiClient = IPlatformApplication.Current!.Services.GetRequiredService<IBbkApiClient>();
		InitializeComponent();
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		EmployeeEntry.Text = "";
		MessageLabel.Text = "";
		await LoadMachinesAsync();
		EmployeeEntry.Focus();
	}

	private async Task LoadMachinesAsync()
	{
		try
		{
			SetLoading(true);
			var result = await apiClient.GetMachinesAsync();
			if (!result.Success || result.Data is null)
			{
				MessageLabel.TextColor = Colors.Red;
				MessageLabel.Text = result.Message;
				return;
			}

			MachinePicker.ItemsSource = result.Data.ToList();
		}
		catch
		{
			MessageLabel.TextColor = Colors.Red;
			MessageLabel.Text = "Không kết nối được Server!\r\nVui lòng kiểm tra lại";
		}
		finally
		{
			SetLoading(false);
		}
	}

	private async void OnEmployeeTextChanged(object? sender, TextChangedEventArgs e)
	{
		if (e.NewTextValue?.Length == 6 && !isLoggingIn)
		{
			await DoLoginAsync();
		}
	}

	private async void OnLoginClicked(object? sender, EventArgs e)
	{
		await DoLoginAsync();
	}

	private async Task DoLoginAsync()
	{
		if (isLoggingIn) return;

		var empText = EmployeeEntry.Text?.Trim() ?? "";
		if (empText.Length != 6)
		{
			MessageLabel.TextColor = Colors.Red;
			MessageLabel.Text = "Sai mã số thẻ!!\r\nVui lòng kiểm tra lại";
			return;
		}

		var machine = MachinePicker.SelectedItem as MachineDto;
		if (machine is null)
		{
			MessageLabel.TextColor = Colors.Red;
			MessageLabel.Text = "Vui lòng chọn máy";
			return;
		}

		try
		{
			isLoggingIn = true;
			SetLoading(true);
			MessageLabel.Text = "";

			var result = await apiClient.LoginAsync(new LoginRequest(empText));
			if (!result.Success || result.Data is null)
			{
				MessageLabel.TextColor = Colors.Red;
				MessageLabel.Text = "Sai mã số thẻ!!\r\nVui lòng kiểm tra lại";
				return;
			}

			MessageLabel.TextColor = Colors.Black;
			MessageLabel.Text = $"{result.Data.Name} - {result.Data.DepartmentNo}";
			Session.Current = new AppSession(machine.Code, result.Data.EmployeeNo, result.Data.Name, result.Data.DepartmentNo);

			if (!string.IsNullOrEmpty(Session.Current.MachineNo) &&
			    !string.IsNullOrEmpty(Session.Current.DepartmentNo) &&
			    !string.IsNullOrEmpty(Session.Current.EmployeeNo) &&
			    !string.IsNullOrEmpty(Session.Current.Name))
			{
				await Shell.Current.GoToAsync(nameof(MenuPage));
			}
		}
		catch
		{
			MessageLabel.TextColor = Colors.Red;
			MessageLabel.Text = "Không kết nối được Server!\r\nVui lòng kiểm tra lại";
		}
		finally
		{
			isLoggingIn = false;
			SetLoading(false);
		}
	}

	private void OnExitClicked(object? sender, EventArgs e)
	{
		Session.Current = null;
#if ANDROID
		Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
#else
		Application.Current?.Quit();
#endif
	}

	private void SetLoading(bool isLoading)
	{
		LoadingIndicator.IsVisible = isLoading;
		LoadingIndicator.IsRunning = isLoading;
	}
}
