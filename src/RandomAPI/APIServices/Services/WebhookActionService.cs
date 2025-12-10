using Microsoft.AspNetCore.Mvc;
using RandomAPI.Models;
using RandomAPI.Repository;

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

        public async Task<IActionResult> HandleGetListenersOfSourceAsync(string source)
        {
            var urls = await base.GetListenersAsync(source);
            return new OkObjectResult(urls);
        }

        public async Task<IActionResult> HandleRegisterActionAsync([FromBody] string url, string source)
        {
            if (string.IsNullOrWhiteSpace(url))
                return new BadRequestObjectResult("URL cannot be empty.");
            //needed both on regisdter and deregister
            string safeUrlForLog = SanitizeURL(ref url);

            await base.AddListenerAsync(url, source);

            _logger.LogInformation("Registered new webhook listener: {Url}", safeUrlForLog);

            return new OkObjectResult(new { Message = $"Listener added successfully: {url}" });
        }

        public async Task<IActionResult> HandleUnregisterActionAsync([FromBody] string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                
                return new BadRequestObjectResult("URL cannot be empty.");
            }

            string safeUrlForLog = SanitizeURL(ref url);
            var removed = await base.RemoveListenerAsync(url);
            if (!removed)
            {
                return new NotFoundObjectResult(new { Message = $"URL not found: {url}" });
            }

            _logger.LogInformation("Unregistered webhook listener: {Url}", safeUrlForLog);
            return new OkObjectResult(new { Message = $"Listener removed: {url}" });
        }

        public async Task<IActionResult> HandleBroadcastActionAsync([FromBody] IWebHookPayload payload, string source)
        {
            var listeners = await GetListenersAsync(source);

            if (!listeners.Any())
                return new BadRequestObjectResult("No listeners registered to broadcast to.");
            payload.Timestamp = DateTime.UtcNow;


            _logger.LogInformation("Broadcasting test payload: {Message}", payload.Content);
            await BroadcastAsync(payload);
            return new OkObjectResult(new
            {
                Message = $"Broadcast sent for message: '{payload.Content}'. Check logs for delivery status."
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