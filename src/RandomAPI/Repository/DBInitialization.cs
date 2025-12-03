using System.Data;
using Dapper;

public static class DBInitialization
{
    public const string CONNECTIONSTRING = "Data Source=PersonalDev.db";

    public static async Task EnsureDb(IServiceProvider services)
    {
        // Use an isolated scope for startup to safely create the connection
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IDbConnection>();

        //to make more tasbles, write the sql and await and chain it :)
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
}
