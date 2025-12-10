
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
}