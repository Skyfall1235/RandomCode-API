using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using RandomAPI.Models;

[ApiController]
[Route("api/[controller]")]
[ApiKey]
[EnableRateLimiting("ParkingApiPolicy")]
public class LastParkedLocationController : ControllerBase
{
    ILastParkedLocationService _service;

    public LastParkedLocationController(ILastParkedLocationService service)
    {
        this._service = service;
    }


    [HttpGet("get-current/{vehicleId}")]
    public async Task<IActionResult> GetCurrentLocation(string vehicleId) 
    {
        LastParkedLocation result = await _service.GetCurrentParkedLocation(vehicleId);
        Console.WriteLine(result);

        if (result == null) return NotFound();
        return Ok(result);
    }

    [ApiKey]
    [HttpGet("get-past/{vehicleId}")]
    public async Task<IActionResult> GetVehiclePast(string vehicleId)
    {
        IEnumerable<LastParkedLocation> result = await _service.GetVehiclePast(vehicleId);

        if (result == null) return NotFound();
        return Ok(result);
    }

    // POST: api/LastParkedLocation
    // Uses [FromForm] because you are likely sending an image file + JSON/fields
    [HttpPost]
    public async Task<IActionResult> PostNewLocation([FromForm] ParkingSubmission submission)
    {
        LastParkedLocation result = await _service.PostCurrentParkedLocation(submission);

        if (result == null)
        {
            return BadRequest("Could not create the parking location. Please check your submission data.");
        }

        return Ok(result);
    }

    [HttpGet("get-current-image/{vehicleId}")]
    public async Task<IActionResult> GetvehiclePhoto(string vehicleId)
    {
        LastParkedLocation result = await _service.GetCurrentParkedLocation(vehicleId);

        // 1. Check if the record exists and has a photo path
        if (result == null || string.IsNullOrEmpty(result.PhotoPath))
        {
            return NotFound("No photo found for this vehicle.");
        }

        // 2. Check if the file actually exists on the disk
        if (!System.IO.File.Exists(result.PhotoPath))
        {
            return NotFound("The image file is missing from the server.");
        }

        // 3. Return the file
        // You can use 'image/jpeg' or a helper to determine the content type
        return PhysicalFile(result.PhotoPath, "image/jpeg");
    }
}

