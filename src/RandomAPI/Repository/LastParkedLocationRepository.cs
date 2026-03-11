using Dapper;
using RandomAPI.Repository;
using System.Data;
using RandomAPI.Models;

public class LastParkedLocationRepository : ILastParkedCarRepository, IInitializer
{
    private readonly Func<IDbConnection> _connectionFactory;
    private readonly DatabaseService _databaseService;

    public LastParkedLocationRepository(Func<IDbConnection> connectionFactory, DatabaseService databaseService)
    {
        _connectionFactory = connectionFactory;
        _databaseService = databaseService;
    }

    private IDbConnection CreateConnection()
    {
        var conn = _connectionFactory();
        conn.Open();
        return conn;
    }

    /// <summary>
    /// Ensures the table exists in the SQLite database.
    /// </summary>
    public async Task InitializeAsync()
    {
        using var db = CreateConnection();
        const string sql = @"
            CREATE TABLE IF NOT EXISTS LastParkedLocation (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            VehicleId TEXT,
            Timestamp TEXT NOT NULL,
            Latitude REAL NOT NULL,
            Longitude REAL NOT NULL,
            Accuracy REAL,
            Floor INTEGER,
            Section TEXT,
            Note TEXT,
            PhotoPath TEXT,`
            IsActive INTEGER DEFAULT 1
            );";

        await db.ExecuteAsync(sql);
    }

    public async Task<LastParkedLocation?> GetActiveLocationAsync(string vehicleId)
    {

        const string sql =
            @"SELECT * FROM LastParkedLocation
              WHERE VehicleId = @vehicleId AND IsActive = 1
              ORDER BY Timestamp DESC
              LIMIT 1;";
        return await _databaseService.ExecuteScalarAsync<LastParkedLocation>(sql, vehicleId);
    }

    public async Task<IEnumerable<LastParkedLocation>> GetLocationHistoryAsync(string vehicleId)
    {
        const string sql = @"
            SELECT * FROM LastParkedLocation 
            WHERE VehicleId = @vehicleId AND IsActive = 0
            ORDER BY Timestamp DESC;";

        return await _databaseService.QueryAsync<LastParkedLocation>(sql, vehicleId);
    }

    public async Task SaveLocationAsync(LastParkedLocation location)
    {
        const string sql = @"
            INSERT INTO LastParkedLocation (
                VehicleId, Timestamp, Latitude, Longitude, 
                Accuracy, Floor, Section, Note, PhotoPath, IsActive
            ) 
            VALUES (
                @VehicleId, @Timestamp, @Latitude, @Longitude, 
                @Accuracy, @FloorLevel, @Section, @Notes, @PhotoPath, @IsActive
            );";
        await _databaseService.ExecuteAsync(sql, location);
    }

    public async Task DeactivateOldLocationsAsync(string vehicleId)
    {
        const string sql = @"
            UPDATE LastParkedLocation 
            SET IsActive = 0
            WHERE VehicleId = @vehicleId AND IsActive = 1;";
        await _databaseService.ExecuteAsync(sql, vehicleId);
    }
}

