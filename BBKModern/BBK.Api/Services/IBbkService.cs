using BBK.Api.Models;

namespace BBK.Api.Services;

public interface IBbkService
{
    Task<ApiResult<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken);

    Task<ApiResult<IReadOnlyList<MachineResponse>>> GetMachinesAsync(CancellationToken cancellationToken);

    Task<ApiResult<IReadOnlyList<MesResponse>>> GetMesListAsync(string machineNo, CancellationToken cancellationToken);

    Task<ApiResult<IReadOnlyList<MesResponse>>> GetMesListForReprintAsync(string machineNo, CancellationToken cancellationToken);

    Task<ApiResult<IReadOnlyList<PrinterResponse>>> GetPrintersAsync(CancellationToken cancellationToken);

    Task<ApiResult<IReadOnlyList<BarcodeResponse>>> GetBarcodesAsync(string mesId, string machineNo, CancellationToken cancellationToken);

    Task<ApiResult<PrintLabelResponse>> PrintLabelAsync(PrintLabelRequest request, CancellationToken cancellationToken);

    Task<ApiResult<PrintLabelResponse>> CompensatePrintLabelAsync(CompensatePrintRequest request, CancellationToken cancellationToken);

    Task<ApiResult<object>> ReprintLabelAsync(ReprintLabelRequest request, CancellationToken cancellationToken);

    Task<ApiResult<object>> FinishAsync(FinishRequest request, CancellationToken cancellationToken);
}
