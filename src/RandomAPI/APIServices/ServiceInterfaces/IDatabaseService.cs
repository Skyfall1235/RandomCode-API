using Microsoft.Data.Sqlite;

public interface IDatabaseService
{
    void Execute(Action<SqliteConnection> action);
    Task<int> ExecuteAsync(string sql, object? param = null);
    Task<T> ExecuteScalarAsync<T>(string sql, object? param = null);
    Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param = null);
}