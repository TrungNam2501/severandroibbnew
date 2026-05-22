using Microsoft.Extensions.Logging;
using BBK.Mobile.Services;

namespace BBK.Mobile;

public static class MauiProgram
{
	// Đổi URL này khi deploy lên server thật, ví dụ: "http://198.1.9.245:5222"
	// Debug Android Emulator: dùng 10.0.2.2 (emulator trỏ về host PC)
	// Debug Android Device (cùng WiFi): dùng IP máy tính, ví dụ "http://192.168.1.x:5222"
#if DEBUG
	private const string ApiBaseUrl = "http://10.0.2.2:5222";
#else
	private const string ApiBaseUrl = "http://localhost:5222";
#endif

	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		// Bỏ qua SSL certificate khi debug
		builder.Services.AddSingleton(sp =>
		{
			var handler = new HttpClientHandler
			{
				ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
			};
			return new HttpClient(handler)
			{
				BaseAddress = new Uri(ApiBaseUrl)
			};
		});
#else
		builder.Services.AddSingleton(sp => new HttpClient
		{
			BaseAddress = new Uri(ApiBaseUrl)
		});
#endif

		builder.Services.AddSingleton<IBbkApiClient, BbkApiClient>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
