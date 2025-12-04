using Dapper;
using Microsoft.Data.Sqlite;
using RandomAPI.Models;
using System.Data;

namespace RandomAPI.Repository
{

    /// <summary>
    /// Implements CRUD operations for Webhook URLs using Dapper and the provided DatabaseService.
    /// </summary>
    public class WebhookRepository : IWebhookRepository, IInitializer
    {
        private readonly Func<IDbConnection> _connectionFactory;

        public WebhookRepository(Func<IDbConnection> connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        private IDbConnection CreateConnection()
        {
            var conn = _connectionFactory();
            conn.Open();
            return conn;
        }

        /// <summary>
        /// Ensures the WebhookUrls table exists in the SQLite database.
        /// </summary>
        public async Task InitializeAsync()
        {
            using var db = CreateConnection();

            const string sql = @"
            CREATE TABLE IF NOT EXISTS WebhookUrls (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Url TEXT NOT NULL UNIQUE,
                Type INTEGER NOT NULL DEFAULT 0
            );";

            await db.ExecuteAsync(sql);
            try
            {
                const string alterTableSql = "ALTER TABLE WebhookUrls ADD COLUMN Type INTEGER NOT NULL DEFAULT 0;";
                await db.ExecuteAsync(alterTableSql);
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 1) { }
            catch (Exception)
            {
                //re-throw any critical exceptions
                throw;
            }
        }

        /// <summary>
        /// Retrieves all registered webhook URLs from the database.
        /// </summary>
        /// <returns>A collection of WebhookUrl objects.</returns>
        public async Task<IEnumerable<WebhookUrl>> GetAllUrlsAsync()
        {
            using var db = CreateConnection();
            const string sql = "SELECT Id, Url FROM WebhookUrls ORDER BY Id;";
            return await db.QueryAsync<WebhookUrl>(sql);
        }

        public async Task<IEnumerable<WebhookUrl>> GetUrlsOfTypeAsync(IWebhookService.WebhookType type)
        {
            using var db = CreateConnection();
            const string sql = "SELECT Id, Url, Type FROM WebhookUrls WHERE Type = @Type;";
            var parameters = new { Type = (int)type };

            return await db.QueryAsync<WebhookUrl>(sql, parameters);
        }

        /// <summary>
        /// Adds a new URL to the database. Uses INSERT OR IGNORE to handle duplicates gracefully.
        /// </summary>
        /// <param name="url">The URL string to add.</param>
        public async Task AddUrlAsync(string url, IWebhookService.WebhookType type)
        {
            using var db = CreateConnection();
            const string sql = "INSERT OR IGNORE INTO WebhookUrls (Url, Type) VALUES (@Url, @Type);";
            var parameters = new
            {
                Url = url,
                Type = (int)type
            };

            await db.ExecuteAsync(sql, parameters);
        }

        /// <summary>
        /// Deletes a URL from the database.
        /// </summary>
        /// <param name="url">The URL string to delete.</param>
        /// <returns>The number of rows deleted (0 or 1).</returns>
        public async Task<int> DeleteUrlAsync(string url)
        {
            using var db = CreateConnection();
            const string sql = "DELETE FROM WebhookUrls WHERE Url = @Url;";
            return await db.ExecuteAsync(sql, new { Url = url });
        }
    }
}
