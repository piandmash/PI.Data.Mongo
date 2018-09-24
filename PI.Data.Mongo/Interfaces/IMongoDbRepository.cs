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
        /// Gets if the repository has indexing disabled
        /// </summary>
        bool IndexingDisabled { get; }

    }
}
