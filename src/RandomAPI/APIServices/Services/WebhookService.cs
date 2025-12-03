using System.Collections.Concurrent;

namespace RandomAPI.Services.Webhooks
{
    public class WebhookService : IWebhookService
    {
        private readonly ConcurrentDictionary<string, byte> _webhookUrls = new();
        private readonly HttpClient _client = new();
        private readonly ILogger<IWebhookService> _logger;

        public WebhookService(ILogger<IWebhookService> logger)
        {
            _logger = logger;
        }

        public IEnumerable<string> GetListeners() => _webhookUrls.Keys;

        public bool AddListener(string url) => _webhookUrls.TryAdd(url, 0);

        public bool RemoveListener(string url) => _webhookUrls.TryRemove(url, out _);

        public async Task BroadcastAsync<T>(T payload) where T : class
        {
            // Snapshot-safe enumeration
            var tasks = _webhookUrls.Keys.Select(async url =>
            {
                try
                {
                    await _client.PostAsJsonAsync(url, payload);
                }
                catch (Exception ex)
                {
                    {
                        _logger.LogWarning(ex, "WebhookPayload failed to Post");
                    }
                }
            });
            await Task.WhenAll(tasks);
        }
    }
}
