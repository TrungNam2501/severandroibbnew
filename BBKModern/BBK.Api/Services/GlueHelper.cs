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
        var (glueType, productType, fixedPartNo) = suffix switch
        {
            "1" or "1-EDGE" or "1THU" => ("RB", "2", normalized),
            "1EDGE" => ("RB", "2", normalized[..5] + "-1-EDGE"),
            "2" or "3" or "4" or "5"
                or "2-EDGE" or "3-EDGE" or "4-EDGE" or "5-EDGE"
                or "2THU" or "3THU" or "4THU" or "5THU" => ("RC", "2", normalized),
            "2EDGE" => ("RC", "2", normalized[..5] + "-2-EDGE"),
            "3EDGE" => ("RC", "2", normalized[..5] + "-3-EDGE"),
            "4EDGE" => ("RC", "2", normalized[..5] + "-4-EDGE"),
            "5EDGE" => ("RC", "2", normalized[..5] + "-5-EDGE"),
            "RE" or "R" => ("RR", "3", normalized),
            "9" or "9-EDGE" or "RM" or "92" or "" or "EDGE" => ("RD", "3", normalized),
            "9EDGE" => ("RD", "3", normalized[..5] + "-9-EDGE"),
            "9THU" => ("RD", "3", normalized[..5] + "-9THU"),
            _ => ("RD", "3", normalized)
        };

        return new GlueInfo(glueType, productType, fixedPartNo);
    }
}
