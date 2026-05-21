using System.Net.Http.Json;
using BBK.Mobile.Models;

namespace BBK.Mobile.Services;

public sealed class BbkApiClient(HttpClient httpClient) : IBbkApiClient
{
    public async Task<ApiResult<LoginResponse>> LoginAsync(LoginRequest request)
    {
        var response = await httpClient.PostAsJsonAsync("/api/auth/login", request);
        return await ReadResultAsync<LoginResponse>(response);
    }

    public async Task<ApiResult<IReadOnlyList<MachineDto>>> GetMachinesAsync()
    {
        return await httpClient.GetFromJsonAsync<ApiResult<IReadOnlyList<MachineDto>>>("/api/production/machines")
            ?? new ApiResult<IReadOnlyList<MachineDto>>(false, "Không đọc được danh sách máy", null);
    }

    public async Task<ApiResult<IReadOnlyList<MesDto>>> GetMesAsync(string machineNo)
    {
        return await httpClient.GetFromJsonAsync<ApiResult<IReadOnlyList<MesDto>>>($"/api/production/mes?machineNo={Uri.EscapeDataString(machineNo)}")
            ?? new ApiResult<IReadOnlyList<MesDto>>(false, "Không đọc được danh sách MES", null);
    }

    public async Task<ApiResult<IReadOnlyList<PrinterDto>>> GetPrintersAsync()
    {
        return await httpClient.GetFromJsonAsync<ApiResult<IReadOnlyList<PrinterDto>>>("/api/production/printers")
            ?? new ApiResult<IReadOnlyList<PrinterDto>>(false, "Không đọc được danh sách máy in", null);
    }

    public async Task<ApiResult<IReadOnlyList<BarcodeDto>>> GetBarcodesAsync(string mesId, string machineNo)
    {
        var url = $"/api/production/barcodes?mesId={Uri.EscapeDataString(mesId)}&machineNo={Uri.EscapeDataString(machineNo)}";
        return await httpClient.GetFromJsonAsync<ApiResult<IReadOnlyList<BarcodeDto>>>(url)
            ?? new ApiResult<IReadOnlyList<BarcodeDto>>(false, "Không đọc được danh sách barcode", null);
    }

    public async Task<ApiResult<PrintLabelResponse>> PrintLabelAsync(PrintLabelRequest request)
    {
        var response = await httpClient.PostAsJsonAsync("/api/production/labels", request);
        return await ReadResultAsync<PrintLabelResponse>(response);
    }

    public async Task<ApiResult<object>> ReprintLabelAsync(ReprintLabelRequest request)
    {
        var response = await httpClient.PostAsJsonAsync("/api/production/labels/reprint", request);
        return await ReadResultAsync<object>(response);
    }

    private static async Task<ApiResult<T>> ReadResultAsync<T>(HttpResponseMessage response)
    {
        var result = await response.Content.ReadFromJsonAsync<ApiResult<T>>();
        return result ?? new ApiResult<T>(false, $"HTTP {(int)response.StatusCode}", default);
    }
}
