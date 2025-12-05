using Microsoft.AspNetCore.Mvc;
using RandomAPI.Models;
namespace RandomAPI.Services.Webhooks
{
    public interface IWebhookService
    {
        // Basel class methods
        /// <summary>
        /// Sends a webhook notification to all registered listeners.
        /// </summary>
        Task BroadcastAsync<T>(T payload) where T : class;

        /// <summary>
        /// Registers a new webhook listener URL.
        /// </summary>
        /// <returns>True if added, false if it already existed.</returns>
        Task AddListenerAsync(string url, WebhookType type = default);

        /// <summary>
        /// Removes a webhook listener URL.
        /// </summary>
        /// <returns>True if removed, false if not found.</returns>
        Task<bool> RemoveListenerAsync(string url);

        /// <summary>
        /// Returns a snapshot of all registered listener URLs.
        /// </summary>
        Task<IEnumerable<string>> GetListenersAsync();

        /// <summary>
        /// returns a snapshot of all registered listenrs of a given type
        /// </summary>
        /// <param name="type"> the type of url</param>
        Task<IEnumerable<string>> GetListenersAsync(WebhookType type = WebhookType.Default);

        // Controller Logic Methods (Implemented in the derived class)
        public Task<IActionResult> HandleGetListenersActionAsync();
        public Task<IActionResult> HandleGetListenersOfTypeAsync(WebhookType type);
        public Task<IActionResult> HandleRegisterActionAsync(string url, WebhookType type = default);
        public Task<IActionResult> HandleUnregisterActionAsync(string url);
        public Task<IActionResult> HandleBroadcastActionAsync(IWebHookPayload payload);

        public enum WebhookType
        {
            Default = 0,
            Discord = 1,
            Logging = 2,
        }
    }
}

