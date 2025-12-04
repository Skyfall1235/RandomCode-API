using System.Data;
using Microsoft.Data.Sqlite;
using RandomAPI.Repository;
using RandomAPI.Services.Webhooks;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IDbConnection>(sp =>
{
    var conn = new SqliteConnection("Data Source=PersonalDev.db;Cache=Shared");
    conn.Open();
    return conn;
});
builder.Services.AddScoped<Func<IDbConnection>>(sp =>
    () => sp.GetRequiredService<IDbConnection>());


#region Add Services
//webhook
builder.Services.AddScoped<IWebhookService, WebhookActionService>();


//time clock service
builder.Services.AddSingleton<IHoursService, TimeOutService>();

//db
builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<IWebhookRepository, WebhookRepository>();

#endregion

#region Initialization
//scanner for initializations
builder.Services.Scan(scan => scan
    .FromAssemblyOf<IInitializer>()
    .AddClasses(c => c.AssignableTo<IInitializer>())
        .As<IInitializer>()
        .WithScopedLifetime()
);

var app = builder.Build();
//the the end, init the dbs
using (var scope = app.Services.CreateScope())
{
    IServiceProvider? serviceProvider = scope.ServiceProvider;
    IEnumerable<IInitializer>? initializers = serviceProvider.GetServices<IInitializer>();
    if (!initializers.Any())
    {
        Console.WriteLine("Warning: No services implementing IInitializer were found.");
    }
    IEnumerable<Task>? initializationTasks = initializers.Select(i => i.InitializeAsync());
    await Task.WhenAll(initializationTasks);
}
#endregion

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

// TODO:
// - good logging service. rabapp has an event table, i bet i could do something worse

// - AlertGatewayService
//      API Endpoint Goal: POST / alert / ingest
//      Brief Description(Project Scope): Centralized Notification Hub with Discord Integration.
//      Receives generic webhooks (from Sentry, CI/CD, etc.), standardizes the payload, applies personalized filtering rules, and routes the cleaned alert to your Discord channel using a webhook.
//      The service will format the alert into a visually appealing Discord Embed using a library like discord-webhook or requests.

// - HealthCheckAggregator
//      API Endpoint Goal: GET / health / summary
//      Brief Description(Project Scope): Unified System Status Dashboard.
//      Periodically polls the health endpoints (/status or /health) of critical development services (Database, Backend API, CI/CD pipeline).
//      Aggregates the results into a single, simplified GREEN/YELLOW/RED status JSON response for quick checking.
public class HealthCheckService { }

// - UniversalSnippetStore
//      API Endpoint Goal: POST / snippet and GET /snippet/search
//      Brief Description (Project Scope): Personal Developer Knowledge Base.
//      A CRUD API to save and retrieve frequently forgotten code snippets, complex CLI commands, and database queries.
//      Supports robust searching by language (python, sql) and customizable tags (regex, lambda, auth).
public class UniversalSnippetService { }
