using Microsoft.AspNetCore.Mvc;

namespace RandomAPI.Models
{
    public class WebhookPayload : IWebHookPayload
    {
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string Content { get; set; } = "";
    }
}