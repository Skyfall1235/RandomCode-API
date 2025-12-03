namespace RandomAPI.Services.Webhooks
{
    public interface ICustomWebhookPayload : IWebHookPayload
    {
        DateTime Timestamp { get; set; }
    }

    public interface IWebHookPayload
    {
        string content { get; set; }
    }
}
