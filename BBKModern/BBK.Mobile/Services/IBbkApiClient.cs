using BBK.Mobile.Models;

namespace BBK.Mobile.Services;

public interface IBbkApiClient
{
    Task<ApiResult<LoginResponse>> LoginAsync(LoginRequest request);

    Task<ApiResult<IReadOnlyList<MachineDto>>> GetMachinesAsync();

    Task<ApiResult<IReadOnlyList<MesDto>>> GetMesAsync(string machineNo);

    Task<ApiResult<IReadOnlyList<PrinterDto>>> GetPrintersAsync();

    Task<ApiResult<IReadOnlyList<BarcodeDto>>> GetBarcodesAsync(string mesId, string machineNo);

    Task<ApiResult<PrintLabelResponse>> PrintLabelAsync(PrintLabelRequest request);

    Task<ApiResult<object>> ReprintLabelAsync(ReprintLabelRequest request);
}
