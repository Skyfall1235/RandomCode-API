using RandomAPI.Models;
using RandomAPI.Services.Webhooks;

namespace RandomAPI.Repository
{
    /// <summary>
    /// Defines the contract for persistence operations related to WebhookUrl models.
    /// </summary>
    public interface IWebhookRepository
    {
        Task<IEnumerable<WebhookUrl>> GetAllUrlsAsync();
        Task<IEnumerable<WebhookUrl>> GetUrlsOfTypeAsync(IWebhookService.WebhookType type);
        Task AddUrlAsync(string url, IWebhookService.WebhookType type);
        Task<int> DeleteUrlAsync(string url);
    }
}
