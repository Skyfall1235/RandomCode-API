using Microsoft.AspNetCore.Mvc;

namespace RandomAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class APIHealthController : ControllerBase
    {
        [HttpPost("calculate")]
        public IActionResult Health()
        {
            return Ok();
        }
    }
}
