using Microsoft.AspNetCore.Mvc;

namespace RandomAPI.Models
{
    public interface IWebHookPayload
    {
        DateTime Timestamp { get; set; }
        string Content { get; set; }
    }
}