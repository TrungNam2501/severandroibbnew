using BBK.Api.Models;

namespace BBK.Api.Repositories;

public sealed class MockBbkRepository : IBbkRepository
{
    public Task<LoginResponse?> FindEmployeeAsync(string employeeNo, CancellationToken cancellationToken)
    {
        LoginResponse? result = employeeNo == "000000" ? null : new LoginResponse(employeeNo, "Demo User", "KV2");
        return Task.FromResult(result);
    }

    public Task<IReadOnlyList<MachineResponse>> GetMachinesAsync(CancellationToken cancellationToken)
    {
        IReadOnlyList<MachineResponse> machines = Enumerable.Range(1, 8).Select(index => new MachineResponse(index.ToString("00"), $"BB37-{index:00}")).ToArray();
        return Task.FromResult(machines);
    }

    public Task<IReadOnlyList<MesResponse>> GetMesListAsync(string machineNo, CancellationToken cancellationToken)
    {
        IReadOnlyList<MesResponse> mes = new[] { new MesResponse("MES-DEMO-001", "R1001-9"), new MesResponse("MES-DEMO-002", "R1002-RM") };
        return Task.FromResult(mes);
    }

    public Task<IReadOnlyList<MesResponse>> GetMesListForReprintAsync(string machineNo, CancellationToken cancellationToken)
    {
        IReadOnlyList<MesResponse> mes = new[] { new MesResponse("MES-DEMO-001", "R1001-9"), new MesResponse("MES-DEMO-002", "R1002-RM"), new MesResponse("VMES-DEMO-003", "R1003-1") };
        return Task.FromResult(mes);
    }

    public Task<IReadOnlyList<PrinterResponse>> GetPrintersAsync(CancellationToken cancellationToken)
    {
        IReadOnlyList<PrinterResponse> printers = new[] { new PrinterResponse("BB-PRN-01", "BB-PRN-01"), new PrinterResponse("BB-PRN-02", "BB-PRN-02") };
        return Task.FromResult(printers);
    }

    public Task<IReadOnlyList<BarcodeResponse>> GetBarcodesAsync(string mesId, string machineNo, CancellationToken cancellationToken)
    {
        IReadOnlyList<BarcodeResponse> barcodes = new[] { new BarcodeResponse("RD26121001", 35), new BarcodeResponse("RD26121002", 40) };
        return Task.FromResult(barcodes);
    }

    public Task<PrintLabelResponse> PrintLabelAsync(PrintLabelRequest request, CancellationToken cancellationToken)
    {
        var barcode = $"RD{DateTime.Now:yyMMdd}{Random.Shared.Next(1, 999):000}";
        return Task.FromResult(new PrintLabelResponse(barcode, "1"));
    }

    public Task<PrintLabelResponse> CompensatePrintLabelAsync(CompensatePrintRequest request, CancellationToken cancellationToken)
    {
        var barcode = $"RD{DateTime.Now:yyMMdd}{Random.Shared.Next(1, 999):000}";
        return Task.FromResult(new PrintLabelResponse(barcode, "1"));
    }

    public Task ReprintLabelAsync(ReprintLabelRequest request, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
