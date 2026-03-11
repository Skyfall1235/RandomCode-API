using Microsoft.AspNetCore.Mvc;

namespace RandomAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class APIHealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Ping()
        {
            return Ok();
        }
    }
}
