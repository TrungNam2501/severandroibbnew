using BBK.Api.Models;
using BBK.Api.Services;
using Microsoft.Data.SqlClient;

namespace BBK.Api.Repositories;

public sealed class SqlBbkRepository(
    ISqlConnectionFactory connectionFactory,
    ILabelExcelService labelExcelService,
    IServerPrintService serverPrintService) : IBbkRepository
{
    public async Task<LoginResponse?> FindEmployeeAsync(string employeeNo, CancellationToken cancellationToken)
    {
        const string sql = "Select name, depno, empno from peremp where empno = @empno and subno = '4'";
        await using var connection = connectionFactory.Create("Erp34");
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@empno", employeeNo);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new LoginResponse(
            reader["empno"].ToString()!.Trim(),
            reader["name"].ToString()!.Trim(),
            reader["depno"].ToString()!.Trim());
    }

    public Task<IReadOnlyList<MachineResponse>> GetMachinesAsync(CancellationToken cancellationToken)
    {
        IReadOnlyList<MachineResponse> machines = Enumerable.Range(1, 8)
            .Select(index => new MachineResponse(index.ToString("00"), index.ToString("00")))
            .ToArray();

        return Task.FromResult(machines);
    }

    public async Task<IReadOnlyList<MesResponse>> GetMesListAsync(string machineNo, CancellationToken cancellationToken)
    {
        var shiftId = ShiftHelper.GetShiftMayBb(DateTime.Now);
        var pday = ShiftHelper.GetProductionDateForBb(shiftId, DateTime.Now).ToString("yyyy-MM-dd");
        const string sql = "select a.Plan_Id, a.Recipe_Code FROM [mfnsShareDB].[dbo].[IF_RtPlan2Mixing] a, [mfns].[dbo].[Ppt_GroupLot] b where a.Shift_Id = @shiftId and a.P_Date = @pday and a.Plan_Id not like 'V%' and a.Plan_Id = b.MesPlanID and b.End_datetime is not null";

        await using var connection = connectionFactory.CreateForMachine(machineNo);
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@shiftId", shiftId);
        command.Parameters.AddWithValue("@pday", pday);

        var result = new List<MesResponse>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(new MesResponse(reader["Plan_Id"].ToString()!.Trim(), reader["Recipe_Code"].ToString()!.Trim()));
        }

        return result;
    }

    public async Task<IReadOnlyList<PrinterResponse>> GetPrintersAsync(CancellationToken cancellationToken)
    {
        const string sql = "SELECT [TenMay], [MaMay] FROM [BB].[dbo].[Printer_BB] where IP = @ip";
        await using var connection = connectionFactory.Create("Erp33");
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@ip", "BB");

        var result = new List<PrinterResponse>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var code = reader["MaMay"].ToString()!.Trim();
            var name = reader["TenMay"].ToString()!.Trim();
            result.Add(new PrinterResponse(code, string.IsNullOrWhiteSpace(name) ? code : name));
        }

        return result;
    }

    public async Task<IReadOnlyList<BarcodeResponse>> GetBarcodesAsync(string mesId, string machineNo, CancellationToken cancellationToken)
    {
        const string sql = "Select barcode, weight from prdebe where subno='4' and factory='V' and mesid = @mesid and machno = @machno";
        await using var connection = connectionFactory.Create("Erp33");
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@mesid", mesId);
        command.Parameters.AddWithValue("@machno", $"V-BB37{machineNo.Trim()}");

        var result = new List<BarcodeResponse>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var weight = decimal.TryParse(reader["weight"].ToString(), out var parsedWeight) ? parsedWeight : 0;
            result.Add(new BarcodeResponse(reader["barcode"].ToString()!.Trim(), weight));
        }

        return result;
    }

    public async Task<PrintLabelResponse> PrintLabelAsync(PrintLabelRequest request, CancellationToken cancellationToken)
    {
        var result = await PrintLabelCoreAsync(request, cancellationToken);
        return result;
    }

    public async Task ReprintLabelAsync(ReprintLabelRequest request, CancellationToken cancellationToken)
    {
        const string checkMesSql = "select Plan_Num from [mfnsShareDB].[dbo].[IF_RtPlan2Mixing] where Plan_Id = @mesid";
        await using (var machineConnection = connectionFactory.CreateForMachine(request.MachineNo))
        {
            await machineConnection.OpenAsync(cancellationToken);
            await using var checkMesCommand = new SqlCommand(checkMesSql, machineConnection);
            checkMesCommand.Parameters.AddWithValue("@mesid", request.MesId);
            var planNum = await checkMesCommand.ExecuteScalarAsync(cancellationToken);
            if (planNum is null)
            {
                throw new InvalidOperationException("MES không tồn tại [IF_RtPlan2Mixing], tạo MES khác!");
            }
        }

        const string checkOemSql = "SELECT [Barcode] FROM [BB].[dbo].[TemOEMBB] where Barcode = @barcode";
        var description = "";
        await using var erpConnection = connectionFactory.Create("Erp33");
        await erpConnection.OpenAsync(cancellationToken);
        await using (var checkOemCommand = new SqlCommand(checkOemSql, erpConnection))
        {
            checkOemCommand.Parameters.AddWithValue("@barcode", request.Barcode.Trim());
            var oem = await checkOemCommand.ExecuteScalarAsync(cancellationToken);
            if (oem is not null)
            {
                description = "OEM";
            }
        }

        const string checkLabelSql = "Select barcode, slipno, partno, weight, pallet_no, indat, effdat, intime, daylimt, prodat, class, some_sx from prdebe where subno='4' and factory='V' and machno = @machno and mesid = @mesid and barcode = @barcode";
        await using var checkLabelCommand = new SqlCommand(checkLabelSql, erpConnection);
        checkLabelCommand.Parameters.AddWithValue("@machno", $"V-BB37{request.MachineNo.Trim()}");
        checkLabelCommand.Parameters.AddWithValue("@mesid", request.MesId);
        checkLabelCommand.Parameters.AddWithValue("@barcode", request.Barcode.Trim());
        await using var reader = await checkLabelCommand.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            throw new InvalidOperationException("Không có dữ liệu TEM này!");
        }

        var labelData = new LabelPrintData(
            reader["partno"].ToString()!.Trim(),
            reader["effdat"].ToString()!.Trim(),
            reader["barcode"].ToString()!.Trim()[..2],
            reader["slipno"].ToString()!.Trim(),
            request.Barcode.Trim(),
            reader["slipno"].ToString()!.Trim()[..1],
            reader["daylimt"].ToString()!.Trim(),
            reader["weight"].ToString()!.Trim(),
            request.MesId.Trim(),
            request.MachineNo.Trim(),
            reader["prodat"].ToString()!.Trim(),
            reader["indat"].ToString()!.Trim(),
            reader["class"].ToString()!.Trim(),
            reader["intime"].ToString()!.Trim(),
            reader["pallet_no"].ToString()!.Trim(),
            description,
            reader["some_sx"].ToString()!.Trim());

        var pathFile = labelExcelService.CreateLabelExcel(labelData, request.PrinterName.Trim(), isReprint: true);
        try
        {
            serverPrintService.PrintExcel(request.PrinterName.Trim(), pathFile);
        }
        finally
        {
            DeleteFileIfExists(pathFile);
        }
    }

    private async Task<PrintLabelResponse> PrintLabelCoreAsync(PrintLabelRequest request, CancellationToken cancellationToken)
    {
        var now = DateTime.Now;
        var shiftId = ShiftHelper.GetShift(now);
        var productionDate = ShiftHelper.GetProductionDate(shiftId, now);
        var pday = productionDate.ToString("yyyyMMdd");
        var partNo = request.RecipeCode.Trim();
        var machineFullNo = $"V-BB37{request.MachineNo.Trim()}";
        var slipNo = $"{shiftId}{request.MachineNo.Trim()}-{pday.Substring(4, 4)}";
        var glue = GlueHelper.Resolve(partNo);

        await ValidateMesAsync(request, partNo, pday, cancellationToken);
        var expirationDays = await GetExpirationDaysAsync(glue.GlueType, partNo, cancellationToken);
        var barcode = await CreateNextBarcodeAsync(glue.GlueType, pday, cancellationToken);
        await EnsurePalletAsync(request.PalletNo.Trim(), request.EmployeeNo, cancellationToken);

        const string insertSql = "INSERT INTO [dbo].[prdebe] VALUES ('4', 'V', @mesid, @machno, @daylimt, @barcode, @slipno, @weight, @prodat, @effdat, @class, @ptype, @candao, @partno, @intime, @indat, @usrno, @pallet, @active, @somesx)";
        await using var connection = connectionFactory.Create("Erp33");
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand(insertSql, connection);
        command.Parameters.AddWithValue("@mesid", request.MesId.Trim());
        command.Parameters.AddWithValue("@machno", machineFullNo);
        command.Parameters.AddWithValue("@daylimt", expirationDays.ToString());
        command.Parameters.AddWithValue("@barcode", barcode);
        command.Parameters.AddWithValue("@slipno", slipNo);
        command.Parameters.AddWithValue("@weight", request.Weight.ToString());
        command.Parameters.AddWithValue("@prodat", pday);
        command.Parameters.AddWithValue("@effdat", now.AddDays(expirationDays).ToString("yyyyMMdd"));
        command.Parameters.AddWithValue("@class", shiftId);
        command.Parameters.AddWithValue("@ptype", glue.ProductType);
        command.Parameters.AddWithValue("@candao", glue.GlueType == "RB" ? "N" : request.ReworkFlag.Trim());
        command.Parameters.AddWithValue("@partno", partNo);
        command.Parameters.AddWithValue("@intime", now.ToString("HH:mm:ss"));
        command.Parameters.AddWithValue("@indat", now.ToString("yyyyMMdd"));
        command.Parameters.AddWithValue("@usrno", request.EmployeeNo.Trim());
        command.Parameters.AddWithValue("@pallet", request.PalletNo.Trim());
        command.Parameters.AddWithValue("@active", glue.ProductType == "3" ? "N" : glue.ProductType == "2" ? "" : "Y");
        command.Parameters.AddWithValue("@somesx", "1");

        var affected = await command.ExecuteNonQueryAsync(cancellationToken);
        if (affected <= 0)
        {
            throw new InvalidOperationException("Đợi 1 chút rồi quét lại!!!");
        }

        if (request.IsOem)
        {
            await InsertOemLabelAsync(request.MesId.Trim(), partNo, barcode, now, cancellationToken);
        }

        var labelData = new LabelPrintData(
            partNo,
            now.AddDays(expirationDays).ToString("yyyyMMdd"),
            glue.GlueType,
            slipNo,
            barcode,
            shiftId,
            expirationDays.ToString(),
            request.Weight.ToString(),
            request.MesId.Trim(),
            request.MachineNo.Trim(),
            pday,
            now.ToString("yyyyMMdd"),
            shiftId,
            now.ToString("HH:mm:ss"),
            request.PalletNo.Trim(),
            request.IsOem ? "OEM" : "",
            "1");

        var pathFile = labelExcelService.CreateLabelExcel(labelData, request.PrinterName.Trim(), isReprint: false);
        try
        {
            serverPrintService.PrintExcel(request.PrinterName.Trim(), pathFile);
        }
        finally
        {
            DeleteFileIfExists(pathFile);
        }

        return new PrintLabelResponse(barcode, "1");
    }

    private async Task ValidateMesAsync(PrintLabelRequest request, string partNo, string pday, CancellationToken cancellationToken)
    {
        const string sql = "Select Plan_Id, P_Date from [mfnsShareDB].[dbo].[IF_RtPlan2Mixing] where Plan_Id = @mesid";
        await using var connection = connectionFactory.CreateForMachine(request.MachineNo);
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@mesid", request.MesId.Trim());
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            throw new InvalidOperationException("Mã MES đã bị đóng! Liên hệ IT mở!");
        }

        var mesDate = reader["P_Date"].ToString()!.Trim().Replace("-", "");
        if (!string.Equals(mesDate, pday, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Mes quá giờ không quét được");
        }
    }

    private async Task<int> GetExpirationDaysAsync(string glueType, string partNo, CancellationToken cancellationToken)
    {
        const string sql = "select expday from [erp].[dbo].[prdexp] where subno='4' and factory='V' and ptype = @ptype and rubno = @rubno";
        await using var connection = connectionFactory.Create("Erp33");
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@ptype", glueType);
        command.Parameters.AddWithValue("@rubno", partNo.Substring(0, Math.Min(5, partNo.Length)));
        var value = await command.ExecuteScalarAsync(cancellationToken);
        if (value is null || !int.TryParse(value.ToString(), out var days))
        {
            throw new InvalidOperationException("Mã keo không được sử dụng. Liên hệ Duyên phòng chế tạo (755) !");
        }

        return days;
    }

    private async Task<string> CreateNextBarcodeAsync(string glueType, string pday, CancellationToken cancellationToken)
    {
        var productionDate = DateTime.ParseExact(pday, "yyyyMMdd", null);
        var month = productionDate.Month switch
        {
            10 => "A",
            11 => "B",
            12 => "C",
            _ => productionDate.Month.ToString()[^1].ToString()
        };
        var dateCode = $"{productionDate:yy}{month}{productionDate:dd}";
        const string sql = "SELECT MAX(SUBSTRING(Barcode,8,3)) FROM [erp].[dbo].[prdebe] where subno='4' and factory='V' and barcode like @barcodeLike and prodat = @prodat";
        await using var connection = connectionFactory.Create("Erp33");
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@barcodeLike", $"%{glueType}%");
        command.Parameters.AddWithValue("@prodat", pday);
        var value = await command.ExecuteScalarAsync(cancellationToken);
        var next = value is null || string.IsNullOrWhiteSpace(value.ToString()) ? 1 : int.Parse(value.ToString()!) + 1;
        return $"{glueType}{dateCode}{next:000}";
    }

    private async Task EnsurePalletAsync(string palletNo, string employeeNo, CancellationToken cancellationToken)
    {
        if (!PalletHelper.IsValid(palletNo))
        {
            throw new InvalidOperationException("Pallet Không hợp lệ, nhập lại!!!");
        }

        const string selectSql = "SELECT PALLET_NO FROM [InTem].[dbo].[PalletBB] WHERE PALLET_NO = @pallet";
        await using var connection = connectionFactory.Create("InTem");
        await connection.OpenAsync(cancellationToken);
        await using var selectCommand = new SqlCommand(selectSql, connection);
        selectCommand.Parameters.AddWithValue("@pallet", palletNo);
        var exists = await selectCommand.ExecuteScalarAsync(cancellationToken);
        if (exists is not null)
        {
            return;
        }

        const string insertSql = "INSERT INTO [InTem].[dbo].[PalletBB] VALUES(@pallet, @date, @employeeNo, '1', 'Y')";
        await using var insertCommand = new SqlCommand(insertSql, connection);
        insertCommand.Parameters.AddWithValue("@pallet", palletNo);
        insertCommand.Parameters.AddWithValue("@date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        insertCommand.Parameters.AddWithValue("@employeeNo", employeeNo.Trim());
        var affected = await insertCommand.ExecuteNonQueryAsync(cancellationToken);
        if (affected <= 0)
        {
            throw new InvalidOperationException("Lỗi cập nhật Pallet PalletBB");
        }
    }

    private async Task InsertOemLabelAsync(string mesId, string partNo, string barcode, DateTime now, CancellationToken cancellationToken)
    {
        const string sql = "INSERT INTO [BB].[dbo].[TemOEMBB] ([mesid], [partno], [Barcode], [indat], [intime]) VALUES(@mesid, @partno, @barcode, @indat, @intime)";
        await using var connection = connectionFactory.Create("Erp33");
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@mesid", mesId);
        command.Parameters.AddWithValue("@partno", partNo);
        command.Parameters.AddWithValue("@barcode", barcode);
        command.Parameters.AddWithValue("@indat", now.ToString("yyyyMMdd"));
        command.Parameters.AddWithValue("@intime", now.ToString("HH:mm:ss"));
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static void DeleteFileIfExists(string pathFile)
    {
        try
        {
            if (File.Exists(pathFile))
            {
                File.Delete(pathFile);
            }
        }
        catch
        {
        }
    }
}
