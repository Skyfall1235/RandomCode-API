using RandomAPI.Models;
using RandomAPI.Services.Webhooks;

namespace RandomAPI.Services.Webhooks
{
    public class WebhookPayload : ICustomWebhookPayload
    {
        public string content { get; set; } = "";
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public class DiscordWebhookPayload : IWebHookPayload
    {
        public string content { get; set; } = "";
    }
}