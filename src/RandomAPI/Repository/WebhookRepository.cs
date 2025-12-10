using Dapper;
using Microsoft.Data.Sqlite;
using RandomAPI.Models;
using System.Data;
using RandomAPI.Services.Webhooks;

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
                Id TEXT PRIMARY KEY AUTOINCREMENT,
                Url TEXT NOT NULL UNIQUE,
                Source TEXT NOT NULL 
            );";

            await db.ExecuteAsync(sql);
        }

        /// <summary>
        /// Retrieves all registered webhook URLs from the database.
        /// </summary>
        /// <returns>A collection of WebhookUrl objects.</returns>
        public async Task<IEnumerable<WebhookUrl>> GetAllUrlsAsync()
        {
            using var db = CreateConnection();
            const string sql = "SELECT Id, Url, Source FROM WebhookUrls ORDER BY Id;";
            return await db.QueryAsync<WebhookUrl>(sql);
        }

        public async Task<IEnumerable<WebhookUrl>> GetUrlsOfSourceAsync(string source)
        {
            using var db = CreateConnection();
            const string sql = "SELECT Id, Url, Source FROM WebhookUrls WHERE Source = @Source;";
            var parameters = new { Source = source };

            return await db.QueryAsync<WebhookUrl>(sql, parameters);
        }

        /// <summary>
        /// Adds a new URL to the database. Uses INSERT OR IGNORE to handle duplicates gracefully.
        /// </summary>
        /// <param name="url">The URL string to add.</param>
        /// <param name="source">The source string to categorize the URL.</param>
        public async Task AddUrlAsync(string url, string source)
        {
            using var db = CreateConnection();
            const string sql = "INSERT OR IGNORE INTO WebhookUrls (Url, Source) VALUES (@Url, @Source);";
            var parameters = new
            {
                Url = url,
                Source = source
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
