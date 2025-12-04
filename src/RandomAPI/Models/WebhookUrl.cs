
namespace RandomAPI.Models
{
    /// <summary>
    /// Represents a registered webhook listener URL stored in the database.
    /// </summary>
    public class WebhookUrl
    {
        public int Id { get; set; }
        public required string Url { get; set; }
        public IWebhookService.WebhookType Type { get; set; }
    }
}
