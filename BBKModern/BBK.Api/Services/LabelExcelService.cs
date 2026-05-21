using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;

namespace BBK.Api.Services;

public sealed class LabelExcelService(IWebHostEnvironment environment) : ILabelExcelService
{
    public string CreateLabelExcel(LabelPrintData data, string printerName, bool isReprint)
    {
        var folder = Path.Combine(environment.ContentRootPath, "Data");
        Directory.CreateDirectory(folder);
        var safePrinterName = string.Join("_", printerName.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));
        var pathFile = Path.Combine(folder, $"_{safePrinterName}_{DateTime.Now:yyyyMMddHHmmssfff}.xlsx");
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using var package = new ExcelPackage(new FileInfo(pathFile));
        var worksheet = package.Workbook.Worksheets.Add("1");
        var labelKind = ResolveLabelKind(data.GlueType);
        var shiftText = data.Shift == "1" ? "早 (Sáng)" : data.Shift == "2" ? "夜 (Đêm)" : data.Shift;
        var description = data.Description == "OEM" ? "QC-OEM" : "";

        worksheet.Cells["C2"].Value = " 建大橡膠（越南）有限公司";
        worksheet.Cells["C3"].Value = "Công ty Cao su Kenda(Việt Nam)";
        worksheet.Cells["M4"].Value = description;
        worksheet.Cells["E4"].Value = labelKind.ChineseTitle;
        worksheet.Cells["C5"].Value = "Machine BB37" + data.MachineNo;
        worksheet.Cells["E5"].Value = labelKind.VietnameseTitle;
        worksheet.Cells["N5"].Value = data.Pallet;
        worksheet.Cells["C6"].Value = "日限  Thời hạn sử dụng:  " + data.ExpirationDays + "日" + data.ExpirationDays + "Ngày";
        worksheet.Cells["C7"].Value = "禁止雨淋，油污，置地，及粉水未乾";
        worksheet.Cells["C8"].Value = "Cấm ướt mưa, dính dầu, để lên đất, bột nước chưa khô";
        worksheet.Cells["C9"].Value = "生產日期 Ngày Tháng Sản Xuất";
        worksheet.Cells["C10"].Value = data.PrintDate + " " + data.PrintTime;
        worksheet.Cells["C11"].Value = "批號";
        worksheet.Cells["C12"].Value = "Số lô";
        worksheet.Cells["E11"].Value = data.SlipNo;
        worksheet.Cells["I11"].Value = "重量";
        worksheet.Cells["I12"].Value = "Trọng lượng";
        worksheet.Cells["L11"].Style.WrapText = true;
        worksheet.Cells["L11"].Value = data.Weight + "kg";
        worksheet.Cells["C13"].Value = "有效日";
        worksheet.Cells["C14"].Value = "Ngày hiệu lực";
        worksheet.Cells["E13"].Value = data.EffectiveDate + " " + data.PrintTime;
        worksheet.Cells["I13"].Value = "班別";
        worksheet.Cells["I14"].Value = "Ca";
        worksheet.Cells["L13"].Style.WrapText = true;
        worksheet.Cells["L13"].Value = shiftText;
        worksheet.Cells["C15"].Value = "規格";
        worksheet.Cells["C16"].Value = "Quy Cách";
        worksheet.Cells["D15"].Style.WrapText = true;
        worksheet.Cells["D15"].Value = NormalizePartNoForLabel(data.PartNo, isReprint);
        worksheet.Cells["G15"].Value = "編號順序";
        worksheet.Cells["G17"].Value = "Thứ tự mã số";
        worksheet.Cells["I15"].Style.WrapText = true;
        worksheet.Cells["I15"].Value = data.BatchNo;
        worksheet.Cells["K15"].Value = "判 定";
        worksheet.Cells["K17"].Value = "Phán định";
        worksheet.Cells["M15"].Style.WrapText = true;
        worksheet.Cells["M15"].Value = "";
        worksheet.Cells["C17"].Value = "委託";
        worksheet.Cells["C18"].Value = "Ủy thác";
        worksheet.Cells["C20"].Value = labelKind.Kvs;
        worksheet.Cells["K20"].Value = "*" + data.Barcode + "*";
        worksheet.Cells["C21"].Value = labelKind.Size;
        worksheet.Cells["K21"].Value = "*" + data.Barcode + "*";

