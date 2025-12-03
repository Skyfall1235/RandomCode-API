using Microsoft.AspNetCore.Mvc;
using RandomAPI.Services.Webhooks;

namespace RandomAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WebhookController : ControllerBase
    {
        private readonly ILogger<WebhookController> _logger;
        private readonly IWebhookService _webhookService;

        public WebhookController(ILogger<WebhookController> logger, IWebhookService webhookService)
        {
            _logger = logger;
            _webhookService = webhookService;
        }

        /// <summary>
        /// Gets a list of all currently registered webhook listener URLs.
        /// </summary>
        [HttpGet("listeners")]
        public IActionResult GetListeners()
        {
            return Ok(_webhookService.GetListeners());
        }

        /// <summary>
        /// Registers a new URL to receive webhook payloads.
        /// </summary>
        /// <param name="url">The URL to register.</param>
        [HttpPost("register")]
        public IActionResult Register([FromBody] string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return BadRequest("URL cannot be empty.");
            }

            if (_webhookService.AddListener(url))
            {
                _logger.LogInformation("Registered new webhook listener: {Url}", url);
                return Ok(new { Message = $"Listener added successfully for {url}" });
            }

            return Conflict(new { Message = $"URL is already registered: {url}" });
        }

        /// <summary>
        /// Removes a URL from the list of webhook listeners.
        /// </summary>
        /// <param name="url">The URL to unregister.</param>
        [HttpDelete("unregister")]
        public IActionResult Unregister([FromBody] string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return BadRequest("URL cannot be empty.");
            }

            if (_webhookService.RemoveListener(url))
            {
                _logger.LogInformation("Unregistered webhook listener: {Url}", url);
                return Ok(new { Message = $"Listener removed successfully for {url}" });
            }

            return NotFound(new { Message = $"URL not found in listener list: {url}" });
        }

        /// <summary>
        /// Endpoint to manually trigger a test broadcast of a payload.
        /// </summary>
        /// <param name="payload">The payload message to send.</param>
        [HttpPost("debug/broadcast-test")]
        public async Task<IActionResult> BroadcastTest([FromBody] WebhookPayload payload)
        {
            if (!_webhookService.GetListeners().Any())
            {
                return BadRequest("No listeners registered to broadcast to.");
            }

            // Ensure the payload has a fresh timestamp
            payload.Timestamp = DateTime.UtcNow;

            _logger.LogInformation("Broadcasting test payload: {Message}", payload.content);

            // This runs asynchronously in the background. We don't wait for every success.
            await _webhookService.BroadcastAsync(payload);

            return Ok(
                new
                {
                    Message = $"Broadcast initiated successfully for message: '{payload.content}'. Check logs for delivery status.",
                }
            );
        }

        [HttpPost("debug/discord-broadcast-test")]
        public async Task<IActionResult> BroadcastDiscordTest(
            [FromBody] DiscordWebhookPayload payload
        )
        {
            if (!_webhookService.GetListeners().Any())
            {
                return BadRequest("No listeners registered to broadcast to.");
            }

            // Ensure the payload has a fresh timestamp
            //payload.Timestamp = DateTime.UtcNow;

            _logger.LogInformation("Broadcasting test payload: {Message}", payload.content);

            // This runs asynchronously in the background. We don't wait for every success.
            await _webhookService.BroadcastAsync(payload);

            return Ok(
                new
                {
                    Message = $"Broadcast initiated successfully for message: '{payload.content}'. Check logs for delivery status.",
                }
            );
        }
    }
}
