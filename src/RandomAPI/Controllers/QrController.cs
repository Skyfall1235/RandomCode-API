using Microsoft.AspNetCore.Mvc;

namespace RandomAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QrController : ControllerBase
    {
        IQrService _qrService;
        

        public QrController(IQrService qrService)
        {
            _qrService = qrService;
        }

        [HttpGet("generate-setup")]
        [ApiKey] 
        public IActionResult GetSetupQr()
        {
            var config = HttpContext.RequestServices.GetRequiredService<IConfiguration>();
            var key = config.GetValue<string>("Authentication:ApiKey");

            var relativeUrl = _qrService.GenerateAuthQR();
            return PhysicalFile(relativeUrl, "image/jpeg");
        }
    }
}