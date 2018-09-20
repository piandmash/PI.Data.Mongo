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
            _Azure = settings.Azure;
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
            //register convention pack
            var pack = new ConventionPack();
            pack.Add(new CamelCaseElementNameConvention());
            pack.Add(new IgnoreExtraElementsConvention(true));
            pack.Add(new StringObjectIdIdGeneratorConvention());
            ConventionRegistry.Register(settings.ConventionsPackName, pack, t => true);
            //set the cache
            if (settings.CacheSettings != null)
            {
                _CacheSettings = settings.CacheSettings;
                //var serializer = new NewtonsoftSerializer(new Newtonsoft.Json.JsonSerializerSettings()
                //{
                //    TypeNameHandling = Newtonsoft.Json.TypeNameHandling.All
                //});
                //if (CacheSettings.Enabled) _Cache = new StackExchangeRedisCacheClient(serializer, CacheSettings.ConnectionString, CacheSettings.KeyPrefix);
            }
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

        private bool _Azure = false;
        /// <summary>
        /// Gets if the repository is an Azure repository
        /// </summary>
        public bool Azure
        {
            get { return _Azure; }
        }

        private ICacheSettings _CacheSettings = new CacheSettings();
        /// <summary>
        /// Gets the cache settings
        /// </summary>
        public ICacheSettings CacheSettings
        {
            get { return _CacheSettings; }
        }

        //private ICacheClient _Cache;
        ///// <summary>
        ///// Gets the cache client
        ///// </summary>
        //public ICacheClient Cache
        //{
        //    get { return _Cache; }
        //}
    }
}
