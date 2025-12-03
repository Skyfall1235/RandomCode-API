using Dapper;
using RandomAPI.Models;
using System.Data;

namespace RandomAPI.Repository
{

    /// <summary>
    /// Implements CRUD operations for Webhook URLs using Dapper and the provided DatabaseService.
    /// </summary>
    public class WebhookRepository : IWebhookRepository, IInitializer
    {
        private readonly IDbConnection _db;

        public WebhookRepository(IDbConnection dbService)
        {
            _db = dbService;
        }

        /// <summary>
        /// Ensures the WebhookUrls table exists in the SQLite database.
        /// </summary>
        public async Task InitializeAsync()
        {
            // Define the table structure with a unique constraint on the Url to prevent duplicates
            const string sql = @"
            CREATE TABLE IF NOT EXISTS WebhookUrls (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Url TEXT NOT NULL UNIQUE
            );";

            await _db.ExecuteAsync(sql);
        }

        /// <summary>
        /// Retrieves all registered webhook URLs from the database.
        /// </summary>
        /// <returns>A collection of WebhookUrl objects.</returns>
        public async Task<IEnumerable<WebhookUrl>> GetAllUrlsAsync()
        {
            const string sql = "SELECT Id, Url FROM WebhookUrls ORDER BY Id;";
            // Dapper maps the columns to the WebhookUrl model properties
            return await _db.QueryAsync<WebhookUrl>(sql);
        }

        /// <summary>
        /// Adds a new URL to the database. Uses INSERT OR IGNORE to handle duplicates gracefully.
        /// </summary>
        /// <param name="url">The URL string to add.</param>
        public async Task AddUrlAsync(string url)
        {
            // SQLITE specific command to ignore unique constraint errors if URL already exists
            const string sql = "INSERT OR IGNORE INTO WebhookUrls (Url) VALUES (@Url);";
            await _db.ExecuteAsync(sql, new { Url = url });
        }

        /// <summary>
        /// Deletes a URL from the database.
        /// </summary>
        /// <param name="url">The URL string to delete.</param>
        /// <returns>The number of rows deleted (0 or 1).</returns>
        public async Task<int> DeleteUrlAsync(string url)
        {
            const string sql = "DELETE FROM WebhookUrls WHERE Url = @Url;";
            return await _db.ExecuteAsync(sql, new { Url = url });
        }
    }
}
