using Microsoft.AspNetCore.Mvc;
using RandomAPI.Models;
using RandomAPI.Services.Webhooks;

namespace RandomAPI.Services.Webhooks
{
    public class WebhookPayload : IWebHookPayload
    {
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string Content { get; set; } = "";
    }
}