using RandomAPI.Repository;
using RandomAPI.Models;
using System.Text.Json;

// PRODUCTION READY: 
// To make this file compile with the real logic uncommented, you must install:
//Google.Apis.Calendar.v3(NuGet)
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;

namespace RandomAPI.DTOs
{
    /// <summary>
    /// Data Transfer Object representing a single scheduled calendar event.
    /// </summary>
    public class CalendarEventDTO
    {
        public required string Summary { get; set; }
        public required DateTime StartTime { get; set; }
        public required DateTime EndTime { get; set; }
        public string? Location { get; set; }
        public string? Description { get; set; }
    }
    /// <summary>
    /// Contract for interacting with the Google Calendar API.
    /// (Uses a mock implementation until full API integration is configured.)
    /// </summary>
    public interface IGoogleCalendarService
    {
        /// <summary>
        /// Fetches all events scheduled for a specific day.
        /// </summary>
        /// <param name="date">The day for which to fetch events (e.g., Monday or Friday).</param>
        /// <returns>A list of scheduled events.</returns>
        Task<List<CalendarEventDTO>> GetEventsForDayAsync(DateTime date);
    }

    /// <summary>
    /// MOCK implementation of the Google Calendar Service. 
    /// NOTE: In a production environment, this is where you would integrate 
    /// the Google.Apis.Calendar.v3 package and use OAuth credentials.
    /// </summary>
    public class GoogleCalendarService : IGoogleCalendarService
    {
        private readonly ILogger<GoogleCalendarService> _logger;
        private readonly IEventRepository _eventRepository;

        public GoogleCalendarService(ILogger<GoogleCalendarService> logger, IEventRepository eventRepository)
        {
            _logger = logger;
            _eventRepository = eventRepository;
        }

        public async Task<List<CalendarEventDTO>> GetEventsForDayAsync(DateTime date)
        {
            _logger.LogWarning("Using MOCK Google Calendar Service. No real API call is being made.");

            // --- Placeholder Data Generation ---

            var mockEvents = new List<CalendarEventDTO>();

            if (date.DayOfWeek == DayOfWeek.Monday)
            {
                mockEvents.Add(new CalendarEventDTO
                {
                    Summary = "Weekly Planning Meeting",
                    StartTime = date.Date.AddHours(9).AddMinutes(30),
                    EndTime = date.Date.AddHours(10).AddMinutes(30),
                    Location = "Zoom Link",
                    Description = "Review last week's tickets and plan sprints."
                });
                mockEvents.Add(new CalendarEventDTO
                {
                    Summary = "Client Check-in Call",
                    StartTime = date.Date.AddHours(14).AddMinutes(0),
                    EndTime = date.Date.AddHours(14).AddMinutes(45),
                    Location = "Google Meet",
                });
            }
            else if (date.DayOfWeek == DayOfWeek.Friday)
            {
                mockEvents.Add(new CalendarEventDTO
                {
                    Summary = "Project Retrospective",
                    StartTime = date.Date.AddHours(11).AddMinutes(0),
                    EndTime = date.Date.AddHours(12).AddMinutes(0),
                    Location = "Office Lounge",
                    Description = "Discuss what went well and areas for improvement."
                });
            }

            _logger.LogInformation($"Mock events generated for {date.ToShortDateString()}: {mockEvents.Count} events.");

            try
            {
                var jsonData = JsonSerializer.Serialize(mockEvents);

                var logEntry = new Models.Event(
                    service: "GoogleCalendarService",
                    type: "EventFetch",
                    jsonData: jsonData
                );

                _logger.LogDebug($"AUDIT LOG: Calendar events pulled. {mockEvents.Count} events logged.");
                await _eventRepository.AddEventAsync(logEntry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log calendar fetch activity to the Event Repository.");
            }

            return mockEvents;
        }
    }


    /// <summary>
    /// Contract for a service dedicated to external interactions with the Google Calendar API.
    /// This layer is responsible for network communication and parsing raw data.
    /// </summary>
    public interface IGoogleCalendarExternalService
    {
        /// <summary>
        /// Fetches calendar events directly from the Google API for a specific day.
        /// </summary>
        /// <param name="date">The day for which to fetch events.</param>
        /// <returns>A list of scheduled events (DTOs).</returns>
        Task<List<CalendarEventDTO>> GetRawEventsForDayAsync(DateTime date);
    }

    

    /// <summary>
    /// PRODUCTION implementation of the external Google Calendar client.
    /// This service is solely responsible for fetching data from the real API 
    /// using an injected, authenticated client.
    /// </summary>
    public class GoogleCalendarExternalService : IGoogleCalendarExternalService
    {
        private readonly ILogger<GoogleCalendarExternalService> _logger;

        // PRODUCTION: This field holds the actual Google API client.
        private readonly CalendarService _calendarApi; 

        public GoogleCalendarExternalService(
            ILogger<GoogleCalendarExternalService> logger, CalendarService calendarApi ) // PRODUCTION: Inject authenticated client
        {
            _logger = logger;
            _calendarApi = calendarApi;
        }

        public async Task<List<CalendarEventDTO>> GetRawEventsForDayAsync(DateTime date)
        {
            _logger.LogInformation($"Attempting to fetch events from Google API for: {date.ToShortDateString()}.");
            var fetchedEvents = new List<CalendarEventDTO>();

            try
            {
                // 1. Define the service request parameters
                var request = _calendarApi.Events.List("primary"); // "primary" is the user's main calendar ID

                // TimeMin and TimeMax must be defined in RFC3339 format, often handled by DateTimeOffset conversion
                request.TimeMin = date.Date;
                request.TimeMax = date.Date.AddDays(1); // Fetch for the whole day
                request.SingleEvents = true; // Required to expand recurring events into individual occurrences
                request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;
                request.ShowDeleted = false; // Only get active events

                // 2. Execute the request
                // Events is the Google API's data structure
                Events apiEvents = await request.ExecuteAsync();

                // 3. Map Google API Events (Event) to your DTO (CalendarEventDTO)
                if (apiEvents.Items != null)
                {
                    fetchedEvents = apiEvents.Items
                        .Where(item => item.Status != "cancelled")
                        .Select(item => new CalendarEventDTO
                        {
                            Summary = item.Summary,
                            Location = item.Location,
                            Description = item.Description,
                            // Note: We check for the preferred DateTimeOffset values first
                            StartTime = item.Start.DateTimeOffset.HasValue
                                        ? item.Start.DateTimeOffset.Value.DateTime
                                        : item.Start.Date is string startDateString
                                            ? DateOnly.Parse(startDateString).ToDateTime(TimeOnly.MinValue)
                                            : DateTime.MinValue,
                            EndTime = item.End.DateTimeOffset.HasValue
                                      ? item.End.DateTimeOffset.Value.DateTime
                                      : item.End.Date is string endDateString
                                          ? DateOnly.Parse(endDateString).ToDateTime(TimeOnly.MinValue)
                                          : DateTime.MinValue
                        }).ToList();
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to execute Google Calendar API request.");
                // Important: Throw the exception so the upstream GoogleCalendarService can handle 
                // the failure and prevent bad logging data.
                throw;
            }

            _logger.LogDebug($"External fetch completed: {fetchedEvents.Count} events returned.");
            return fetchedEvents;
        }
    }
}
