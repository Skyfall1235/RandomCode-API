using Dapper;
using Microsoft.Data.Sqlite;

public class DatabaseService
{
    private readonly string _connectionString;

    public DatabaseService(string dbPath = "Data/myapp.db")
    {
        //WILL NEED TO BE UPDATED
        _connectionString = $"Data Source={dbPath}";
    }

    /// <summary>
    /// Opens a connection to SQLite, executes an action, then closes the connection.
    /// </summary>
    public void Execute(Action<SqliteConnection> action)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        action(connection);
    }

    /// <summary>
    /// Opens a connection, executes a Dapper query, and returns a collection of results.
    /// </summary>
    /// <typeparam name="T">The type of object to map the result rows to (e.g., Event).</typeparam>
    /// <param name="sql">The SQL SELECT statement to execute.</param>
    /// <param name="param">Optional parameters object for Dapper.</param>
    /// <returns>A collection of mapped objects.</returns>
    public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param = null)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        // Dapper extension methods (QueryAsync) are used here
        return await connection.QueryAsync<T>(sql, param);
    }

    /// <summary>
    /// Opens a connection, executes a Dapper command (INSERT/UPDATE/DELETE), and returns the number of rows affected.
    /// </summary>
    /// <param name="sql">The SQL command to execute.</param>
    /// <param name="param">Optional parameters object for Dapper.</param>
    /// <returns>The number of rows affected.</returns>
    public async Task<int> ExecuteAsync(string sql, object? param = null)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        // Dapper extension methods (ExecuteAsync) are used here
        return await connection.ExecuteAsync(sql, param);
    }

    /// <summary>
    /// Opens a connection, executes a command, and returns a single value (e.g., the last inserted ID).
    /// </summary>
    /// <typeparam name="T">The type of the scalar result (e.g., int).</typeparam>
    /// <param name="sql">The SQL command to execute.</param>
    /// <param name="param">Optional parameters object for Dapper.</param>
    /// <returns>The single scalar result.</returns>
    public async Task<T> ExecuteScalarAsync<T>(string sql, object? param = null)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

#pragma warning disable CS8603 // Possible null reference return.
        return await connection.ExecuteScalarAsync<T>(sql, param);
#pragma warning restore CS8603 // Possible null reference return.
    }
}
