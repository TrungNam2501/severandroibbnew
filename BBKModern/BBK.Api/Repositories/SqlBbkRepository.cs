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

    public async Task<IReadOnlyList<MesResponse>> GetMesListForReprintAsync(string machineNo, CancellationToken cancellationToken)
    {
        var shiftId = ShiftHelper.GetShiftMayBb(DateTime.Now);
        var pdateObj = ShiftHelper.GetProductionDateForBb(shiftId, DateTime.Now);
        var pday = pdateObj.ToString("yyyy-MM-dd");
        const string sql = "select a.Plan_Id, a.Recipe_Code FROM [mfnsShareDB].[dbo].[IF_RtPlan2Mixing] a, [mfns].[dbo].[Ppt_GroupLot] b where a.Shift_Id = @shiftId and a.P_Date = @pday and a.Plan_Id = b.MesPlanID and b.End_datetime is not null";

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
        var now = DateTime.Now;
        var shiftId = ShiftHelper.GetShift(now);
        var productionDate = ShiftHelper.GetProductionDate(shiftId, now);
        var pday = productionDate.ToString("yyyyMMdd");

        return await PrintLabelCoreAsync(request.RecipeCode.Trim(), request.MachineNo.Trim(), request.PrinterName.Trim(),
            shiftId, request.EmployeeNo.Trim(), request.ReworkFlag, request.Weight, request.MesId.Trim(),
            request.PalletNo.Trim(), request.IsOem, pday, now.ToString("yyyyMMdd"), now.ToString("HH:mm:ss"),
            productionDate, cancellationToken);
    }

    public async Task<PrintLabelResponse> CompensatePrintLabelAsync(CompensatePrintRequest request, CancellationToken cancellationToken)
    {
        var productionDate = DateTime.ParseExact(request.ProductionDate.Trim(), "yyyyMMdd", null);

        return await PrintLabelCoreAsync(request.RecipeCode.Trim(), request.MachineNo.Trim(), request.PrinterName.Trim(),
            request.ShiftId.Trim(), request.EmployeeNo.Trim(), request.ReworkFlag, request.Weight, request.MesId.Trim(),
            request.PalletNo.Trim(), request.IsOem, request.ProductionDate.Trim(), request.PrintDate.Trim(),
            request.PrintTime.Trim(), productionDate, cancellationToken);
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

        var slipno = reader["slipno"].ToString()!.Trim();
        var partno = reader["partno"].ToString()!.Trim();
        var soluong = reader["weight"].ToString()!.Trim();
        var pallet = reader["pallet_no"].ToString()!.Trim();
        await reader.CloseAsync();

        var pathFile = labelExcelService.CreateLabelExcel(labelData, request.PrinterName.Trim(), isReprint: true);
        try
        {
            serverPrintService.PrintExcel(request.PrinterName.Trim(), pathFile);
        }
        finally
        {
            DeleteFileIfExists(pathFile);
        }

        // Log reprint to Print_again_log (same as old Print_BB_Again)
        var printDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        const string logSql = "Insert into [BB].[dbo].[Print_again_log] Values (@mesid, @barcode, @slipno, @partno, @soluong, @pallet, @printDate, @usrno, N'Hiện trường tự in, Device: Android ')";
        await using var logCommand = new SqlCommand(logSql, erpConnection);
        logCommand.Parameters.AddWithValue("@mesid", request.MesId.Trim());
        logCommand.Parameters.AddWithValue("@barcode", request.Barcode.Trim());
        logCommand.Parameters.AddWithValue("@slipno", slipno);
        logCommand.Parameters.AddWithValue("@partno", partno);
        logCommand.Parameters.AddWithValue("@soluong", soluong);
        logCommand.Parameters.AddWithValue("@pallet", pallet);
        logCommand.Parameters.AddWithValue("@printDate", printDate);
        logCommand.Parameters.AddWithValue("@usrno", request.UserName.Trim());
        await logCommand.ExecuteNonQueryAsync(cancellationToken);
    }

    private async Task<PrintLabelResponse> PrintLabelCoreAsync(
        string partNo, string machineNo, string printerName,
        string shiftId, string employeeNo, string reworkFlag, decimal weight,
        string mesId, string palletNo, bool isOem,
        string pday, string indat, string intime,
        DateTime productionDate, CancellationToken cancellationToken)
    {
        var machineFullNo = $"V-BB37{machineNo}";
        var slipNo = $"{shiftId}{machineNo}-{pday.Substring(4, 4)}";
        var glue = GlueHelper.Resolve(partNo);
        partNo = glue.NormalizedPartNo;

        // 1. Check rubnod_Ptype override (from old code lines 401-416)
        var (glueType, productType) = await CheckPtypeOverrideAsync(glue, partNo, cancellationToken);

        // 2. Validate MES exists and date matches + recipe matches
        var (planId, recipeName, planQty) = await ValidateMesAsync(mesId, partNo, pday, machineNo, cancellationToken);

        // 3. Get expiration days
        var expirationDays = await GetExpirationDaysAsync(glueType, partNo, cancellationToken);

        // 4. Generate barcode
        var barcode = await CreateNextBarcodeAsync(glueType, pday, productionDate, cancellationToken);

        // 5. Check glue weight limit (GioiHanKeo)
        var (gioiHanKeo, keoSx) = await CheckGlueLimitAsync(glueType, machineFullNo, planId, recipeName, machineNo, weight, cancellationToken);

        // 6. Validate and ensure pallet
        await ValidatePalletAsync(palletNo, mesId, employeeNo, cancellationToken);

        // 7. Determine active flag and candao
        var active = productType == "3" ? "N" : productType == "2" ? "" : "Y";
        var candao = glueType == "RB" ? "N" : reworkFlag.Trim();

        // 8. Insert OEM label if needed
        var now = DateTime.Now;
        if (isOem)
        {
            await InsertOemLabelAsync(planId, partNo, barcode, now, cancellationToken);
        }

        // 9. Calculate somesx (batch number) - same as old code lines 653-677
        var somesx = await CalculateBatchNumberAsync(machineFullNo, planId, machineNo, partNo, weight, keoSx, gioiHanKeo, cancellationToken);

        // 10. INSERT into prdebe
        var effdat = now.AddDays(expirationDays).ToString("yyyyMMdd");
        const string insertSql = "INSERT INTO [dbo].[prdebe] VALUES ('4', 'V', @mesid, @machno, @daylimt, @barcode, @slipno, @weight, @prodat, @effdat, @class, @ptype, @candao, @partno, @intime, @indat, @usrno, @pallet, @active, @somesx)";
        await using var connection = connectionFactory.Create("Erp33");
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand(insertSql, connection);
        command.Parameters.AddWithValue("@mesid", planId);
        command.Parameters.AddWithValue("@machno", machineFullNo);
        command.Parameters.AddWithValue("@daylimt", expirationDays.ToString());
        command.Parameters.AddWithValue("@barcode", barcode);
        command.Parameters.AddWithValue("@slipno", slipNo);
        command.Parameters.AddWithValue("@weight", weight.ToString());
        command.Parameters.AddWithValue("@prodat", pday);
        command.Parameters.AddWithValue("@effdat", effdat);
        command.Parameters.AddWithValue("@class", shiftId);
        command.Parameters.AddWithValue("@ptype", productType);
        command.Parameters.AddWithValue("@candao", candao);
        command.Parameters.AddWithValue("@partno", partNo);
        command.Parameters.AddWithValue("@intime", intime);
        command.Parameters.AddWithValue("@indat", indat);
        command.Parameters.AddWithValue("@usrno", employeeNo);
        command.Parameters.AddWithValue("@pallet", palletNo);
        command.Parameters.AddWithValue("@active", active);
        command.Parameters.AddWithValue("@somesx", somesx);

        var affected = await command.ExecuteNonQueryAsync(cancellationToken);
        if (affected <= 0)
        {
            throw new InvalidOperationException("Đợi 1 chút rồi quét lại!!!");
        }

        // 11. Generate Excel and print
        var labelData = new LabelPrintData(
            partNo, effdat, glueType, slipNo, barcode, shiftId,
            expirationDays.ToString(), weight.ToString(), mesId, machineNo,
            pday, indat, shiftId, intime, palletNo,
            isOem ? "OEM" : "", somesx);

        var pathFile = labelExcelService.CreateLabelExcel(labelData, printerName, isReprint: false);
        try
        {
            serverPrintService.PrintExcel(printerName, pathFile);
        }
        finally
        {
            DeleteFileIfExists(pathFile);
        }

        return new PrintLabelResponse(barcode, somesx);
    }

    /// <summary>
    /// Check rubnod_Ptype table for ptype override (old code lines 401-416).
    /// If count >= 2: error. If count == 1: override to RB with ptype from DB.
    /// </summary>
    private async Task<(string GlueType, string ProductType)> CheckPtypeOverrideAsync(GlueInfo glue, string partNo, CancellationToken cancellationToken)
    {
        const string sql = "SELECT [ptype], LTRIM(RTRIM([rubno_7])) rubno_7 FROM [InTem].[dbo].[rubnod_Ptype] WHERE SUBSTRING(rubno_7,7,1)= '2' AND rubno_7 = @partno";
        await using var connection = connectionFactory.Create("InTem");
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@partno", partNo.Trim());

        var rows = new List<string>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            rows.Add(reader["ptype"].ToString()!.Trim());
        }

        if (rows.Count >= 2)
        {
            throw new InvalidOperationException("Liên hệ phòng thí nghiệm (a Thuần) đóng 1 tiêu chuẩn");
        }

        if (rows.Count == 1)
        {
            return ("RB", rows[0]);
        }

        return (glue.GlueType, glue.ProductType);
    }

    /// <summary>
    /// Validate MES: check exists, date matches, recipe matches (old code lines 345-486).
    /// Returns (Plan_Id, Recipe_Code, Plan_Num).
    /// </summary>
    private async Task<(string PlanId, string RecipeName, string PlanQty)> ValidateMesAsync(
        string mesId, string partNo, string pday, string machineNo, CancellationToken cancellationToken)
    {
        // Check MES exists and date matches
        const string checkSql = "select Plan_Id, P_Date from [mfnsShareDB].[dbo].[IF_RtPlan2Mixing] where Plan_Id = @mesid";
        await using var connection = connectionFactory.CreateForMachine(machineNo);
        await connection.OpenAsync(cancellationToken);
        await using var checkCommand = new SqlCommand(checkSql, connection);
        checkCommand.Parameters.AddWithValue("@mesid", mesId);
        await using var checkReader = await checkCommand.ExecuteReaderAsync(cancellationToken);
        if (!await checkReader.ReadAsync(cancellationToken))
        {
            throw new InvalidOperationException("Mã MES đã bị đóng! Liên hệ IT mở!");
        }

        var mesDate = checkReader["P_Date"].ToString()!.Trim().Replace("-", "");
        await checkReader.CloseAsync();
        if (!string.Equals(mesDate, pday.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Mes quá giờ không quét được");
        }

        // Check recipe matches MES (old code lines 474-486)
        const string recipeSql = "Select Plan_Id, Plan_Num, Recipe_Code from [mfnsShareDB].[dbo].[IF_RtPlan2Mixing] where replace(Recipe_Name,'-','') = @partno and Plan_Id = @mesid";
        await using var recipeCommand = new SqlCommand(recipeSql, connection);
        recipeCommand.Parameters.AddWithValue("@partno", partNo.Replace("-", ""));
        recipeCommand.Parameters.AddWithValue("@mesid", mesId);
        await using var recipeReader = await recipeCommand.ExecuteReaderAsync(cancellationToken);
        if (!await recipeReader.ReadAsync(cancellationToken))
        {
            throw new InvalidOperationException("MES không tồn tại  IF_RtPlan2Mixing , tạo MES khác!");
        }

        var planId = recipeReader["Plan_Id"].ToString()!.Trim();
        var recipeName = recipeReader["Recipe_Code"].ToString()!.Trim();
        var planQty = recipeReader["Plan_Num"].ToString()!.Trim();
        return (planId, recipeName, planQty);
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
            throw new InvalidOperationException("Mã keo không được sử dụng.\n Liên hệ Duyên phòng chế tạo (755) !");
        }

        return days;
    }

    private async Task<string> CreateNextBarcodeAsync(string glueType, string pday, DateTime productionDate, CancellationToken cancellationToken)
    {
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

    /// <summary>
    /// Check glue weight limit (old code lines 496-528).
    /// Returns (GioiHanKeo, KeoSX).
    /// </summary>
    private async Task<(float GioiHanKeo, float KeoSx)> CheckGlueLimitAsync(
        string glueType, string machineFullNo, string planId, string recipeName,
        string machineNo, decimal weight, CancellationToken cancellationToken)
    {
        if (!glueType.StartsWith("R"))
        {
            return (0, 0);
        }

        // Get GioiHanKeo = FinishNum * sum(set_weight)
        const string limitSql = "select b.FinishNum * (select sum(set_weight) from [mfns].[dbo].[pmt_weigh] where father_code = b.RecipeName) from [mfns].[dbo].[Ppt_GroupLot] b where MesPlanID = @planId and RecipeName = @recipeName and End_datetime is not null";
        await using var machineConnection = connectionFactory.CreateForMachine(machineNo);
        await machineConnection.OpenAsync(cancellationToken);
        await using var limitCommand = new SqlCommand(limitSql, machineConnection);
        limitCommand.Parameters.AddWithValue("@planId", planId);
        limitCommand.Parameters.AddWithValue("@recipeName", recipeName);
        await using var limitReader = await limitCommand.ExecuteReaderAsync(cancellationToken);
        float gioiHanKeo = 0;
        if (await limitReader.ReadAsync(cancellationToken))
        {
            gioiHanKeo = float.Parse(limitReader[0].ToString()!.Trim());
        }
        await limitReader.CloseAsync();

        // Get current scanned weight
        const string weightSql = "select ISNULL(sum([weight]),0) from [erp].[dbo].[prdebe] where subno='4' and factory='V' and machno = @machno and mesid = @planId";
        await using var erpConnection = connectionFactory.Create("Erp33");
        await erpConnection.OpenAsync(cancellationToken);
        await using var weightCommand = new SqlCommand(weightSql, erpConnection);
        weightCommand.Parameters.AddWithValue("@machno", machineFullNo);
        weightCommand.Parameters.AddWithValue("@planId", planId);
        var weightValue = await weightCommand.ExecuteScalarAsync(cancellationToken);
        var keoSx = float.Parse(weightValue?.ToString() ?? "0");

        var keoVo = keoSx + (float)weight;
        if (keoVo > gioiHanKeo)
        {
            if (gioiHanKeo < keoSx)
            {
                throw new InvalidOperationException("MES này quá số lượng kế hoạch, không thể quét tiếp!");
            }
            else
            {
                throw new InvalidOperationException($"Lỗi! MES này chỉ quét được {(gioiHanKeo - keoSx).ToString().Trim()}KG nữa!");
            }
        }

        return (gioiHanKeo, keoSx);
    }

    /// <summary>
    /// Validate pallet: check prefix, export status, duplicate for same MES, ensure exists in PalletBB
    /// (old code lines 533-578).
    /// </summary>
    private async Task ValidatePalletAsync(string palletNo, string mesId, string employeeNo, CancellationToken cancellationToken)
    {
        if (!PalletHelper.IsValid(palletNo))
        {
            throw new InvalidOperationException("Pallet Không hợp lệ, nhập lại!!!");
        }

        // Check pallet export status (old code lines 562-571)
        const string exportSql = "select top 1 active from [dbo].[prdebe] where subno ='4' and factory='V' and pallet_no = @pallet order by indat desc, intime desc";
        await using var erpConnection = connectionFactory.Create("Erp33");
        await erpConnection.OpenAsync(cancellationToken);
        await using var exportCommand = new SqlCommand(exportSql, erpConnection);
        exportCommand.Parameters.AddWithValue("@pallet", palletNo.Trim());
        await using var exportReader = await exportCommand.ExecuteReaderAsync(cancellationToken);
        if (await exportReader.ReadAsync(cancellationToken))
        {
            if (exportReader[0].ToString() == "N")
            {
                throw new InvalidOperationException("Pallet này chưa xuất, không được trùng pallet");
            }
        }
        await exportReader.CloseAsync();

        // Check pallet not duplicate for same MES (old code lines 572-578)
        const string dupSql = "select count(*) from [dbo].[prdebe] where subno ='4' and factory='V' and mesid = @mesid and pallet_no = @pallet";
        await using var dupCommand = new SqlCommand(dupSql, erpConnection);
        dupCommand.Parameters.AddWithValue("@mesid", mesId.Trim());
        dupCommand.Parameters.AddWithValue("@pallet", palletNo.Trim());
        var dupCount = await dupCommand.ExecuteScalarAsync(cancellationToken);
        if (dupCount is not null && int.TryParse(dupCount.ToString(), out var count) && count > 0)
        {
            throw new InvalidOperationException("1 Pallet chỉ được quét 1 lần cho 1 mã MES");
        }

        // Ensure pallet exists in PalletBB (old code lines 544-558)
        const string selectSql = "SELECT PALLET_NO FROM [InTem].[dbo].[PalletBB] WHERE PALLET_NO = @pallet";
        await using var intemConnection = connectionFactory.Create("InTem");
        await intemConnection.OpenAsync(cancellationToken);
        await using var selectCommand = new SqlCommand(selectSql, intemConnection);
        selectCommand.Parameters.AddWithValue("@pallet", palletNo);
        var exists = await selectCommand.ExecuteScalarAsync(cancellationToken);
        if (exists is not null)
        {
            return;
        }

        const string insertSql = "INSERT INTO [InTem].[dbo].[PalletBB] VALUES(@pallet, @date, @employeeNo, '1', 'Y')";
        await using var insertCommand = new SqlCommand(insertSql, intemConnection);
        insertCommand.Parameters.AddWithValue("@pallet", palletNo);
        insertCommand.Parameters.AddWithValue("@date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        insertCommand.Parameters.AddWithValue("@employeeNo", employeeNo.Trim());
        var affected = await insertCommand.ExecuteNonQueryAsync(cancellationToken);
        if (affected <= 0)
        {
            throw new InvalidOperationException("Lỗi cập nhật Pallet\nPalletBB");
        }
    }

    /// <summary>
    /// Calculate batch number (somesx) - same as old code lines 602-677.
    /// Uses weight recipe (kg tiêu chuẩn), kgDaIn (already printed), kgIn (current weight),
    /// and GioiHanKeo (glue limit) to compute batch range.
    /// </summary>
    private async Task<string> CalculateBatchNumberAsync(
        string machineFullNo, string planId, string machineNo,
        string partNo, decimal kgIn, float keoSx, float gioiHanKeo,
        CancellationToken cancellationToken)
    {
        // Get weight recipe (kg tiêu chuẩn)
        const string weightRecipeSql = "SELECT SUM(set_weight) as weightRecipe FROM [mfns].[dbo].[pmt_weigh] where father_code = @partno";
        await using var machineConnection = connectionFactory.CreateForMachine(machineNo);
        await machineConnection.OpenAsync(cancellationToken);
        await using var weightCommand = new SqlCommand(weightRecipeSql, machineConnection);
        weightCommand.Parameters.AddWithValue("@partno", partNo);
        var weightValue = await weightCommand.ExecuteScalarAsync(cancellationToken);
        if (weightValue is null || string.IsNullOrWhiteSpace(weightValue.ToString()))
        {
            throw new InvalidOperationException("Không tìm thấy số kg tiêu chuẩn");
        }

        var kgTieuChuan = double.Parse(weightValue.ToString()!.Trim());
        var kgDaIn = (double)keoSx;
        var kgInDouble = (double)kgIn;
        const double epsilon = 0.000001;

        var start = (int)Math.Floor(kgDaIn / kgTieuChuan) + 1;
        var end = (int)Math.Floor((kgDaIn + kgInDouble - epsilon) / kgTieuChuan) + 1;

        var maxBatch = (int)Math.Ceiling(gioiHanKeo / kgTieuChuan);
        if (end > maxBatch)
        {
            end = maxBatch;
        }

        if (start == end)
        {
            return start.ToString();
        }

        return string.Join("-", Enumerable.Range(start, end - start + 1));
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
