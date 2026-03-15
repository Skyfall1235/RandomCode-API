using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Data.Sqlite;
using RandomAPI.Repository;
using RandomAPI.Services.Webhooks;
using System.Collections.Generic;
using System.Data;
using System.Threading.RateLimiting;

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
//db
builder.Services.AddScoped<IDatabaseService, DatabaseService>();
builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<IWebhookRepository, WebhookRepository>();
builder.Services.AddScoped<ILastParkedCarRepository, LastParkedLocationRepository>();

//qr
builder.Services.AddScoped<IQrService, QrService>();


//webhook
builder.Services.AddScoped<IWebhookService, WebhookActionService>();

//parked car location
builder.Services.AddScoped<ILastParkedLocationService, LastParkedLocationService>();

//CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

#endregion

#region Initialization
//scanner for initializations
builder.Services.Scan(scan => scan
    .FromAssemblyOf<IInitializer>()
    .AddClasses(c => c.AssignableTo<IInitializer>())
        .As<IInitializer>()
        .WithScopedLifetime()
);

//force swagger to req api keys
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "RandomAPI", Version = "v1" });

    // 1. Define the API Key security scheme
    c.AddSecurityDefinition("ApiKey", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "Enter your API key in the box below. Format: X-Api-Key: your-key",
        Name = "X-Api-Key",      // The name of the header
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "ApiKeyScheme"
    });

    // 2. Make sure Swagger actually uses it for requests
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                }
            },
            new string[] { }
        }
    });
});

//this is the rate limiting stuff middleare.
builder.Services.AddRateLimiter(options =>
{
    // This runs whenever ANY policy is triggered
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("ParkingApiPolicy", httpContext =>
    {
        var remoteIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(remoteIp, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 10,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        });
    });

    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsync(
            "Too many requests. Please wait a minute before trying again.",
            cancellationToken: token);
    };
});

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
    app.UseDeveloperExceptionPage();
}

app.UseCors();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();


// TODO:
// - good logging _service.

// - 

// - AlertGatewayService
//      API Endpoint Goal: POST / alert / ingest
//      Brief Description(Project Scope): Centralized Notification Hub with Discord Integration.
//      Receives generic webhooks (from Sentry, CI/CD, etc.), standardizes the payload, applies personalized filtering rules, and routes the cleaned alert to your Discord channel using a webhook.
//      The _service will format the alert into a visually appealing Discord Embed using a library like discord-webhook or requests.

// - HealthCheckAggregator
//      API Endpoint Goal: GET / health / summary
//      Brief Description(Project Scope): Unified System Status Dashboard.
//      Periodically polls the health endpoints (/status or /health) of critical development services (Database, Backend API, CI/CD pipeline).
//      Aggregates the results into a single, simplified GREEN/YELLOW/RED status JSON response for quick checking.

// - endpoint that subscribes to a youtuber channel, and whwenver theyt upload a video, we download it and display it
// - a safe LOGIN _service
// - using the login _service, i could store info about my parked car

//rate limiter
//action requioring api key :)

//what if we stored all the authernticator stuff on a db for me to be able to access any auth at any time? that cant be easdy but maybe we could start with google ones and move to others eventually?




