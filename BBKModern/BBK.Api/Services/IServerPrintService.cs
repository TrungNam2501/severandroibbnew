namespace BBK.Api.Services;

public interface IServerPrintService
{
    void PrintExcel(string printerName, string pathFile);
}
