using PI.Data.Mongo.Interfaces;

namespace PI.Data.Mongo.Models
{
    public class MongoRepositorySettings : IMongoRepositorySettings
    {

        /// <summary>
        /// Constructor
        /// </summary>
        public MongoRepositorySettings()
        {

        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connectionString">The connection string</param>
        /// <param name="databaseName">The database name</param>
        /// <param name="log">Optional log setting, defaults to false</param>
        /// <param name="azure">Optional flag to advise if using Azure, defaults to false</param>
        /// <param name="conventionsPackName">Optional name for the conventions pack, defaults to pi</param>
        public MongoRepositorySettings(string connectionString, 
            string databaseName, 
            bool log = false, 
            bool azure = false, 
            ICacheSettings cacheSettings = null,
            string conventionsPackName = "pi")
        {
            ConnectionString = connectionString;
            DatabaseName = databaseName;
            Log = log;
            Azure = azure;
            CacheSettings = cacheSettings;
            ConventionsPackName = conventionsPackName;
        }

        /// <summary>
        /// Gets/Sets the connection string
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Gets/sets the database name
        /// </summary>
        public string DatabaseName { get; set; }

        /// <summary>
        /// Flag to log, set to true to log
        /// </summary>
        public bool Log { get; set; }

        /// <summary>
        /// Flag to advise if the store is Azure
        /// </summary>
        public bool Azure { get; set; }

        /// <summary>
        /// The Cache Settings
        /// </summary>
        public ICacheSettings CacheSettings { get; set; }

        /// <summary>
        /// Gets/sets the conventions pack name
        /// </summary>
        public string ConventionsPackName { get; set; }
    }
}