        ApplyFormat(worksheet);
        package.Save();
        return pathFile;
    }

    private static (string VietnameseTitle, string ChineseTitle, string Kvs, string Size) ResolveLabelKind(string glueType)
    {
        return glueType switch
        {
            "RC" => ("Thẻ biểu thị keo tinh luyện", "精煉膠標示卡", "KVS3J1C001.9  Rev.5", "(180mm×130mm×0.08mm)"),
            "RB" => ("Thẻ biểu thị keo cán luyên", "混煉膠標示卡", "KVS3J1C001.7  Rev.5", "(180mm×130mm×0.08mm)"),
            _ => ("Thẻ biểu thị keo xúc tiến", "加促膠標示卡", "KVS3J1C001.8  Rev.5", "(180mm×130mm×0.08mm)")
        };
    }

    private static string NormalizePartNoForLabel(string partNo, bool isReprint)
    {
        if (isReprint || partNo.Length <= 7)
        {
            return partNo.Trim();
        }

        var edge = partNo[7..];
        return string.IsNullOrWhiteSpace(edge) ? partNo.Trim() : partNo[..7].Trim() + edge.Trim();
    }

    private static void ApplyFormat(ExcelWorksheet worksheet)
    {
        using (var range = worksheet.Cells["B2:P21"])
        {
            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
            range.Style.Font.Name = "Arial";
            range.Style.Font.Size = 13;
            range.Style.Fill.BackgroundColor.SetColor(Color.White);
            range.Style.Font.Color.SetColor(Color.Black);
        }

        worksheet.Cells["M4"].Style.Font.Bold = true;
        worksheet.Cells["E4"].Style.Font.Bold = true;
        worksheet.Cells["E5"].Style.Font.Bold = true;
        worksheet.Cells["K20"].Style.Font.Name = "Code39AzaleaWide2";
        worksheet.Cells["K20"].Style.Font.Size = 28;
        worksheet.Cells["C16"].Style.Font.Size = 12;
        worksheet.Cells["M4"].Style.Font.Size = 16;
        worksheet.Cells["C4"].Style.Font.Bold = true;
        worksheet.Cells["C5"].Style.Font.Size = 11;

        foreach (var address in new[]
        {
            "C2:O2", "C3:O3", "E4:L4", "M4:O4", "C5:D5", "E5:L5", "N5:O5", "C6:O6", "C7:O7", "C8:O8", "C9:O9", "C10:O10",
            "C11:D11", "C12:D12", "E11:H12", "I11:K11", "I12:K12", "L11:O12", "C13:D13", "C14:D14", "E13:H14", "I13:K13", "I14:K14", "L13:O14",
            "D15:F16", "G15:H16", "G17:H18", "I15:J18", "K15:L16", "K17:L18", "M15:O18", "D17:F18", "C20:G20", "K20:O20", "C21:G21", "K21:O21"
        })
        {
            worksheet.Cells[address].Merge = true;
        }

        worksheet.Row(1).Height = 5;
        worksheet.Row(2).Height = 20.5;
        worksheet.Row(7).Height = 22.5;
        worksheet.Row(8).Height = 22.5;
        worksheet.Row(19).Height = 5;
        worksheet.Row(20).Height = 30;
        worksheet.Row(22).Height = 5.5;

        var widths = new double[] { 2, 2, 10, 5, 5, 5, 8, 8, 5, 5, 5, 7, 5, 5, 5, 2, 2 };
        for (var i = 0; i < widths.Length; i++)
        {
            worksheet.Column(i + 1).Width = widths[i];
        }

        worksheet.Cells["A1:P22"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        worksheet.Cells["A1:P22"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        worksheet.Cells["D15"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        worksheet.Cells["D15"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        worksheet.Cells["C7"].Style.VerticalAlignment = ExcelVerticalAlignment.Bottom;
        worksheet.Cells["C8"].Style.VerticalAlignment = ExcelVerticalAlignment.Top;
        worksheet.Cells["G15"].Style.VerticalAlignment = ExcelVerticalAlignment.Bottom;
        worksheet.Cells["K15"].Style.VerticalAlignment = ExcelVerticalAlignment.Bottom;
        worksheet.Cells["G17"].Style.VerticalAlignment = ExcelVerticalAlignment.Top;
        worksheet.Cells["K17"].Style.VerticalAlignment = ExcelVerticalAlignment.Top;
        worksheet.Cells["E14"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        worksheet.Cells["C2"].Style.VerticalAlignment = ExcelVerticalAlignment.Bottom;

        worksheet.Cells["C6:O18"].Style.Border.BorderAround(ExcelBorderStyle.Thin);
        var cell = worksheet.Cells["C6:O18"];
        var border = cell.Style.Border;
        border.Top.Style = border.Left.Style = border.Right.Style = border.Bottom.Style = ExcelBorderStyle.Thin;

        foreach (var address in new[] { "C7:O7", "C11:K11", "C13:D13", "I13:K13", "C15", "C17", "G16:H16", "K16:L16" })
        {
            worksheet.Cells[address].Style.Border.Bottom.Style = ExcelBorderStyle.None;
        }

        foreach (var address in new[] { "C8:O8", "C12:K12", "C14:D14", "I14:K14", "C16", "C18", "G17:H17", "K17:L17" })
        {
            worksheet.Cells[address].Style.Border.Top.Style = ExcelBorderStyle.None;
        }

        worksheet.Cells["B2:P22"].Style.Border.BorderAround(ExcelBorderStyle.Medium);
        worksheet.PrinterSettings.TopMargin = 0.2m;
        worksheet.PrinterSettings.LeftMargin = 0.8m;
        worksheet.PrinterSettings.RightMargin = 0.5m;
        worksheet.PrinterSettings.PaperSize = ePaperSize.A5;
        worksheet.PrinterSettings.Orientation = eOrientation.Landscape;


        //// ---- CẤU HÌNH SANG KHỔ A4 VÀ LÀM TRÒN VÙNG IN ----

        //// 1. Đổi khổ giấy sang A4
        //worksheet.PrinterSettings.PaperSize = ePaperSize.A4;
        //worksheet.PrinterSettings.Orientation = eOrientation.Landscape; // Giữ nguyên hướng ngang (hoặc eOrientation.Portrait nếu muốn dọc)

        //// 2. Thiết lập lề chuẩn cho khổ A4 (Đơn vị tính bằng inch)
        //worksheet.PrinterSettings.TopMargin = 0.4m;
        //worksheet.PrinterSettings.BottomMargin = 0.4m;
        //worksheet.PrinterSettings.LeftMargin = 0.4m;
        //worksheet.PrinterSettings.RightMargin = 0.4m;

        //// 3. ÉP EXCEL FIT VỪA KHÍT 1 TRANG A4 (Chống lỗi in ra giấy trắng cực kỳ hiệu quả)
        //worksheet.PrinterSettings.FitToPage = true;
        //worksheet.PrinterSettings.FitToWidth = 1;  // Ép chiều ngang vừa đúng 1 trang
        //worksheet.PrinterSettings.FitToHeight = 1; // Ép chiều dọc vừa đúng 1 trang

        //// 4. Định nghĩa rõ ràng vùng in (Chỉ in từ cột B đến cột P, từ dòng 2 đến dòng 22)
        //// Việc này giúp máy in bỏ qua các ô trống rác xung quanh, không sợ bị xả giấy trắng
        //worksheet.PrinterSettings.PrintArea = worksheet.Cells["B2:P22"];
    }
}
