using RandomAPI.Models;

namespace RandomAPI.Repository
{
    /// <summary>
    /// Defines the contract for persistence operations related to the Event model.
    /// </summary>
    public interface IEventRepository
    {
        /// <summary>
        /// Attempts to add a new Event record to the database.
        /// Throws an exception if the EventId (based on Content hash) already exists.
        /// </summary>
        /// <param name="eventModel">The new Event object to insert.</param>
        /// <returns>The ID (surrogate key) of the newly inserted record.</returns>
        Task<int> AddEventAsync(Event eventModel);

        /// <summary>
        /// Retrieves all events from the database, ordered by Timestamp descending.
        /// </summary>
        /// <returns>A list of all Event objects.</returns>
        Task<IEnumerable<Event>> GetAllEventsAsync();

        /// <summary>
        /// Retrieves a specific number of the most recent events from the database.
        /// </summary>
        /// <param name="count">The maximum number of recent events to retrieve.</param>
        /// <returns>A list of recent Event objects.</returns>
        Task<IEnumerable<Event>> GetRangeOfRecentEventsAsync(int count);

        /// <summary>
        /// Retrieves a list of events matching the provided list of surrogate IDs.
        /// </summary>
        /// <param name="ids">A collection of event surrogate keys (Ids) to retrieve.</param>
        /// <returns>A list of Event objects matching the provided IDs.</returns>
        Task<IEnumerable<Event>> GetEventsByIdsAsync(IEnumerable<int> ids);

        /// <summary>
        /// Removes a single event record from the database by its surrogate Id.
        /// </summary>
        /// <param name="id">The surrogate key (Id) of the event to remove.</param>
        /// <returns>The number of rows deleted (0 or 1).</returns>
        Task<int> RemoveEventAsync(int id);

        /// <summary>
        /// Removes multiple event records from the database by their surrogate Ids.
        /// </summary>
        /// <param name="ids">A collection of event surrogate keys (Ids) to remove.</param>
        /// <returns>The total number of rows deleted.</returns>
        Task<int> RemoveEventsByIdsAsync(IEnumerable<int> ids);
    }
}
