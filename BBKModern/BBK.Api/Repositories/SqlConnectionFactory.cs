using Microsoft.Data.SqlClient;

namespace BBK.Api.Repositories;

public sealed class SqlConnectionFactory(IConfiguration configuration) : ISqlConnectionFactory
{
    public SqlConnection Create(string name)
    {
        var connectionString = configuration.GetConnectionString(name);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException($"Missing connection string: {name}");
        }

        return new SqlConnection(connectionString);
    }

    public SqlConnection CreateForMachine(string machineNo)
    {
        var normalized = machineNo.Trim();
        var key = normalized switch
        {
            "V-BB3701" or "01" => "Machine01",
            "V-BB3702" or "02" => "Machine02",
            "V-BB3703" or "03" => "Machine03",
            "V-BB3704" or "04" => "Machine04",
            "V-BB3705" or "05" => "Machine05",
            "V-BB3706" or "06" => "Machine06",
            "V-BB3707" or "07" => "Machine07",
            "V-BB3708" or "08" => "Machine08",
            _ => throw new InvalidOperationException($"Unknown machine: {machineNo}")
        };

        return Create(key);
    }
}
