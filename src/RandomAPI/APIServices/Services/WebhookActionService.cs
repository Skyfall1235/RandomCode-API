using Microsoft.AspNetCore.Mvc;
using RandomAPI.Models;
using RandomAPI.Repository;
using static IWebhookService;

namespace RandomAPI.Services.Webhooks
{
    public class WebhookActionService : BaseWebhookService, IWebhookService
    {
        public WebhookActionService(IWebhookRepository repo, ILogger<IWebhookService> logger)
            : base(repo, logger) {  }

        public async Task<IActionResult> HandleGetListenersActionAsync()
        {
            var urls = await base.GetListenersAsync();
            return new OkObjectResult(urls);
        }

        public async Task<IActionResult> HandleGetListenersOfTypeAsync(WebhookType type)
        {
            var urls = await base.GetListenersAsync(type);
            return new OkObjectResult(urls);
        }

        public async Task<IActionResult> HandleRegisterActionAsync([FromBody] string url, IWebhookService.WebhookType type = default)
        {
            if (string.IsNullOrWhiteSpace(url))
                return new BadRequestObjectResult("URL cannot be empty.");
            //neede both on regisdter and deregister
            string safeUrlForLog = SanitizeURL(ref url);

            await base.AddListenerAsync(url, type);

            _logger.LogInformation("Registered new webhook listener: {Url}", safeUrlForLog);

            return new OkObjectResult(new { Message = $"Listener added successfully: {url}" });
        }

        public async Task<IActionResult> HandleUnregisterActionAsync([FromBody] string url)
        {
            string safeUrlForLog = url;
            if (string.IsNullOrWhiteSpace(url))
            {
                
                return new BadRequestObjectResult("URL cannot be empty.");
            }

            safeUrlForLog = SanitizeURL(ref url);
            var removed = await base.RemoveListenerAsync(url);
            if (!removed)
            {
                return new NotFoundObjectResult(new { Message = $"URL not found: {url}" });
            }

            _logger.LogInformation("Unregistered webhook listener: {Url}", safeUrlForLog);
            return new OkObjectResult(new { Message = $"Listener removed: {url}" });
        }

        public async Task<IActionResult> HandleBroadcastActionAsync([FromBody] IWebHookPayload payload)
        {
            var listeners = await base.GetListenersAsync();

            if (!listeners.Any())
                return new BadRequestObjectResult("No listeners registered to broadcast to.");

            switch (payload)
            {
                case WebhookPayload p:
                    p.Timestamp = DateTime.UtcNow;

                    break;

                case DiscordWebhookPayload p:
                    break;

                default:
                    _logger.LogWarning("Received unsupported payload type: {Type}", payload.GetType().Name);
                    return new BadRequestObjectResult(new { Message = "Unsupported webhook payload type." });
            }

            _logger.LogInformation("Broadcasting test payload: {Message}", payload.content);
            await base.BroadcastAsync(payload);
            return new OkObjectResult(new
            {
                Message = $"Broadcast sent for message: '{payload.content}'. Check logs for delivery status."
            });
        }

        private static string SanitizeURL(ref string url)
        {
            url = url.Trim();
            string? safeUrlForLog = url.Replace("\r", "").Replace("\n", "");
            return safeUrlForLog;
        }
    }
}