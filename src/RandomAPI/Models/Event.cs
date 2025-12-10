namespace RandomAPI.Models
{
    public class Event
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string? EventId { get; set; } // The 8-char hash
        public string? Service { get; set; }
        public string? Type { get; set; }
        public string? DataType { get; set; }
        public string? JsonData { get; set; }

        public Event() { }

        public Event(
            int id,
            DateTime timestamp,
            string? service,
            string? type,
            string? dataType,
            string? jsonData
        )
        {
            Id = id;
            Timestamp = timestamp;
            EventId = HashUtils.GenerateShortHash(jsonData ?? "");
            Service = service;
            Type = type;
            DataType = dataType;
            JsonData = jsonData;
        }

        /// <summary>
        /// Creates a new event, auto-generating Timestamp and Content-based EventId.
        /// </summary>
        public Event(string service, string type, string jsonData, string dataType = "JSON")
        {
            Service = service;
            Type = type;
            DataType = dataType;
            JsonData = jsonData;
            Timestamp = DateTime.UtcNow;
            EventId = HashUtils.GenerateShortHash(jsonData ?? "");
            Id = -1; // Placeholder
        }
    }
}
