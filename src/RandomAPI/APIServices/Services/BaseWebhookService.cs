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
            url = url.Trim();
            var safeUrlForLog = url.Replace("\r", "").Replace("\n", "");

            await base.AddListenerAsync(url, type);

            _logger.LogInformation("Registered new webhook listener: {Url}", safeUrlForLog);

            return new OkObjectResult(new { Message = $"Listener added successfully: {url}" });
        }

        public async Task<IActionResult> HandleUnregisterActionAsync([FromBody] string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
            var safeUrlForLog = url.Replace("\r", "").Replace("\n", "");
                return new BadRequestObjectResult("URL cannot be empty.");
            }
            url = url.Trim();

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
    }


    public class BaseWebhookService
    {
        protected readonly IWebhookRepository _repo;
        protected readonly HttpClient _client = new();
        protected readonly ILogger<IWebhookService> _logger;

        public BaseWebhookService(IWebhookRepository repo, ILogger<IWebhookService> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<IEnumerable<string>> GetListenersAsync()
        {
            var urls = await _repo.GetAllUrlsAsync();
            return urls.Select(u => u.Url);
        }

        public async Task<IEnumerable<string>> GetListenersAsync(WebhookType type = WebhookType.Default)
        {
            var urls = await _repo.GetUrlsOfTypeAsync(type);
            return urls.Select(u => u.Url);
        }

        public async Task AddListenerAsync(string url, WebhookType type = default)
        {
            await _repo.AddUrlAsync(url, type);
        }

        public async Task<bool> RemoveListenerAsync(string url)
        {
            var result = await _repo.DeleteUrlAsync(url);
            return result > 0;
        }

        //basic broadcast for all
        public async Task BroadcastAsync<T>(T payload) where T : class
        {
            IEnumerable<string> urls = await GetListenersAsync();
            await BroadcastAsync(payload, urls);
        }
        //derived for the payloads
        public async Task BroadcastAsync<T>(T payload, IEnumerable<string> urls) where T : class
        {
            var tasks = urls.Select(async url =>
            {
                try
                {
                    await _client.PostAsJsonAsync(url, payload);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Webhook POST failed for URL: {url}", url);
                }
            });
            await Task.WhenAll(tasks);
        }
    }
}