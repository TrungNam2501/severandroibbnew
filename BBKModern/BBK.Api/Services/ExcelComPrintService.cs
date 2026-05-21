using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace BBK.Api.Services;

[SupportedOSPlatform("windows")]
public sealed class ExcelComPrintService(ILogger<ExcelComPrintService> logger) : IServerPrintService
{
    public void PrintExcel(string printerName, string pathFile)
    {
        if (!OperatingSystem.IsWindows())
        {
            throw new InvalidOperationException("Chức năng in Excel chỉ hỗ trợ server Windows");
        }

        if (printerName.StartsWith("Fax", StringComparison.OrdinalIgnoreCase)
            || printerName.StartsWith("Foxit", StringComparison.OrdinalIgnoreCase)
            || printerName.StartsWith("Microsoft", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Chon lai may in");
        }

        Type? excelType = Type.GetTypeFromProgID("Excel.Application");
        if (excelType is null)
        {
            throw new InvalidOperationException("Server chưa cài Microsoft Excel để in tem");
        }

        object? excelApp = null;
        object? workbooks = null;
        object? workbook = null;
        object? worksheets = null;

        try
        {
            excelApp = Activator.CreateInstance(excelType);
            if (excelApp is null)
            {
                throw new InvalidOperationException("Không khởi tạo được Excel trên server");
            }

            excelType.InvokeMember("DisplayAlerts", System.Reflection.BindingFlags.SetProperty, null, excelApp, new object[] { false });
            workbooks = excelType.InvokeMember("Workbooks", System.Reflection.BindingFlags.GetProperty, null, excelApp, null);
            workbook = workbooks!.GetType().InvokeMember("Open", System.Reflection.BindingFlags.InvokeMethod, null, workbooks, new object[] { pathFile });
            worksheets = workbook!.GetType().InvokeMember("Worksheets", System.Reflection.BindingFlags.GetProperty, null, workbook, null);
            worksheets!.GetType().InvokeMember("PrintOut", System.Reflection.BindingFlags.InvokeMethod, null, worksheets, new object?[] { Type.Missing, Type.Missing, Type.Missing, Type.Missing, printerName, Type.Missing, Type.Missing, Type.Missing });
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Server Excel print failed for {PathFile} on {PrinterName}", pathFile, printerName);
            throw new InvalidOperationException("Lỗi kẹt lệnh in 9.245!");
        }
        finally
        {
            TryCloseWorkbook(workbook);
            TryQuitExcel(excelApp);
            ReleaseCom(worksheets);
            ReleaseCom(workbook);
            ReleaseCom(workbooks);
            ReleaseCom(excelApp);
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }

    private static void TryCloseWorkbook(object? workbook)
    {
        try
        {
            workbook?.GetType().InvokeMember("Close", System.Reflection.BindingFlags.InvokeMethod, null, workbook, new object[] { false });
        }
        catch
        {
        }
    }

    private static void TryQuitExcel(object? excelApp)
    {
        try
        {
            excelApp?.GetType().InvokeMember("Quit", System.Reflection.BindingFlags.InvokeMethod, null, excelApp, null);
        }
        catch
        {
        }
    }

    private static void ReleaseCom(object? value)
    {
        if (value is not null && Marshal.IsComObject(value))
        {
            Marshal.FinalReleaseComObject(value);
        }
    }
}
