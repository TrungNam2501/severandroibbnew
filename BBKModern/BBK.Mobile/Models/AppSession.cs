namespace BBK.Mobile.Models;

public sealed record AppSession(string MachineNo, string EmployeeNo, string Name, string DepartmentNo);

public static class Session
{
    public static AppSession? Current { get; set; }
}
