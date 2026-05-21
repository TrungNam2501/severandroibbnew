namespace BBK.Api.Services;

public static class ShiftHelper
{
    public static string GetShift(DateTime now)
    {
        var start = now.Date.Add(new TimeSpan(7, 30, 0));
        var end = now.Date.Add(new TimeSpan(19, 0, 0));
        return now >= start && now <= end ? "1" : "2";
    }

    public static string GetShiftMayBb(DateTime now)
    {
        var start = now.Date.Add(new TimeSpan(7, 30, 0));
        var end = now.Date.Add(new TimeSpan(19, 0, 0));
        return now >= start && now <= end ? "0" : "1";
    }

    public static DateTime GetProductionDate(string shift, DateTime now)
    {
        if (shift == "2" && now >= now.Date && now <= now.Date.Add(new TimeSpan(7, 30, 0)))
        {
            return now.AddDays(-1).Date;
        }

        return now.Date;
    }

    public static DateTime GetProductionDateForBb(string shift, DateTime now)
    {
        if (shift == "1" && now >= now.Date && now <= now.Date.Add(new TimeSpan(7, 30, 0)))
        {
            return now.AddDays(-1).Date;
        }

        return now.Date;
    }
}
