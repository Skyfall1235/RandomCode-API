public interface IWebhookService
{
    /// <summary>
    /// Sends a webhook notification to all registered listeners.
    /// </summary>
    Task BroadcastAsync<T>(T payload) where T : class;

    /// <summary>
    /// Registers a new webhook listener URL.
    /// </summary>
    /// <returns>True if added, false if it already existed.</returns>
    bool AddListener(string url);

    /// <summary>
    /// Removes a webhook listener URL.
    /// </summary>
    /// <returns>True if removed, false if not found.</returns>
    bool RemoveListener(string url);

    /// <summary>
    /// Returns a snapshot of all registered listener URLs.
    /// </summary>
    IEnumerable<string> GetListeners();
}
