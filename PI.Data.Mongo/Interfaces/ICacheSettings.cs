namespace PI.Data.Mongo.Interfaces
{
    public interface ICacheSettings
    {
        /// <summary>
        /// Gets/Sets the connection string
        /// </summary>
        string ConnectionString { get; set; }

        /// <summary>
        /// Flag to set if caching is enabled
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        /// Gets/Sets a key prefix for the cache
        /// </summary>
        string KeyPrefix { get; set; }

    }
}
