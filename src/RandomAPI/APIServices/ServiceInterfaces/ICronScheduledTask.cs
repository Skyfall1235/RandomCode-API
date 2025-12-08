
/// <summary>
/// Contract for a scheduled background task that includes its execution logic and its specific schedule.
/// </summary>
public interface ICronScheduledTask
{
    /// <summary>
    /// A unique name for logging and identification.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// The schedule definition for the task. 
    /// Format: [DayOfWeek|Daily]@[HH:MM] 
    /// Examples: "Daily@08:30", "Monday@08:30", "Sunday@00:00"
    /// </summary>
    string Schedule { get; }

    /// <summary>
    /// Executes the scheduled job logic.
    /// </summary>
    Task ExecuteAsync();
}


/// <summary>
/// A daily scheduled task for routine maintenance (e.g., database cleanup, log rotation).
/// Runs every day at 04:00 AM UTC.
/// </summary>
public class DailyMaintenanceTask : ICronScheduledTask
{
    private readonly ILogger<DailyMaintenanceTask> _logger;

    public DailyMaintenanceTask(ILogger<DailyMaintenanceTask> logger)
    {
        _logger = logger;
    }

    // --- ICronScheduledTask Implementation ---
    public string Name => "Daily Maintenance & Cleanup";
    public string Schedule => "Daily@04:00"; // Run every day at 4:00 AM UTC
    // ----------------------------------------

    public async Task ExecuteAsync()
    {
        _logger.LogInformation("Executing Daily Maintenance Task: Running routine cleanup.");

        // --- Actual Scheduled Logic ---
        await Task.Delay(100); // Simulate asynchronous database cleanup or file deletion
        // Note: For a real cleanup task, you would inject and use a repository here.
        // ------------------------------

        _logger.LogInformation("Daily Maintenance Task completed successfully.");
    }
}

/// <summary>
/// Scheduled task for generating a report every Monday morning.
/// </summary>
public class BiWeeklyReportTaskMonday : ICronScheduledTask
{
    private readonly ILogger<BiWeeklyReportTaskMonday> _logger;

    public BiWeeklyReportTaskMonday(ILogger<BiWeeklyReportTaskMonday> logger)
    {
        _logger = logger;
    }

    public string Name => "Bi-Weekly Report (Monday)";
    public string Schedule => "Monday@08:30"; // Run every Monday at 8:30 AM UTC

    public async Task ExecuteAsync()
    {
        _logger.LogInformation("Executing Monday Report Task: Compiling weekly progress report.");
        await Task.Delay(100); // Simulate report generation
        _logger.LogInformation("Monday Report Task completed successfully.");
    }
}

/// <summary>
/// Scheduled task for generating a report every Friday morning.
/// </summary>
public class BiWeeklyReportTaskFriday : ICronScheduledTask
{
    private readonly ILogger<BiWeeklyReportTaskFriday> _logger;

    public BiWeeklyReportTaskFriday(ILogger<BiWeeklyReportTaskFriday> logger)
    {
        _logger = logger;
    }

    public string Name => "Bi-Weekly Report (Friday)";
    public string Schedule => "Friday@08:30"; // Run every Friday at 8:30 AM UTC

    public async Task ExecuteAsync()
    {
        _logger.LogInformation("Executing Friday Report Task: Compiling end-of-week summary report.");
        await Task.Delay(100); // Simulate report generation
        _logger.LogInformation("Friday Report Task completed successfully.");
    }
}