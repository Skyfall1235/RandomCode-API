using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using RandomAPI.DTOs;

public class TimeOutService(ILogger<IHoursService> logger) : IHoursService
{
    public HoursResponseDto Calculate(HoursRequestDto request)
    {
        var rounded = request.Hours.Select(RoundToQuarter).ToList();

        double runningTotal = 0;
        List<double> cumulative = new();

        foreach (double r in rounded)
        {
            runningTotal += r;
            cumulative.Add(runningTotal);
        }

        double hoursWorkedActual = request.Hours.Sum();
        double hoursWorkedRounded = cumulative.LastOrDefault();
        double remaining = Math.Max(0, 40 - hoursWorkedRounded);

        DateTime timeIn = ParseTimeInput(request.TimeIn);
        TimeSpan delta = TimeSpan.FromMinutes(6);

        DateTime mean = timeIn.AddHours(remaining);
        DateTime min = mean - delta;
        DateTime max = mean + delta;

        return new HoursResponseDto
        {
            HoursWorkedActual = hoursWorkedActual,
            HoursWorkedRounded = hoursWorkedRounded,
            RemainingHours = remaining,
            MinTimeOut = min.ToString("hh:mm tt"),
            MaxTimeOut = max.ToString("hh:mm tt"),
        };
    }

    // Helpers
    private DateTime ParseTimeInput(string input)
    {
        input = input.Trim().ToLower().Replace(" ", "");

        if (!input.Contains("am") && !input.Contains("pm"))
            input += "am";

        if (Regex.IsMatch(input, @"^\d{1,2}(am|pm)$"))
            input = input.Replace("am", ":00am").Replace("pm", ":00pm");

        string[] formats = { "h:mmtt", "hh:mmtt", "htt", "hhtt", "h tt", "hh tt" };

        if (
            DateTime.TryParseExact(
                input,
                formats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime parsed
            )
        )
        {
            return parsed;
        }
        //if fail, throw and log
        Exception e = new FormatException("Invalid time format.");
        logger.LogWarning(e, "Invalid time format.");
        throw e;
    }

    private double RoundToQuarter(double hours)
    {
        return Math.Round(hours * 4) / 4.0;
    }
}
