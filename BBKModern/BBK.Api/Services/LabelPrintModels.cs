namespace BBK.Api.Services;

public sealed record LabelPrintData(
    string PartNo,
    string EffectiveDate,
    string GlueType,
    string SlipNo,
    string Barcode,
    string Shift,
    string ExpirationDays,
    string Weight,
    string MesId,
    string MachineNo,
    string ProductionDate,
    string PrintDate,
    string Class,
    string PrintTime,
    string Pallet,
    string Description,
    string BatchNo);
