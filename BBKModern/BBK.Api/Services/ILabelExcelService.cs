namespace BBK.Api.Services;

public interface ILabelExcelService
{
    string CreateLabelExcel(LabelPrintData data, string printerName, bool isReprint);
}
