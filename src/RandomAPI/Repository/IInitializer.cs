namespace RandomAPI.Repository
{
    /// <summary>
    /// Defines a contract for services (like repositories) that require an asynchronous
    /// initialization step (e.g., ensuring a database table exists).
    /// </summary>
    public interface IInitializer
    {
        Task InitializeAsync();
    }
}
