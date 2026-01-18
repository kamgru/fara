using System.Data.Common;
using Microsoft.Data.Sqlite;

namespace Fara.Web.DataAccess;

public interface ISqlQueryRunner
{
    Task<IReadOnlyList<T>> QueryAsync<T>(
        string sql,
        Func<DbDataReader, T> map,
        params SqlArgument[] args);

    Task<T?> GetAsync<T>(
        string sql,
        Func<DbDataReader, T> map,
        params SqlArgument[] args);
}

public class SqlQueryRunner(
    string connectionString) : ISqlQueryRunner
{
    public async Task<IReadOnlyList<T>> QueryAsync<T>(
        string sql,
        Func<DbDataReader, T> map,
        params SqlArgument[] args)
    {
        await using SqliteConnection connection = new(connectionString);
        await connection.OpenAsync();
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = sql;
        foreach (SqlArgument arg in args)
        {
            SqliteParameter parameter = command.CreateParameter();
            parameter.ParameterName = arg.Name;
            parameter.DbType = arg.DbType;
            parameter.Value = arg.Value;
            command.Parameters.Add(parameter);
        }

        List<T> result = [];
        await using SqliteDataReader reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(map(reader));
        }

        return result;
    }

    public async Task<T?> GetAsync<T>(
        string sql,
        Func<DbDataReader, T> map,
        params SqlArgument[] args)
    {
        await using SqliteConnection connection = new(connectionString);
        await connection.OpenAsync();
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = sql;
        foreach (SqlArgument arg in args)
        {
            SqliteParameter parameter = command.CreateParameter();
            parameter.ParameterName = arg.Name;
            parameter.DbType = arg.DbType;
            parameter.Value = arg.Value;
            command.Parameters.Add(parameter);
        }

        await using SqliteDataReader reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return map(reader);
        }
        return default;
    }
}
