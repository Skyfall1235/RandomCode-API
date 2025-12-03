using RandomAPI.Models;

namespace RandomAPI.Repository
{
    /// <summary>
    /// Defines the contract for persistence operations related to WebhookUrl models.
    /// </summary>
    public interface IWebhookRepository
    {
        Task<IEnumerable<WebhookUrl>> GetAllUrlsAsync();
        Task AddUrlAsync(string url);
        Task<int> DeleteUrlAsync(string url);
    }
}
