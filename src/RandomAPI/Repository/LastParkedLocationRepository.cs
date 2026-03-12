using Dapper;
using RandomAPI.Repository;
using System.Data;
using RandomAPI.Models;

public class LastParkedLocationRepository : ILastParkedCarRepository, IInitializer
{
    private readonly Func<IDbConnection> _connectionFactory;

    public LastParkedLocationRepository(Func<IDbConnection> connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    private IDbConnection CreateConnection()
    {
        var conn = _connectionFactory();
        if (conn.State != ConnectionState.Open) conn.Open();
        return conn;
    }

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
                PhotoPath TEXT,
                IsActive INTEGER DEFAULT 1
            );";

        await db.ExecuteAsync(sql);
    }

    public async Task<LastParkedLocation?> GetActiveLocationAsync(string vehicleId)
    {
        using var db = CreateConnection();
        const string sql = @"
        SELECT 
            Timestamp AS ParkedAt,
            VehicleId, 
            Latitude, 
            Longitude, 
            Accuracy, 
            Floor AS FloorLevel,
            Section, 
            Note AS Notes,         
            PhotoPath, 
            IsActive
        FROM LastParkedLocation
        WHERE VehicleId = @vehicleId AND IsActive = 1
        ORDER BY Timestamp DESC
        LIMIT 1;";

        return await db.QuerySingleOrDefaultAsync<LastParkedLocation>(sql, new { vehicleId });
    }

    public async Task<IEnumerable<LastParkedLocation>> GetLocationHistoryAsync(string vehicleId)
    {
        using var db = CreateConnection();
        const string sql = @"
            SELECT * FROM LastParkedLocation 
            WHERE VehicleId = @vehicleId AND IsActive = 0
            ORDER BY Timestamp DESC;";

        return await db.QueryAsync<LastParkedLocation>(sql, new { vehicleId });
    }

    public async Task SaveLocationAsync(LastParkedLocation location)
    {
        using var db = CreateConnection();
        const string sql = @"
            INSERT INTO LastParkedLocation (
            VehicleId, Timestamp, Latitude, Longitude, 
            Accuracy, Floor, Section, Note, PhotoPath, IsActive
        ) 
        VALUES (
            @VehicleId, @ParkedAt, @Latitude, @Longitude, 
            @Accuracy, @FloorLevel, @Section, @Notes, @PhotoPath, @IsActive
        );";

        // Dapper automatically maps properties from the 'location' object to @parameters
        await db.ExecuteAsync(sql, location);
    }

    public async Task DeactivateOldLocationsAsync(string vehicleId)
    {
        using var db = CreateConnection();
        const string sql = @"
        UPDATE LastParkedLocation 
        SET IsActive = 0
        WHERE VehicleId = @vehicleId AND IsActive = 1;";

        // This works! Dapper finds 'vehicleId' inside the anonymous object
        await db.ExecuteAsync(sql, new { vehicleId });
    }
}