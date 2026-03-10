using Microsoft.AspNetCore.Mvc;

namespace RandomAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ResumeController : ControllerBase
    {
        private readonly string _resumePath;

        public ResumeController(IWebHostEnvironment env)
        {
            _resumePath = Path.Combine(env.WebRootPath, "resumes");

            if (!Directory.Exists(_resumePath))
                Directory.CreateDirectory(_resumePath);
        }

        [HttpGet("latest")]
        public IActionResult GetLatestResume()
        {
            var directory = new DirectoryInfo(_resumePath);
            var latestFile = directory.GetFiles("*.pdf")
                .OrderByDescending(f => f.LastWriteTime)
                .FirstOrDefault();

            if (latestFile == null)
                return NotFound("No resume versions found.");

            return PhysicalFile(latestFile.FullName, "application/pdf", "Wyatt_Murray_Resume.pdf");
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadResume(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            // Format: resume_2026-03-10_1430.pdf
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HHmm");
            var fileName = $"resume_{timestamp}{Path.GetExtension(file.FileName)}";
            var fullPath = Path.Combine(_resumePath, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Ok(new { Message = "New version archived.", FileName = fileName });
        }
    }
}
