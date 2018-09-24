using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
//using StackExchange.Redis.Extensions.Core;
//using StackExchange.Redis.Extensions.Newtonsoft;
using MongoDB.Bson;
using PI.Data.Mongo.Models;
using PI.Data.Mongo.Interfaces;

namespace PI.Data.Mongo
{
    public class MongoDbRepository : IMongoDbRepository
    {
        /// <summary>
        /// Constructor for the class
        /// </summary>
        /// <param name="connectionString">The connection string to the data store</param>
        /// <param name="databaseName">The database name to target</param>
        public MongoDbRepository(IMongoRepositorySettings settings)
        {
            _IndexingDisabled = settings.IndexingDisabled;
            var clientSettings = MongoClientSettings.FromUrl(new MongoUrl(settings.ConnectionString));
            if (settings.Log)
            {
                clientSettings.ClusterConfigurator = cb =>
                {
                    cb.Subscribe<MongoDB.Driver.Core.Events.CommandStartedEvent>(e =>
                    {
                        System.Diagnostics.Debug.WriteLine($"{e.CommandName} - {e.Command.ToJson()}");
                    });
                };
            }
            _Client = new MongoClient(clientSettings);
            if (_Client != null) _Database = _Client.GetDatabase(settings.DatabaseName);
            //register conventions
            RegisteringConventionsPack(settings.ConventionsPackName);
        }

        /// <summary>
        /// Creates the standard convention pack setup for the repository, override to create and manage your own pack settings
        /// </summary>
        /// <param name="conventionsPackName">The name of the convention pack</param>
        protected virtual void RegisteringConventionsPack(string conventionsPackName)
        {
            //register convention pack
            var pack = new ConventionPack();
            pack.Add(new CamelCaseElementNameConvention());
            pack.Add(new IgnoreExtraElementsConvention(true));
            pack.Add(new StringObjectIdIdGeneratorConvention());
            ConventionRegistry.Register(conventionsPackName, pack, t => true);
        }

        private IMongoClient _Client;
        /// <summary>
        /// Getter for the read only IMongoClient
        /// </summary>
        public IMongoClient Client
        {
            get { return _Client; }
        }

        private IMongoDatabase _Database;
        /// <summary>
        /// Getter for the read only IMongoDatabase
        /// </summary>
        public IMongoDatabase Database
        {
            get { return _Database; }
        }

        private bool _IndexingDisabled = false;
        /// <summary>
        /// Gets if the repository has indexing disabled
        /// </summary>
        public bool IndexingDisabled
        {
            get { return _IndexingDisabled; }
        }

    }
}
