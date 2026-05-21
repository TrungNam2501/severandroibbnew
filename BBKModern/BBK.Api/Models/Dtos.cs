namespace BBK.Api.Models;

public sealed record LoginRequest(string EmployeeNo);

public sealed record LoginResponse(string EmployeeNo, string Name, string DepartmentNo);

public sealed record MachineResponse(string Code, string Name);

public sealed record MesResponse(string PlanId, string RecipeCode);

public sealed record PrinterResponse(string Code, string Name);

public sealed record BarcodeResponse(string Barcode, decimal Weight);

public sealed record PrintLabelRequest(string RecipeCode, string MachineNo, string PrinterName, string EmployeeNo, string ReworkFlag, decimal Weight, string MesId, string PalletNo, string DeviceId, bool IsOem, string DepartmentNo);

public sealed record PrintLabelResponse(string Barcode, string BatchNo);

public sealed record ReprintLabelRequest(string UserName, string MachineNo, string Barcode, string MesId, decimal Weight, string PrinterName, string RecipeCode);

public sealed record CompensatePrintRequest(string RecipeCode, string MachineNo, string PrinterName, string ShiftId, string EmployeeNo, string ReworkFlag, decimal Weight, string MesId, string PalletNo, string ProductionDate, string PrintDate, string PrintTime, bool IsOem);
