using RandomAPI.Models;

public interface ILastParkedCarRepository
{
    // 1. Get the current active location for a specific vehicle
    Task<LastParkedLocation?> GetActiveLocationAsync(string vehicleId);

    // 2. Get the history of where a vehicle has been (all IsActive = 0)
    Task<IEnumerable<LastParkedLocation>> GetLocationHistoryAsync(string vehicleId);

    // 3. Save a new location
    // Idea: This method should probably call a "DeactivateOldLocations" SQL command 
    // first so only one spot is active at a time for that VehicleId.
    Task SaveLocationAsync(LastParkedLocation location);

    // 4. A helper to flip IsActive to 0 for a specific vehicle
    Task DeactivateOldLocationsAsync(string vehicleId);
}

