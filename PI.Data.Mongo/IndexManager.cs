using MongoDB.Bson;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace PI.Data.Mongo
{
    /// <summary>
    /// Static class to facillitate indexing
    /// </summary>
    /// <remarks>Error codes follow the format DAL-IM-XXX</remarks>
    public static class IndexManager
    {

        /// <summary>
        /// Gets a matching contest from the data store
        /// </summary>
        /// <remarks>Error codes follow the format DAL-IM-1XX</remarks>
        /// <param name="database">The database to build the index on</param>
        /// <param name="collectionName">The collection to index</param>
        /// <param name="indexDoc">The index document</param>
        /// <param name="indexName">The name of the index</param>
        public static async Task<string> Index(IMongoDatabase database, string collectionName, BsonDocument indexDoc, string indexName)
        {
            var collection = database.GetCollection<BsonDocument>(collectionName);
            return await collection.Indexes.CreateOneAsync(indexDoc, new CreateIndexOptions() { Name  = indexName, Background = true });
        }

    }
}
