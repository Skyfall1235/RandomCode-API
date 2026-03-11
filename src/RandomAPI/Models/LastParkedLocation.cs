using System.Text.Json.Serialization;

namespace RandomAPI.Models
{
    public class LastParkedLocation
    {
        // key info
        public DateTime ParkedAt { get; set; }
        public string? VehicleId { get; set; }

        // location stuff
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public float? Accuracy { get; set; }
        public string? HumanReadableAddress { get; set; }

        // various other things
        public string? Notes { get; set; }
        public int? FloorLevel { get; set; }
        public string? Section { get; set; }

        // media and management
        [JsonIgnore]
        public string? PhotoPath { get; set; }
        public bool IsActive { get; set; } // moreso for a db than for the model

        public string GoogleMapsUrl => $"https://www.google.com/maps/search/?api=1&query={Latitude},{Longitude}";

        public LastParkedLocation(ParkingSubmission submission)
        {
            ParkedAt = DateTime.UtcNow;
            VehicleId = submission.VehicleId;
            Latitude = submission.Latitude;
            Longitude = submission.Longitude;
            Accuracy = submission.Accuracy;
            HumanReadableAddress = submission.HumanReadableAddress;
            Notes = submission.Notes;
            FloorLevel = submission.FloorLevel;
            Section = submission.Section;
            //leave photo to be fileld in later
            IsActive = true;//defgault to true as this is likely a new solution
        }

        public LastParkedLocation() { }
    }

    public class ParkingSubmission
    {
        // Identifies which car we are parking
        public string? VehicleId { get; set; }

        // Required GPS data
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        // Optional GPS metadata
        public float? Accuracy { get; set; }
        public string? HumanReadableAddress { get; set; }

        // The physical image file from the request
        public IFormFile? Photo { get; set; }

        // Detail fields
        public int? FloorLevel { get; set; }
        public string? Section { get; set; }
        public string? Notes { get; set; }
    }
}

