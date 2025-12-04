using System.Data;
using Dapper;
using Microsoft.Data.Sqlite;
using RandomAPI.Models;

namespace RandomAPI.Repository
{
    public class EventRepository : IEventRepository, IInitializer
    {
        private readonly Func<IDbConnection> _connectionFactory;
        private readonly ILogger<EventRepository> _logger;

        public EventRepository(Func<IDbConnection> connectionFactory, ILogger<EventRepository> logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
        }

        private IDbConnection CreateConnection()
        {
            var conn = _connectionFactory();
            conn.Open();
            return conn;
        }

        public async Task InitializeAsync()
        {
            using var db = CreateConnection();
            var sql =
                    @"
        CREATE TABLE IF NOT EXISTS Events (
            Id          INTEGER PRIMARY KEY AUTOINCREMENT,
            Timestamp   TEXT NOT NULL,
            Service     TEXT NOT NULL,
            Type        TEXT NOT NULL,
            DataType    TEXT,
            JsonData    TEXT NOT NULL,
            EventId     TEXT NOT NULL,
            CONSTRAINT UQ_EventId UNIQUE (EventId)
        );";
            await db.ExecuteAsync(sql);
        }

        /// <inheritdoc />
        public async Task<int> AddEventAsync(Event eventModel)
        {
            using var db = CreateConnection();
            const string sql =
                @"
                INSERT INTO Events (Timestamp, EventId, Service, Type, DataType, JsonData) 
                VALUES (@Timestamp, @EventId, @Service, @Type, @DataType, @JsonData);
                
                SELECT last_insert_rowid();";

            try
            {
                var newId = await db.ExecuteScalarAsync<int>(sql, eventModel);
                return newId;
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 19) // Error code 19 is 'CONSTRAINT'
            {
                _logger.LogWarning(
                    $"WARNING: Duplicate event detected. EventId: {eventModel.EventId}"
                );
                const string selectExistingSql = "SELECT Id FROM Events WHERE EventId = @EventId";
                var existingId = await db.ExecuteScalarAsync<int>(
                    selectExistingSql,
                    new { eventModel.EventId }
                );
                return existingId;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR during event insertion: {ex.Message}");
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Event>> GetAllEventsAsync()
        {
            using var db = CreateConnection();
            // Retrieves all records, ordered by newest first.
            const string sql = "SELECT * FROM Events ORDER BY Timestamp DESC";

            var events = await db.QueryAsync<Event>(sql);
            return events;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Event>> GetRangeOfRecentEventsAsync(int count)
        {
            using var db = CreateConnection();
            const string sql = "SELECT * FROM Events ORDER BY Timestamp DESC LIMIT @Count";

            var events = await db.QueryAsync<Event>(sql, new { Count = count });
            return events;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Event>> GetEventsByIdsAsync(IEnumerable<int> ids)
        {
            if (ids == null || !ids.Any())
                return Enumerable.Empty<Event>();

            using var db = CreateConnection();
            const string sql = "SELECT * FROM Events WHERE Id IN @Ids ORDER BY Timestamp DESC";

            var events = await db.QueryAsync<Event>(sql, new { Ids = ids });
            return events;
        }

        /// <inheritdoc />
        public async Task<int> RemoveEventAsync(int id)
        {
            using var db = CreateConnection();
            const string sql = "DELETE FROM Events WHERE Id = @Id";

            var rowsAffected = await db.ExecuteAsync(sql, new { Id = id });
            return rowsAffected;
        }

        /// <inheritdoc />
        public async Task<int> RemoveEventsByIdsAsync(IEnumerable<int> ids)
        {
            if (ids == null || !ids.Any())
                return 0;
            using var db = CreateConnection();
            const string sql = "DELETE FROM Events WHERE Id IN @Ids";

            var rowsAffected = await db.ExecuteAsync(sql, new { Ids = ids });
            return rowsAffected;
        }
    }
}


