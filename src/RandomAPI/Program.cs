var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

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


//TODO:
// - good logging service. rabapp has an event table, i bet i could do something worse

// - add a db. sqlite will probably work here since its small
//how tf will i get a db in here

// - AlertGatewayService
//      API Endpoint Goal: POST / alert / ingest
//      Brief Description(Project Scope): Centralized Notification Hub with Discord Integration.
//      Receives generic webhooks (from Sentry, CI/CD, etc.), standardizes the payload, applies personalized filtering rules, and routes the cleaned alert to your Discord channel using a webhook.
//      The service will format the alert into a visually appealing Discord Embed using a library like discord-webhook or requests.
public class WebhookPayload
{
    //payload, timestamp, event enum probably
}
public interface IWebhookService
{
    Task SendWebhookAsync(object payload);
    void AddListener(string url);
    IEnumerable<string> GetListeners();
}

public class WebhookService : IWebhookService
{
    private readonly HttpClient _httpClient;
    private readonly ConcurrentBag<string> _listeners = new ConcurrentBag<string>();

    public WebhookService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public void AddListener(string url)
    {
        // validate url here
        _listeners.Add(url);
    }

    public IEnumerable<string> GetListeners() => _listeners;

    public async Task SendWebhookAsync(WebhookPayload payload)
    {
        string json = JsonConvert.SerializeObject(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
//this can be wayyyyy better
        foreach (var url in _listeners)
        {
            try
            {
                var response = await _httpClient.PostAsync(url, content);
                Console.WriteLine($"Sent to {url}: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending to {url}: {ex.Message}");
            }
        }
    }
}


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
// will require a db


// - basic sqlite db
public class DatabaseService
{
    private readonly string _connectionString;

    public DatabaseService(string dbPath = "Data/myapp.db")
    {
        //WILL NEED TO BE UPDATED
        _connectionString = $"Data Source={dbPath}";
    }

    /// <summary>
    /// Opens a connection to SQLite, executes an action, then closes the connection.
    /// </summary>
    public void Execute(Action<SqliteConnection> action)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        action(connection);
    }
}
