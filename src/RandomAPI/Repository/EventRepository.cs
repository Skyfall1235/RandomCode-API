using Dapper;
using RandomAPI.Models;
using System.Data;
using Microsoft.Data.Sqlite;

namespace RandomAPI.Repository
{
    public class EventRepository : IEventRepository
    {
        private readonly IDbConnection _db;
        private readonly ILogger<EventRepository> _logger;
        public EventRepository(IDbConnection db, ILogger<EventRepository> logger)
        {
            _db = db;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<int> AddEventAsync(Event eventModel)
        {
            const string sql = @"
                INSERT INTO Events (Timestamp, EventId, Service, Type, DataType, JsonData) 
                VALUES (@Timestamp, @EventId, @Service, @Type, @DataType, @JsonData);
                
                SELECT last_insert_rowid();";

            try
            {
                var newId = await _db.ExecuteScalarAsync<int>(sql, eventModel);
                return newId;
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 19) // Error code 19 is 'CONSTRAINT'
            {
                _logger.LogWarning($"WARNING: Duplicate event detected. EventId: {eventModel.EventId}");
                const string selectExistingSql = "SELECT Id FROM Events WHERE EventId = @EventId";
                var existingId = await _db.ExecuteScalarAsync<int>(selectExistingSql, new { eventModel.EventId });
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
            // Retrieves all records, ordered by newest first.
            const string sql = "SELECT * FROM Events ORDER BY Timestamp DESC";

            var events = await _db.QueryAsync<Event>(sql);
            return events;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Event>> GetRangeOfRecentEventsAsync(int count)
        {
            const string sql = "SELECT * FROM Events ORDER BY Timestamp DESC LIMIT @Count";

            var events = await _db.QueryAsync<Event>(sql, new { Count = count });
            return events;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Event>> GetEventsByIdsAsync(IEnumerable<int> ids)
        {
            if (ids == null || !ids.Any()) return Enumerable.Empty<Event>();

            const string sql = "SELECT * FROM Events WHERE Id IN @Ids ORDER BY Timestamp DESC";

            var events = await _db.QueryAsync<Event>(sql, new { Ids = ids });
            return events;
        }

        /// <inheritdoc />
        public async Task<int> RemoveEventAsync(int id)
        {
            const string sql = "DELETE FROM Events WHERE Id = @Id";

            var rowsAffected = await _db.ExecuteAsync(sql, new { Id = id });
            return rowsAffected;
        }

        /// <inheritdoc />
        public async Task<int> RemoveEventsByIdsAsync(IEnumerable<int> ids)
        {
            if (ids == null || !ids.Any()) return 0;

            const string sql = "DELETE FROM Events WHERE Id IN @Ids";

            var rowsAffected = await _db.ExecuteAsync(sql, new { Ids = ids });
            return rowsAffected;
        }
    }
}
