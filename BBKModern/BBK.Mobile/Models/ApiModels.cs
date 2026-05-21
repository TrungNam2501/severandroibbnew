namespace BBK.Mobile.Models;

public sealed record ApiResult<T>(bool Success, string Message, T? Data);

public sealed record LoginRequest(string EmployeeNo);

public sealed record LoginResponse(string EmployeeNo, string Name, string DepartmentNo);

public sealed record MachineDto(string Code, string Name);

public sealed record MesDto(string PlanId, string RecipeCode);

public sealed record PrinterDto(string Code, string Name);

public sealed record BarcodeDto(string Barcode, decimal Weight);

public sealed record PrintLabelRequest(
    string RecipeCode,
    string MachineNo,
    string PrinterName,
    string EmployeeNo,
    string ReworkFlag,
    decimal Weight,
    string MesId,
    string PalletNo,
    string DeviceId,
    bool IsOem,
    string DepartmentNo);

public sealed record PrintLabelResponse(string Barcode, string BatchNo);

public sealed record ReprintLabelRequest(
    string UserName,
    string MachineNo,
    string Barcode,
    string MesId,
    decimal Weight,
    string PrinterName,
    string RecipeCode);
