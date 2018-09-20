using MongoDB.Driver;
//using StackExchange.Redis.Extensions.Core;

namespace PI.Data.Mongo.Interfaces
{
    /// <summary>
    /// IMongoDbRepository
    /// </summary>
    public interface IMongoDbRepository
    {
        /// <summary>
        /// Getter for the read only IMongoClient
        /// </summary>
        IMongoClient Client { get; }

        /// <summary>
        /// Getter for the read only IMongoDatabase
        /// </summary>
        IMongoDatabase Database { get; }
        
        /// <summary>
        /// Gets if the repository is an Azure repository
        /// </summary>
        bool Azure { get; }

        /// <summary>
        /// Gets the cache settings
        /// </summary>
        ICacheSettings CacheSettings { get; }

        ///// <summary>
        ///// Gets the cache client
        ///// </summary>
        //ICacheClient Cache { get; }
    }
}
