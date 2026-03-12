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
        Task<IEnumerable<WebhookUrl>> GetUrlsOfSourceAsync(string source);
        Task AddUrlAsync(string url, string source);
        Task<int> DeleteUrlAsync(string url);
    }
}
