using Dapper;
using Backend.Interfaces;
using Microsoft.Data.SqlClient;

namespace Backend.Implementations;

public class SqlHelper : ISqlHelper
{
    private readonly ISqlConnectionStringBuilder _connectionStringBuilder;

    public SqlHelper(ISqlConnectionStringBuilder connectionStringBuilder)
    {
        _connectionStringBuilder = connectionStringBuilder;
    }

    public async Task<IEnumerable<T>> QueryAsync<T>(string sql)
    {
        return await QueryAsync<T>("master", sql);
    }

    public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object sqlParams)
    {
        return await QueryAsync<T>("master", sql, sqlParams);
    }

    public async Task<IEnumerable<T>> QueryAsync<T>(string sql, DynamicParameters sqlParams)
    {
        return await QueryAsync<T>("master", sql, sqlParams);
    }

    public async Task<IEnumerable<T>> QueryAsync<T>(string database, string sql)
    {
        using (SqlConnection connection = new SqlConnection(_connectionStringBuilder.GetConnectionString(database)))
        {
            return await connection.QueryAsync<T>(sql);
        }
    }

    public async Task<IEnumerable<T>> QueryAsync<T>(string database, string sql, object sqlParams)
    {
        using (SqlConnection connection = new SqlConnection(_connectionStringBuilder.GetConnectionString(database)))
        {
            return await connection.QueryAsync<T>(sql, sqlParams);
        }
    }

    public async Task<IEnumerable<T>> QueryAsync<T>(string database, string sql, DynamicParameters sqlParams)
    {
        using (SqlConnection connection = new SqlConnection(_connectionStringBuilder.GetConnectionString(database)))
        {
            return await connection.QueryAsync<T>(sql, sqlParams);
        }
    }

    public async Task<int> ExecuteAsync(string sql)
    {
        return await ExecuteAsync("master", sql);
    }

    public async Task<int> ExecuteAsync(string sql, object sqlParams)
    {
        return await ExecuteAsync("master", sql, sqlParams);
    }

    public async Task<int> ExecuteAsync(string sql, DynamicParameters sqlParams)
    {
        return await ExecuteAsync("master", sql, sqlParams);
    }

    public async Task<int> ExecuteAsync(string database, string sql)
    {
        using (SqlConnection connection = new SqlConnection(_connectionStringBuilder.GetConnectionString(database)))
        {
            return await connection.ExecuteAsync(sql);
        }
    }

    public async Task<int> ExecuteAsync(string database, string sql, object sqlParams)
    {
        using (SqlConnection connection = new SqlConnection(_connectionStringBuilder.GetConnectionString(database)))
        {
            return await connection.ExecuteAsync(sql, sqlParams);
        }
    }

    public async Task<int> ExecuteAsync(string database, string sql, DynamicParameters sqlParams)
    {
        using (SqlConnection connection = new SqlConnection(_connectionStringBuilder.GetConnectionString(database)))
        {
            return await connection.ExecuteAsync(sql, sqlParams);
        }
    }

    public async Task<bool> Exists(string database, string sql)
    {
        return (await QueryAsync<int>(database, sql)).SingleOrDefault() != 0;
    }

    public async Task<bool> Exists(string database, string sql, object sqlParams)
    {
        return (await QueryAsync<int>(database, sql, sqlParams)).SingleOrDefault() != 0;
    }

    public async Task<bool> Exists(string database, string sql, DynamicParameters sqlParams)
    {
        return (await QueryAsync<int>(database, sql, sqlParams)).SingleOrDefault() != 0;
    }
}
