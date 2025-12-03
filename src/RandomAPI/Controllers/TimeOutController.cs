using Microsoft.AspNetCore.Mvc;
using RandomAPI.DTOs;

namespace RandomAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HoursController : ControllerBase
    {
        private readonly IHoursService _hoursService;
        private readonly ILogger<HoursController> _logger;

        public HoursController(IHoursService hoursService, ILogger<HoursController> logger)
        {
            _hoursService = hoursService;
            _logger = logger;
        }

        [HttpPost("calculate")]
        public ActionResult<HoursResponseDto> Calculate(HoursRequestDto dto)
        {
            try
            {
                var result = _hoursService.Calculate(dto);
                return Ok(result);
            }
            catch (FormatException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
