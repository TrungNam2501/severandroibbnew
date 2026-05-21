using BBK.Api.Models;
using BBK.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace BBK.Api.Controllers;

[ApiController]
[Route("api/production")]
public sealed class ProductionController(IBbkService service) : ControllerBase
{
    [HttpGet("machines")]
    public async Task<ApiResult<IReadOnlyList<MachineResponse>>> GetMachines(CancellationToken cancellationToken)
    {
        return await service.GetMachinesAsync(cancellationToken);
    }

    [HttpGet("mes")]
    public async Task<ActionResult<ApiResult<IReadOnlyList<MesResponse>>>> GetMes([FromQuery] string machineNo, CancellationToken cancellationToken)
    {
        return await service.GetMesListAsync(machineNo, cancellationToken);
    }

    [HttpGet("mes/reprint")]
    public async Task<ActionResult<ApiResult<IReadOnlyList<MesResponse>>>> GetMesForReprint([FromQuery] string machineNo, CancellationToken cancellationToken)
    {
        return await service.GetMesListForReprintAsync(machineNo, cancellationToken);
    }

    [HttpGet("printers")]
    public async Task<ApiResult<IReadOnlyList<PrinterResponse>>> GetPrinters(CancellationToken cancellationToken)
    {
        return await service.GetPrintersAsync(cancellationToken);
    }

    [HttpGet("barcodes")]
    public async Task<ApiResult<IReadOnlyList<BarcodeResponse>>> GetBarcodes([FromQuery] string mesId, [FromQuery] string machineNo, CancellationToken cancellationToken)
    {
        return await service.GetBarcodesAsync(mesId, machineNo, cancellationToken);
    }

    [HttpPost("labels")]
    public async Task<ActionResult<ApiResult<PrintLabelResponse>>> PrintLabel(PrintLabelRequest request, CancellationToken cancellationToken)
    {
        return await service.PrintLabelAsync(request, cancellationToken);
    }

    [HttpPost("labels/compensate")]
    public async Task<ActionResult<ApiResult<PrintLabelResponse>>> CompensatePrintLabel(CompensatePrintRequest request, CancellationToken cancellationToken)
    {
        return await service.CompensatePrintLabelAsync(request, cancellationToken);
    }

    [HttpPost("labels/reprint")]
    public async Task<ActionResult<ApiResult<object>>> ReprintLabel(ReprintLabelRequest request, CancellationToken cancellationToken)
    {
        return await service.ReprintLabelAsync(request, cancellationToken);
    }

    [HttpPost("finish")]
    public async Task<ActionResult<ApiResult<object>>> Finish(FinishRequest request, CancellationToken cancellationToken)
    {
        return await service.FinishAsync(request, cancellationToken);
    }
}
