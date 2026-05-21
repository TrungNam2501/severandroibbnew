using BBK.Api.Models;
using BBK.Api.Repositories;

namespace BBK.Api.Services;

public sealed class BbkService(IBbkRepository repository, ILogger<BbkService> logger) : IBbkService
{
    public async Task<ApiResult<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.EmployeeNo) || request.EmployeeNo.Trim().Length != 6)
        {
            return ApiResult<LoginResponse>.Fail("Sai mã số thẻ");
        }

        var employee = await repository.FindEmployeeAsync(request.EmployeeNo.Trim(), cancellationToken);
        return employee is null ? ApiResult<LoginResponse>.Fail("Số thẻ không tồn tại") : ApiResult<LoginResponse>.Ok(employee);
    }

    public async Task<ApiResult<IReadOnlyList<MachineResponse>>> GetMachinesAsync(CancellationToken cancellationToken)
    {
        return ApiResult<IReadOnlyList<MachineResponse>>.Ok(await repository.GetMachinesAsync(cancellationToken));
    }

    public async Task<ApiResult<IReadOnlyList<MesResponse>>> GetMesListAsync(string machineNo, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(machineNo))
        {
            return ApiResult<IReadOnlyList<MesResponse>>.Fail("Chưa chọn máy");
        }

        return ApiResult<IReadOnlyList<MesResponse>>.Ok(await repository.GetMesListAsync(machineNo.Trim(), cancellationToken));
    }

    public async Task<ApiResult<IReadOnlyList<MesResponse>>> GetMesListForReprintAsync(string machineNo, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(machineNo))
        {
            return ApiResult<IReadOnlyList<MesResponse>>.Fail("Chưa chọn máy");
        }

        return ApiResult<IReadOnlyList<MesResponse>>.Ok(await repository.GetMesListForReprintAsync(machineNo.Trim(), cancellationToken));
    }

    public async Task<ApiResult<IReadOnlyList<PrinterResponse>>> GetPrintersAsync(CancellationToken cancellationToken)
    {
        return ApiResult<IReadOnlyList<PrinterResponse>>.Ok(await repository.GetPrintersAsync(cancellationToken));
    }

    public async Task<ApiResult<IReadOnlyList<BarcodeResponse>>> GetBarcodesAsync(string mesId, string machineNo, CancellationToken cancellationToken)
    {
        return ApiResult<IReadOnlyList<BarcodeResponse>>.Ok(await repository.GetBarcodesAsync(mesId.Trim(), machineNo.Trim(), cancellationToken));
    }

    public async Task<ApiResult<PrintLabelResponse>> PrintLabelAsync(PrintLabelRequest request, CancellationToken cancellationToken)
    {
        if (request.Weight < 30)
        {
            return ApiResult<PrintLabelResponse>.Fail("Lỗi! Trọng lượng không phù hợp!");
        }

        if (string.IsNullOrWhiteSpace(request.PalletNo) || request.PalletNo.Trim().Length < 6)
        {
            return ApiResult<PrintLabelResponse>.Fail("Pallet không đủ ký tự, nhập lại!!!");
        }

        try
        {
            var result = await repository.PrintLabelAsync(request, cancellationToken);
            return ApiResult<PrintLabelResponse>.Ok(result, "In tem thành công");
        }
        catch (InvalidOperationException ex)
        {
            return ApiResult<PrintLabelResponse>.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Print label failed");
            return ApiResult<PrintLabelResponse>.Fail("Không kết nối được Server!");
        }
    }

    public async Task<ApiResult<PrintLabelResponse>> CompensatePrintLabelAsync(CompensatePrintRequest request, CancellationToken cancellationToken)
    {
        if (request.Weight < 30)
        {
            return ApiResult<PrintLabelResponse>.Fail("Lỗi! Trọng lượng không phù hợp!");
        }

        if (string.IsNullOrWhiteSpace(request.PalletNo) || request.PalletNo.Trim().Length < 6)
        {
            return ApiResult<PrintLabelResponse>.Fail("Pallet không đủ ký tự, nhập lại!!!");
        }

        try
        {
            var result = await repository.CompensatePrintLabelAsync(request, cancellationToken);
            return ApiResult<PrintLabelResponse>.Ok(result, "In tem bù thành công");
        }
        catch (InvalidOperationException ex)
        {
            return ApiResult<PrintLabelResponse>.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Compensate print label failed");
            return ApiResult<PrintLabelResponse>.Fail("Không kết nối được Server!");
        }
    }

    public async Task<ApiResult<object>> ReprintLabelAsync(ReprintLabelRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await repository.ReprintLabelAsync(request, cancellationToken);
            return ApiResult<object>.Ok(new { }, "In lại tem thành công");
        }
        catch (InvalidOperationException ex)
        {
            return ApiResult<object>.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Reprint label failed");
            return ApiResult<object>.Fail("Không kết nối được Server!");
        }
    }

    public Task<ApiResult<object>> FinishAsync(FinishRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Finish batch: Recipe={Recipe}, MesId={MesId}, Machine={Machine}",
            request.RecipeCode, request.MesId, request.MachineNo);
        return Task.FromResult(ApiResult<object>.Ok(new { }, "Kết thúc mẻ thành công"));
    }
}
