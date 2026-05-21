using BBK.Api.Repositories;
using BBK.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

if (builder.Configuration.GetConnectionString("Erp33") is not null)
{
    builder.Services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();
    builder.Services.AddScoped<IBbkRepository, SqlBbkRepository>();
    builder.Services.AddSingleton<ILabelExcelService, LabelExcelService>();

    if (OperatingSystem.IsWindows())
    {
        builder.Services.AddSingleton<IServerPrintService, ExcelComPrintService>();
    }
    else
    {
        builder.Services.AddSingleton<IServerPrintService, NoOpPrintService>();
    }
}
else
{
    builder.Services.AddSingleton<IBbkRepository, MockBbkRepository>();
}

builder.Services.AddScoped<IBbkService, BbkService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
