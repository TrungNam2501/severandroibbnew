namespace BBK.Api.Services;

public sealed record GlueInfo(string GlueType, string ProductType, string NormalizedPartNo);

public static class GlueHelper
{
    public static GlueInfo Resolve(string partNo)
    {
        var normalized = partNo.Trim();
        if (normalized.Length <= 5)
        {
            return new GlueInfo("RD", "3", normalized);
        }

        var suffix = normalized.Length > 6 ? normalized[6..].ToUpperInvariant() : "";
        return suffix switch
        {
            "1" or "1-EDGE" or "1EDGE" or "1THU" => new GlueInfo("RB", "2", normalized),
            "2" or "3" or "4" or "5" or "2-EDGE" or "3-EDGE" or "4-EDGE" or "5-EDGE" or "2EDGE" or "3EDGE" or "4EDGE" or "5EDGE" or "2THU" or "3THU" or "4THU" or "5THU" => new GlueInfo("RC", "2", normalized),
            "RE" or "R" => new GlueInfo("RR", "3", normalized),
            _ => new GlueInfo("RD", "3", normalized)
        };
    }
}
