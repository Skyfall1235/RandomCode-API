using Microsoft.AspNetCore.Mvc;
using RandomAPI.Models;

public class LastParkedLocationService : ILastParkedLocationService
{
    private readonly string _imagePath;
    protected readonly ILastParkedCarRepository _repo;
    protected readonly ILogger<ILastParkedCarRepository> _logger;
    public LastParkedLocationService(ILastParkedCarRepository repo, ILogger<ILastParkedCarRepository> logger, IWebHostEnvironment env)
    {
        _repo = repo;
        _logger = logger;
        _imagePath = PathUtils.CreateOrReturnWbRootPath(env, "parking-photos");
    }

    public async Task<LastParkedLocation> GetCurrentParkedLocation(string vehicleId)
    {
        //retrive 
        LastParkedLocation? location = await _repo.GetActiveLocationAsync(vehicleId);
#pragma warning disable CS8603 // Possible null reference return.
        return location;//return null if missing as if its null we should handle it in the controller
#pragma warning restore CS8603 // Possible null reference return.
    }

    public async Task<IEnumerable<LastParkedLocation>> GetVehiclePast(string vehicleId)
    {
        IEnumerable<LastParkedLocation> history = await _repo.GetLocationHistoryAsync(vehicleId);

        return history;
    }

    public async Task<LastParkedLocation> PostCurrentParkedLocation([FromForm] ParkingSubmission submission)
    {
        LastParkedLocation parkedLocation = new(submission);

        string filePath;
        if (submission.Photo is { Length: > 0 })
        {
            var fileName = $"{Guid.NewGuid()}_{submission.Photo.FileName}";
            filePath = Path.Combine(_imagePath, fileName);

            using var stream = File.Create(filePath);
            await submission.Photo.CopyToAsync(stream);

            parkedLocation.PhotoPath = filePath;
        }

        await _repo.DeactivateOldLocationsAsync(submission.VehicleId);

        await _repo.SaveLocationAsync(parkedLocation);

        return parkedLocation;
    }

}

