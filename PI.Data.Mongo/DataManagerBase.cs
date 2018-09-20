using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PI.Utilities.Interfaces;
using PI.Data.Mongo.Interfaces;
using PI.Data.Mongo.Models;
namespace PI.Data.Mongo
{
    /// <summary>
    /// An abstract class to define the base of all the data managers, it contains a number of helper methods
    /// </summary>
    /// <remarks>Error codes follow the format DAL-DMB-XXX</remarks>
    public abstract class DataManagerBase : IDataManager
    {
        
        /// <summary>
        /// Flag to audit new items, set to false by default
        /// switch to True to audit new items as well as updated items
        /// </summary>
        public bool AuditNewItems { get; set; } = false;

        /// <summary>
        /// Flag to advise if full audit storage is applied for the data manager
        /// </summary>
        public bool AuditByDefault { get; set; } = true;

        /// <summary>
        /// protected property to store the IMongoDatabase used by the data manager
        /// </summary>
        public readonly IMongoDbRepository Repository = null;
        
        /// <summary>
        /// A Dictionary to provide granular audit settings for individual methods
        /// </summary>
        public Dictionary<string, string> AuditSettings { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Public string to store the User Name used to set the AuditFields
        /// </summary>
        public string AuditUser { get; set; }

        /// <summary>
        /// Sets the current working client id for the controller
        /// </summary>
        public string WorkingClientId { get; set; }
        
        /// <summary>
        /// Constructor for the class
        /// </summary>
        /// <remarks>Error codes follow the format DAL-DMB-1XX</remarks>
        /// <param name="repository">The IMongoDbRepository for the data store</param>
        /// <param name="auditSettings">Optional dictionary of audit settings</param>
        /// <param name="auditUser">Option string for the audit user</param>
        public DataManagerBase(IMongoRepositorySettings repositorySettings, Dictionary<string, string> auditSettings = null, string auditUser = null, string workingClientId = null)
        {
            Repository = new MongoDbRepository(repositorySettings);
            if (auditSettings != null) AuditSettings = auditSettings;
            if (!String.IsNullOrEmpty(auditUser)) AuditUser = auditUser;
            if (!String.IsNullOrEmpty(workingClientId)) WorkingClientId = workingClientId;
        }

        /// <summary>
        /// Constructor for the class
        /// </summary>
        /// <remarks>Error codes follow the format DAL-DMB-2XX</remarks>
        /// <param name="connectionString">The connection string to the data store</param>
        /// <param name="databaseName">The database name to target</param>
        /// <param name="auditSettings">Optional dictionary of audit settings</param>
        /// <param name="auditUser">Option string for the audit user</param>
        public DataManagerBase(string connectionString, string databaseName, Dictionary<string, string> auditSettings = null, string auditUser = null, string workingClientId = null)
        {
            Repository = new MongoDbRepository(new MongoRepositorySettings(connectionString, databaseName));
            if (auditSettings != null) AuditSettings = auditSettings;
            if (!String.IsNullOrEmpty(auditUser)) AuditUser = auditUser;
            if (!String.IsNullOrEmpty(workingClientId)) WorkingClientId = workingClientId;
        }


        /// <summary>
        /// Toggels the enabled value of the matching item
        /// </summary>
        /// <remarks>Error codes follow the format DAL-DMB-3XX</remarks>
        /// <param name="id">The id of the matching item</param>
        /// <returns>The matching IClient</returns>
        public async Task<UpdateResult> SimpleUpdate<T>(string id, UpdateDefinition<T> updateBuilder, string collectionName, string auditCollectionName, string auditSettingsKey = null, FilterDefinition<T> filter = null)
        {
            //just update what's required
            var collection = Repository.Database.GetCollection<T>(collectionName);
            var filterBuilder = Builders<T>.Filter;
            if (filter == null) { filter = filterBuilder.Empty; }
            filter = filter & filterBuilder.Eq("Id", id);
            var update = updateBuilder
                .Set("UpdatedDate", DateTime.UtcNow)
                .Set("UpdatedBy", this.AuditUser);
            var result = await collection.UpdateOneAsync(filter, update);

            //fire and forget the audit
            await AuditItem<T>(id, collectionName, auditCollectionName, auditSettingsKey);

            return result;
        }

        /// <summary>
        /// Audits the item based on the sent id and collection
        /// </summary>
        /// <remarks>Error codes follow the format DAL-DMB-4XX</remarks>
        /// <typeparam name="T">The type of the object to audit</typeparam>
        /// <param name="id">The id of the item to audit</param>
        /// <param name="collectionName">The collection name of the audit item</param>
        /// <param name="auditCollectionName">The collection name for the audit</param>
        /// <param name="auditSettingsKey">Optional audit setting key to reference the AuditSettings dictionary</param>
        /// <param name="auditUser">Optional string for the audit user</param>
        /// <param name="auditCapSize">Optional int to set the audit cap size</param>
        public async Task AuditItem<T>(string id, string collectionName, string auditCollectionName, string auditSettingsKey = null, string auditUser = null, int auditCapSize = 100)
        {
            //set audit by default value item has not yet been created
            bool auditItem = AuditByDefault;
            if (auditItem)
            {
                //check the audit settings allow
                try
                {
                    auditItem = (!String.IsNullOrEmpty(auditSettingsKey) && AuditSettings != null && AuditSettings.ContainsKey(auditSettingsKey)) ? bool.Parse(AuditSettings[auditSettingsKey]) : auditItem;
                }
                catch { }
            }
            //audit item if set 
            if (auditItem)
            {
                try
                {
                    auditCapSize = (!String.IsNullOrEmpty(auditCollectionName + ".Size") && AuditSettings != null && AuditSettings.ContainsKey(auditCollectionName + ".Size")) ? int.Parse(AuditSettings[auditCollectionName + ".Size"]) : auditCapSize;
                }
                catch { }
                T item = await SearchManager.Find<T>(Repository.Database, collectionName, id, WorkingClientId);
                await AuditManager.AuditItem<T>(Repository.Database, item, false, auditCollectionName, auditUser, auditCapSize);
            }
        }

        /// <summary>
        /// Audits the item based on the sent id and collection
        /// </summary>
        /// <remarks>Error codes follow the format DAL-DMB-4XX</remarks>
        /// <typeparam name="T">The type of the object to audit</typeparam>
        /// <param name="items">An IEnumerable list of items to audit</param>
        /// <param name="auditCollectionName">The collection name for the audit</param>
        /// <param name="auditSettingsKey">Optional audit setting key to reference the AuditSettings dictionary</param>
        /// <param name="auditUser">Optional string for the audit user</param>
        /// <param name="auditCapSize">Optional int to set the audit cap size</param>
        public async Task AuditItems(string[] ids, string collectionName, string auditCollectionName, string auditSettingsKey = null, string auditUser = null, int auditCapSize = 100)
        {
            //set audit by default value item has not yet been created
            bool auditItem = AuditByDefault;
            if (auditItem)
            {
                //check the audit settings allow
                try
                {
                    auditItem = (!String.IsNullOrEmpty(auditSettingsKey) && AuditSettings != null && AuditSettings.ContainsKey(auditSettingsKey)) ? bool.Parse(AuditSettings[auditSettingsKey]) : auditItem;
                }
                catch { }
            }
            //audit item if set 
            if (auditItem)
            {
                try
                {
                    auditCapSize = (!String.IsNullOrEmpty(auditCollectionName + ".Size") && AuditSettings != null && AuditSettings.ContainsKey(auditCollectionName + ".Size")) ? int.Parse(AuditSettings[auditCollectionName + ".Size"]) : auditCapSize;
                }
                catch { }
                FilterDefinitionBuilder<BaseIdWithQuickSearchModel> filterBuilder = Builders<BaseIdWithQuickSearchModel>.Filter;
                var filter = filterBuilder.Empty;
                filter = filter & filterBuilder.Where(w => ids.Contains(w.Id));
                var collection = Repository.Database.GetCollection<BaseIdWithQuickSearchModel>(collectionName);
                var items = await collection.Find(filter).ToListAsync<BaseIdWithQuickSearchModel>();
                foreach (var item in items) await AuditManager.AuditItem<BaseIdWithQuickSearchModel>(Repository.Database, item, false, auditCollectionName, auditUser, auditCapSize);
            }
        }

        /// <summary>
        /// Audits the item based on the sent id and collection
        /// </summary>
        /// <remarks>Error codes follow the format DAL-DMB-4XX</remarks>
        /// <typeparam name="T">The type of the object to audit</typeparam>
        /// <param name="items">An IEnumerable list of items to audit</param>
        /// <param name="auditCollectionName">The collection name for the audit</param>
        /// <param name="auditSettingsKey">Optional audit setting key to reference the AuditSettings dictionary</param>
        /// <param name="auditUser">Optional string for the audit user</param>
        /// <param name="auditCapSize">Optional int to set the audit cap size</param>
        public async Task AuditItems<T>(IEnumerable<T> items, string auditCollectionName, string auditSettingsKey = null, string auditUser = null, int auditCapSize = 100)
        {
            //set audit by default value item has not yet been created
            bool auditItem = AuditByDefault;
            if (auditItem)
            {
                //check the audit settings allow
                try
                {
                    auditItem = (!String.IsNullOrEmpty(auditSettingsKey) && AuditSettings != null && AuditSettings.ContainsKey(auditSettingsKey)) ? bool.Parse(AuditSettings[auditSettingsKey]) : auditItem;
                }
                catch { }
            }
            //audit item if set 
            if (auditItem)
            {
                try
                {
                    auditCapSize = (!String.IsNullOrEmpty(auditCollectionName + ".Size") && AuditSettings != null && AuditSettings.ContainsKey(auditCollectionName + ".Size")) ? int.Parse(AuditSettings[auditCollectionName + ".Size"]) : auditCapSize;
                }
                catch { }
                foreach(var item in items) await AuditManager.AuditItem<T>(Repository.Database, item, false, auditCollectionName, auditUser, auditCapSize);
            }
        }

        /// <summary>
        /// Audits the sent item
        /// </summary>
        /// <remarks>Error codes follow the format DAL-DMB-5XX</remarks>
        /// <typeparam name="T">The type of the object to audit</typeparam>
        /// <param name="item">The item to audit</param>
        /// <param name="auditCollectionName">The collection name for the audit</param>
        /// <param name="auditSettingsKey">Optional audit setting key to reference the AuditSettings dictionary</param>
        /// <param name="auditUser">Optional string for the audit user</param>
        public async Task AuditItem<T>(T item, string auditCollectionName, string auditSettingsKey = null, string auditUser = null)
        {
            //set the user name if sent
            if (!String.IsNullOrEmpty(AuditUser) && String.IsNullOrEmpty(auditUser)) auditUser = AuditUser;
            //create the new item or update the item audit fields            
            bool newItem = (item is IAuditFieldsWithId) ? AuditManager.SetAuditFields(item as IAuditFieldsWithId, auditUser) : true;
            //set audit by default value item has not yet been created
            bool auditItem = AuditByDefault;
            if(auditItem)
            {
                //check the audit settings allow
                auditItem  = (!String.IsNullOrEmpty(auditSettingsKey) && AuditSettings != null && AuditSettings.ContainsKey(auditSettingsKey))? bool.Parse(AuditSettings[auditSettingsKey]) : auditItem;
            }
            //audit item if set 
            if (auditItem) await AuditManager.AuditItem<T>(Repository.Database, item, newItem, auditCollectionName, auditUser);
        }

        /// <summary>
        /// Drops the collection
        /// </summary>
        /// <remarks>Error codes follow the format DAL-DMB-6XX</remarks>
        /// <param name="collectionName">The name of the collection to drop</param>
        /// <returns></returns>
        public async Task DropCollection(string collectionName)
        {
            await Repository.Database.DropCollectionAsync(collectionName);
        }

        /// <summary>
        /// Returns true if the collection exists
        /// </summary>
        /// <remarks>Error codes follow the format DAL-DMB-7XX</remarks>
        /// <param name="collectionName">The name of the collection to check exists</param>
        /// <returns></returns>
        public async Task<bool> CollectionExists(string collectionName)
        {
            var filter = new BsonDocument("name", collectionName);
            var collections = await Repository.Database.ListCollectionsAsync(new ListCollectionsOptions { Filter = filter });
            return await collections.AnyAsync();
        }

        /// <summary>
        /// Creates the collection name from the prefix and suffix
        /// </summary>
        /// <remarks>Error codes follow the format DAL-DMB-8XX</remarks>
        /// <param name="collectionName">The collection Name</param>
        /// <param name="collectionPrefix">The prefix</param>
        /// <param name="collectionSuffix">The suffix</param>
        /// <returns>The generated collection name</returns>
        public static string GetCollectionName(string collectionName, string collectionPrefix = null, string collectionSuffix = null)
        {
            if (!String.IsNullOrEmpty(collectionPrefix)) collectionName = collectionPrefix + collectionName;
            if (!String.IsNullOrEmpty(collectionSuffix)) collectionName += collectionSuffix;
            return collectionName;

        }

    }
}
