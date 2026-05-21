using BBK.Api.Models;

namespace BBK.Api.Repositories;

public interface IBbkRepository
{
    Task<LoginResponse?> FindEmployeeAsync(string employeeNo, CancellationToken cancellationToken);

    Task<IReadOnlyList<MachineResponse>> GetMachinesAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<MesResponse>> GetMesListAsync(string machineNo, CancellationToken cancellationToken);

    Task<IReadOnlyList<MesResponse>> GetMesListForReprintAsync(string machineNo, CancellationToken cancellationToken);

    Task<IReadOnlyList<PrinterResponse>> GetPrintersAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<BarcodeResponse>> GetBarcodesAsync(string mesId, string machineNo, CancellationToken cancellationToken);

    Task<PrintLabelResponse> PrintLabelAsync(PrintLabelRequest request, CancellationToken cancellationToken);

    Task<PrintLabelResponse> CompensatePrintLabelAsync(CompensatePrintRequest request, CancellationToken cancellationToken);

    Task ReprintLabelAsync(ReprintLabelRequest request, CancellationToken cancellationToken);
}
