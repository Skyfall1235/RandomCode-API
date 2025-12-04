using Microsoft.AspNetCore.Mvc;
using RandomAPI.Services.Webhooks;
using static IWebhookService;

namespace RandomAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WebhookController : ControllerBase
    {
        private readonly ILogger<WebhookController> _logger;
        private readonly IWebhookService _webhookService;

        public WebhookController(
            ILogger<WebhookController> logger,
            IWebhookService webhookService)
        {
            _logger = logger;
            _webhookService = webhookService;
        }

        /// <summary>
        /// Gets a list of all currently registered webhook listener URLs.
        /// </summary>
        [HttpGet("listeners")]
        public async Task<IActionResult> GetListeners()
        {
            var urls = await _webhookService.GetListenersAsync();
            return Ok(urls);
        }

        /// <summary>
        /// Registers a new URL to receive webhook payloads.
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> RegisterUrl([FromBody] string url)
        {
            return await _webhookService.HandleRegisterActionAsync(url);
        }
        /// <summary>
        /// Registers a new URL to receive webhook payloads.
        /// </summary>
        [HttpPost("register-discord")]
        public async Task<IActionResult> RegisterDiscordUrl([FromBody] string url)
        {
            return await _webhookService.HandleRegisterActionAsync(url, WebhookType.Discord);
        }

        /// <summary>
        /// Removes a URL from the list of webhook listeners.
        /// </summary>
        [HttpDelete("unregister")]
        public async Task<IActionResult> UnregisterUrl([FromBody] string url)
        {
            return await _webhookService.HandleUnregisterActionAsync(url);
        }

        /// <summary>
        /// Endpoint to manually trigger a test broadcast.
        /// </summary>
        [HttpPost("debug/broadcast-test")]
        public async Task<IActionResult> BroadcastTest([FromBody] WebhookPayload payload)
        {
            var listeners = await _webhookService.GetListenersAsync();

            if (!listeners.Any())
                return BadRequest("No listeners registered to broadcast to.");

            payload.Timestamp = DateTime.UtcNow;

            _logger.LogInformation("Broadcasting test payload: {Message}", payload.content);

            await _webhookService.BroadcastAsync(payload);

            return Ok(new
            {
                Message = $"Broadcast sent for message: '{payload.content}'. Check logs for delivery status."
            });
        }

        [HttpPost("debug/discord-broadcast-test")]
        public async Task<IActionResult> BroadcastDiscordTest([FromBody] DiscordWebhookPayload payload)
        {
            var listeners = await _webhookService.GetListenersAsync();

            if (!listeners.Any())
                return BadRequest("No listeners registered to broadcast to.");

            _logger.LogInformation("Broadcasting Discord payload: {Message}", payload.content?.Replace("\r", "").Replace("\n", ""));

            await _webhookService.BroadcastAsync(payload);

            return Ok(new
            {
                Message = $"Broadcast sent for message: '{payload.content}'. Check logs for delivery status."
            });
        }
    }
}
