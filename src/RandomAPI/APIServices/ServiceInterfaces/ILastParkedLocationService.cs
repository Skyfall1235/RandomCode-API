using Microsoft.AspNetCore.Mvc;
using RandomAPI.Models;

public interface ILastParkedLocationService
{
    Task<LastParkedLocation> GetCurrentParkedLocation(string vehicleId);
    Task<IEnumerable<LastParkedLocation>> GetVehiclePast(string vehicleId);
    Task<LastParkedLocation> PostCurrentParkedLocation([FromForm] ParkingSubmission submission);
}