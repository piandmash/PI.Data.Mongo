using PI.Data.Mongo.Interfaces;

namespace PI.Data.Mongo.Models
{
    public class CacheSettings : ICacheSettings
    {

        /// <summary>
        /// Constructor
        /// </summary>
        public CacheSettings()
        {

        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connectionString">The connection string</param>
        /// <param name="enabled">Optional enabled setting, defaults to false</param>
        public CacheSettings(string connectionString, bool enabled = false)
        {
            ConnectionString = connectionString;
            Enabled = enabled;
        }

        /// <summary>
        /// Gets/Sets the connection string
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Flag to set if caching is enabled
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets/Sets a key prefix for the cache
        /// </summary>
        public string KeyPrefix { get; set; }
    }
}
