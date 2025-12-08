
/// <summary>
/// The centralized background service that monitors the clock and executes 
/// all registered ICronScheduledTask instances based on their Schedule property.
/// </summary>
public class CronTaskRunnerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CronTaskRunnerService> _logger;

    // The runner checks the schedule every 60 seconds (or less if needed).
    private static readonly TimeSpan CheckInterval = TimeSpan.FromSeconds(60);

    public CronTaskRunnerService(
        ILogger<CronTaskRunnerService> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Logic to check if a task's schedule matches the current minute.
    /// Uses a custom format: [DayOfWeek|Daily]@[HH:MM]
    /// </summary>
    private static bool IsScheduleDue(string schedule, DateTime now)
    {
        // Example: "Sunday@00:00"
        if (string.IsNullOrWhiteSpace(schedule) || !schedule.Contains('@')) return false;

        var parts = schedule.Split('@');
        var dayPart = parts[0].Trim();
        var timePart = parts[1].Trim();

        // 1. Check Time (HH:MM)
        // Check if the current hour and minute match the scheduled time
        if (!TimeSpan.TryParseExact(timePart, "hh\\:mm", null, out TimeSpan scheduledTime))
        {
            return false;
        }

        // Only fire if the current UTC hour and minute match the scheduled time
        if (now.Hour != scheduledTime.Hours || now.Minute != scheduledTime.Minutes)
        {
            return false;
        }

        // 2. Check Day (Daily or Specific DayOfWeek)
        if (dayPart.Equals("Daily", StringComparison.OrdinalIgnoreCase))
        {
            return true; // Scheduled to run every day at this time
        }

        // Check if the current DayOfWeek matches the scheduled day
        if (Enum.TryParse<DayOfWeek>(dayPart, true, out DayOfWeek scheduledDay))
        {
            return now.DayOfWeek == scheduledDay;
        }

        return false;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Cron Task Runner Service started. Checking schedule every minute.");

        while (!stoppingToken.IsCancellationRequested)
        {
            // Get the current time in UTC, rounded down to the nearest minute.
            var now = DateTime.UtcNow.AddSeconds(-DateTime.UtcNow.Second);
            _logger.LogDebug($"Checking schedules for time: {now:yyyy-MM-dd HH:mm} UTC");

            try
            {
                // Must create a scope for each execution cycle
                using (var scope = _serviceProvider.CreateScope())
                {
                    // Resolve ALL services registered under the ICronScheduledTask contract.
                    IEnumerable<ICronScheduledTask> scheduledTasks =
                        scope.ServiceProvider.GetServices<ICronScheduledTask>();

                    var tasksToRun = scheduledTasks
                        .Where(task => IsScheduleDue(task.Schedule, now))
                        .ToList();

                    if (tasksToRun.Any())
                    {
                        _logger.LogInformation($"Found {tasksToRun.Count} tasks due now. Executing concurrently.");

                        var executionTasks = tasksToRun
                            .Select(task => task.ExecuteAsync().ContinueWith(t =>
                            {
                                if (t.IsFaulted)
                                {
                                    _logger.LogError(t.Exception, $"Task '{task.Name}' failed to execute.");
                                }
                            }, TaskContinuationOptions.ExecuteSynchronously)) // Ensure logging is safe
                            .ToList();

                        await Task.WhenAll(executionTasks);
                        _logger.LogInformation("Concurrent execution completed for this cycle.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled error occurred during the Cron execution cycle.");
            }

            // Wait for the next interval check.
            await Task.Delay(CheckInterval, stoppingToken);
        }

        _logger.LogInformation("Cron Task Runner Service stopping.");
    }
}