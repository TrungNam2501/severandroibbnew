namespace BBK.Api.Services;

public static class PalletHelper
{
    public static bool IsValid(string palletNo)
    {
        var value = palletNo.Trim();
        if (value.Length < 6)
        {
            return false;
        }

        return value.StartsWith("VB", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("EB", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("VC", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("VD", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("VE", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("VF", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("V", StringComparison.OrdinalIgnoreCase);
    }
}
