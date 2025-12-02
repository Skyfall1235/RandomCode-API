using System.Data;
using Microsoft.Data.Sqlite;


var builder = WebApplication.CreateBuilder(args);


// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddTransient<IDbConnection>(_ => new SqliteConnection(DBInitialization.CONNECTIONSTRING));

var app = builder.Build();
await DBInitialization.EnsureDb(app.Services);

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

// - add a db. sqlite will probably work here since its small
//how tf will i get a db in here

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
