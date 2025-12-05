using RandomAPI.Repository;

namespace RandomAPI.Services.Webhooks
{
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

        public async Task<IEnumerable<string>> GetListenersAsync(IWebhookService.WebhookType type = IWebhookService.WebhookType.Default)
        {
            var urls = await _repo.GetUrlsOfTypeAsync(type);
            return urls.Select(u => u.Url);
        }

        public async Task AddListenerAsync(string url, IWebhookService.WebhookType type = default)
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