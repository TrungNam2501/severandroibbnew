namespace BBK.Api.Services;

public sealed class NoOpPrintService(ILogger<NoOpPrintService> logger) : IServerPrintService
{
    public void PrintExcel(string printerName, string pathFile)
    {
        logger.LogWarning("PrintExcel skipped (non-Windows OS): printer={PrinterName}, file={PathFile}", printerName, pathFile);
    }
}
