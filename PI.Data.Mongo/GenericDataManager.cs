using MongoDB.Bson;
using MongoDB.Driver;
using PI.Data.Mongo.Interfaces;
using PI.Utilities.Interfaces;
using PI.Utilities.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace PI.Data.Mongo
{

    /// <summary>
    /// A generic data manager, you can use for most standard data requirements
    /// Inherit and create your own manager to complete updates by overriding the BuildUpdate method
    /// </summary>
    /// <typeparam name="T">The type of the object to manage</typeparam>
    public class GenericDataManager<T> : DataManagerBase where T : IAuditFieldsWithId
    {

        #region Properties

        //public event UpdateDelegate<T> Updating;

        /// <summary>
        /// The name of the collection to use
        /// </summary>
        public string CollectionName { get; set; }

        /// <summary>
        /// The name of the collection to use
        /// </summary>
        public string CollectionNameLive { get; set; }

        /// <summary>
        /// The name of the collection to use for auditing
        /// </summary>
        public string CollectionNameAudit { get; set; }

        /// <summary>
        /// The name of the collection to use for auditing
        /// </summary>
        public string CollectionNameAuditLive { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor for the class
        /// </summary>
        /// <param name="repositorySettings">The IMongoRepositorySettings settings for the manager</param>
        /// <param name="auditSettings">Optional audit settings to use</param>
        /// <param name="auditUser">Optional name of the user to save for audi</param>
        /// <param name="workingClientId">Optional id for the current client</param>
        public GenericDataManager(IMongoRepositorySettings repositorySettings, Dictionary<string, string> auditSettings = null, string auditUser = null, string workingClientId = null) : base(repositorySettings, auditSettings, auditUser, workingClientId)
        { }

        /// <summary>
        /// Constructor for the class
        /// </summary>
        /// <param name="connectionString">The connection string to the data store</param>
        /// <param name="databaseName">The database name to target</param>
        /// <param name="auditSettings">Optional audit settings to use</param>
        /// <param name="auditUser">Optional name of the user to save for audi</param>
        /// <param name="workingClientId">Optional id for the current client</param>
        public GenericDataManager(string connectionString, string databaseName, Dictionary<string, string> auditSettings = null, string auditUser = null, string workingClientId = null) : base(connectionString, databaseName, auditSettings, auditUser, workingClientId)
        { }

        #endregion

        /// <summary>
        /// Returns a matching item from the data store
        /// </summary>
        /// <remarks>Error codes follow the format DAL-CM-1XX</remarks>
        /// <param name="id">The id of the item to find</param>
        /// <returns>The matching item or null</returns>
        public async Task<T> Find(string id)
        {
            var result = await SearchManager.Find<T>(Repository.Database, CollectionName, id, WorkingClientId);
            return result;
        }

        /// <summary>
        /// Search the items with the matching search term
        /// </summary>
        /// <remarks>Error codes follow the format DAL-CM-3XX</remarks>
        /// <param name="searchTerm">Optional search term to match on</param>
        /// <param name="filter">The filter with match types gt, lt, eq, regex (greater than, less than, equal and regex). E.g filter=name:contains:Visa or filter=allowChildren:eq:true</param>
        /// <param name="sort">The sort order E.g search=name desc,createdDate asc</param>
        /// <param name="startAt">Start at index, defualts to 0</param>
        /// <param name="pageSize">Page size, defaults to 500</param>
        /// <param name="deleted">Optional flag to set to filter deleted items on = defaulted to false</param>
        /// <param name="ignoreDeleteFlag">Optional flag to advise if the results should be filtered by the deleted value</param>
        /// <param name="archived">Optional flag to set to filter archive items on = defaulted to false</param>
        /// <param name="ignoreArchiveFlag">Optional flag to advise if the results should be filtered by the archived value</param>
        /// <returns>A SearchResult with the list of items</returns>
        public async Task<SearchResult<T>> Search(string searchTerm = null, string filter = null, string sort = null, int startAt = 0, int pageSize = 500, bool deleted = false, bool ignoreDeleteFlag = false, bool archived = false, bool ignoreArchiveFlag = false)
        {
            //create promotionId filter
            FilterDefinitionBuilder<T> filterBuilder = Builders<T>.Filter;
            var filterDef = filterBuilder.Empty;
            if (!String.IsNullOrEmpty(filter)) filterDef = SearchManager.AddFilter(filter, filterDef);
            return await SearchManager.Search<T>(Repository.Database, CollectionName, WorkingClientId, searchTerm, sort, startAt, pageSize, filter: filterDef, filterStr: filter, deleted: deleted, ignoreDeleteFlag: ignoreDeleteFlag, archived: archived, ignoreArchiveFlag: ignoreArchiveFlag);
        }

        /// <summary>
        /// Search the items with the matching search term
        /// This search lets you alter the object type being returned
        /// </summary>
        /// <remarks>Error codes follow the format DAL-CM-3XX</remarks>
        /// <typeparam name="I">A type to return from the search</typeparam>
        /// <param name="searchTerm">Optional search term to match on</param>
        /// <param name="filter">The filter with match types gt, lt, eq, regex (greater than, less than, equal and regex). E.g filter=name:contains:Visa or filter=allowChildren:eq:true</param>
        /// <param name="sort">The sort order E.g search=name desc,createdDate asc</param>
        /// <param name="startAt">Start at index, defualts to 0</param>
        /// <param name="pageSize">Page size, defaults to 500</param>
        /// <param name="deleted">Optional flag to set to filter deleted items on = defaulted to false</param>
        /// <param name="ignoreDeleteFlag">Optional flag to advise if the results should be filtered by the deleted value</param>
        /// <param name="archived">Optional flag to set to filter archive items on = defaulted to false</param>
        /// <param name="ignoreArchiveFlag">Optional flag to advise if the results should be filtered by the archived value</param>
        /// <returns>A SearchResult with the list of items</returns>
        public async Task<SearchResult<I>> Search<I>(string searchTerm = null, string filter = null, string sort = null, int startAt = 0, int pageSize = 500, bool deleted = false, bool ignoreDeleteFlag = false, bool archived = false, bool ignoreArchiveFlag = false)
        {
            //create promotionId filter
            FilterDefinitionBuilder<I> filterBuilder = Builders<I>.Filter;
            var filterDef = filterBuilder.Empty;
            if (!String.IsNullOrEmpty(filter)) filterDef = SearchManager.AddFilter(filter, filterDef);
            return await SearchManager.Search<I>(Repository.Database, CollectionName, WorkingClientId, searchTerm, sort, startAt, pageSize, filter: filterDef, filterStr: filter, deleted: deleted, ignoreDeleteFlag: ignoreDeleteFlag, archived: archived, ignoreArchiveFlag: ignoreArchiveFlag);
        }

        /// <summary>
        /// Creates the item in the data store
        /// </summary>
        /// <remarks>Error codes follow the format DAL-CM-4XX</remarks>
        /// <param name="item">Class implementing the property create interface</param>
        /// <param name="dataResult">Optional data result interface</param>
        /// <returns>Returns the new item in full</returns>
        public async Task<T> Create(object item, IDataResult dataResult = null)
        {
            //create item from summary
            var fullItem = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<T>(item.ToBsonDocument());
            //create a new Id
            fullItem.Id = ObjectId.GenerateNewId().ToString();
            //save and return
            return await Save(fullItem, dataResult, true);
        }

        /// <summary>
        /// Saves the item to the data store
        /// This will Upsert the entire item so use with care
        /// </summary>
        /// <remarks>Error codes follow the format DAL-CM-5XX</remarks>
        /// <param name="item">The full item to save</param>
        /// <param name="dataResult">Optional data result interface</param>
        /// <param name="create">Optional create parameter used for the data result to mark as complete</param>
        /// <returns>Returns the item sent</returns>
        public async Task<T> Save(T item, IDataResult dataResult = null, bool create = false)
        {
            //audit item
            await AuditItem<T>(item, CollectionName, CollectionName + ".Save");
            //get the collection and do replace one
            var collection = Repository.Database.GetCollection<T>(CollectionName);
            var result = await collection.ReplaceOneAsync(x => x.Id == item.Id, item, new UpdateOptions
            {
                IsUpsert = true
            });
            if (dataResult != null) dataResult.SetDataResults(result.IsAcknowledged, result.IsModifiedCountAvailable, result.MatchedCount, result.ModifiedCount, (create) ? 0 : 1, (create) ? 0 : 1);
            return item;
        }


        /// <summary>
        /// Updates the item in the data store
        /// </summary>
        /// <remarks>Error codes follow the format DAL-CM-6XX</remarks>
        /// <typeparam name="I">The item type to update against</typeparam>
        /// <param name="item">Class implementing the property edit interface</param>
        /// <param name="dataResult">Optional data result interface</param>
        /// <returns>Returns the item sent</returns>
        public async Task<I> Update<I>(I item, IDataResult dataResult = null) where I : IId
        {
            //create update
            var update = BuildUpdate<I>(item);

            //call simple update
            var result = await SimpleUpdate<T>(item.Id, update, CollectionName, CollectionNameAudit, CollectionName + ".Update");
            if (dataResult != null) dataResult.SetDataResults(result.IsAcknowledged, result.IsModifiedCountAvailable, result.MatchedCount, result.ModifiedCount);
            //return item
            return item;
        }

        /// <summary>
        /// Virtual method to be overridden to implement the update builder for targeted updates
        /// </summary>
        /// <typeparam name="I">The item type to update against</typeparam>
        /// <param name="item">The item to update with</param>
        /// <returns></returns>
        public virtual UpdateDefinition<T> BuildUpdate<I>(I item) where I : IId
        {
            //just update what's required
            var update = Builders<T>.Update.Set(s => s.Id, item.Id);
            return update;
        }
                
    }
}
