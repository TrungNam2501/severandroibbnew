using Microsoft.Extensions.Logging;
using BBK.Mobile.Services;

namespace BBK.Mobile;

public static class MauiProgram
{
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

		builder.Services.AddSingleton(sp => new HttpClient
		{
			BaseAddress = new Uri("https://localhost:7132")
		});
		builder.Services.AddSingleton<IBbkApiClient, BbkApiClient>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
