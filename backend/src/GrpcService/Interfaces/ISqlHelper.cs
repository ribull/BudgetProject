﻿using Dapper;

namespace GrpcService.Interfaces;

public interface ISqlHelper
{
    Task<IEnumerable<T>> QueryAsync<T>(string sql);

    Task<IEnumerable<T>> QueryAsync<T>(string sql, object sqlParams);

    Task<IEnumerable<T>> QueryAsync<T>(string sql, DynamicParameters sqlParams);

    Task<IEnumerable<T>> QueryAsync<T>(string database, string sql);

    Task<IEnumerable<T>> QueryAsync<T>(string database, string sql, object sqlParams);

    Task<IEnumerable<T>> QueryAsync<T>(string database, string sql, DynamicParameters sqlParams);

    Task<int> ExecuteAsync(string sql);

    Task<int> ExecuteAsync(string sql, object sqlParams);

    Task<int> ExecuteAsync(string sql, DynamicParameters sqlParams);

    Task<int> ExecuteAsync(string database, string sql);

    Task<int> ExecuteAsync(string database, string sql, object sqlParams);

    Task<int> ExecuteAsync(string database, string sql, DynamicParameters sqlParams);

    Task<bool> Exists(string database, string sql);

    Task<bool> Exists(string database, string sql, object sqlParams);

    Task<bool> Exists(string database, string sql, DynamicParameters sqlParams);
}