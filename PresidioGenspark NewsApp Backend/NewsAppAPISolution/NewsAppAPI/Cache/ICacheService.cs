    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    namespace NewsAppAPI.Cache
    {
        public interface ICacheService
        {
            /// <summary>
            /// Adds an item to the cache with a specific key and expiration time.
            /// </summary>
            /// <param name="key">The key for the cached item.</param>
            /// <param name="value">The value to cache.</param>
            /// <param name="expiration">The expiration time for the cached item.</param>
            Task SetAsync(string key, object value, TimeSpan expiration);

            /// <summary>
            /// Retrieves an item from the cache by its key.
            /// </summary>
            /// <param name="key">The key for the cached item.</param>
            /// <returns>The cached item or null if not found.</returns>
            Task<object> GetAsync(string key);

            /// <summary>
            /// Removes an item from the cache by its key.
            /// </summary>
            /// <param name="key">The key for the cached item.</param>
            Task RemoveAsync(string key);

            /// <summary>
            /// Checks if an item exists in the cache by its key.
            /// </summary>
            /// <param name="key">The key for the cached item.</param>
            /// <returns>True if the item exists; otherwise, false.</returns>
            Task<bool> ContainsAsync(string key);

            /// <summary>
            /// Clears all items from the cache.
            /// </summary>
            Task ClearAsync();
        }
    }
