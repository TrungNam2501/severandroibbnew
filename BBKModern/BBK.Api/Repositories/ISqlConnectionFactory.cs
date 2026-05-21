using Microsoft.Data.SqlClient;

namespace BBK.Api.Repositories;

public interface ISqlConnectionFactory
{
    SqlConnection Create(string name);

    SqlConnection CreateForMachine(string machineNo);
}
