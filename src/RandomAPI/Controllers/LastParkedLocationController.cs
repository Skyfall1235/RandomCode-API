using Microsoft.AspNetCore.Mvc;
using RandomAPI.Models;

[ApiController]
[Route("api/[controller]")]
public class LastParkedLocationController : ControllerBase
{
    LastParkedLocationService service;//REPLACE WITH INTERFACE

    [HttpGet("ParkedLocation/get-current/{vehicleId}")]
    public async Task<IActionResult> GetCurrentLocation(string vehicleId) 
    {
        LastParkedLocation result = await service.GetCurrentParkedLocation(vehicleId);

        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet("ParkedLocation/get-past/{vehicleId}")]
    public async Task<IActionResult> GetVehiclePast(string vehicleId)
    {
        IEnumerable<LastParkedLocation> result = await service.GetVehiclePast(vehicleId);

        if (result == null) return NotFound();
        return Ok(result);
    }

    // POST: api/LastParkedLocation
    // Uses [FromForm] because you are likely sending an image file + JSON/fields
    [HttpPost]
    public async Task<IActionResult> PostNewLocation([FromForm] ParkingSubmission submission)
    {
        LastParkedLocation result = await service.PostCurrentParkedLocation(submission);

        if (result == null)
        {
            return BadRequest("Could not create the parking location. Please check your submission data.");
        }

        return Ok(result);
    }

    [HttpGet("ParkedLocation/get-current-imnage/{vehicleId}")]
    public async Task<IActionResult> GetvehiclePhoto(string vehicleId)
    {
        LastParkedLocation result = await service.GetCurrentParkedLocation(vehicleId);

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

