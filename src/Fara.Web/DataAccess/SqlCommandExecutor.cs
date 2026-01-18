using Microsoft.Data.Sqlite;

namespace Fara.Web.DataAccess;

public interface ISqlCommandExecutor
{
    Task ExecuteAsync(string sql, params SqlArgument[] args);
}

public class SqlCommandExecutor(
    string connectionString) : ISqlCommandExecutor
{
    public async Task ExecuteAsync(string sql, params SqlArgument[] args)
    {
        await using SqliteConnection connection = new(connectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        foreach (SqlArgument arg in args)
        {
            SqliteParameter parameter = command.CreateParameter();
            parameter.ParameterName = arg.Name;
            parameter.DbType = arg.DbType;
            parameter.Value = arg.Value;
            command.Parameters.Add(parameter);
        }
        await command.ExecuteNonQueryAsync();
    }
}
