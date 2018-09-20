using MongoDB.Bson;
using MongoDB.Driver;
using PI.Utilities.Interfaces;
using PI.Utilities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PI.Data.Mongo
{
    /// <summary>
    /// Static class to manage audit functions
    /// </summary>
    /// <remarks>Error codes follow the format DAL-AM-XXX</remarks>
    public static class AuditManager
    {

        /// <summary>
        /// Static method to set the audit fields for the set item returning the id
        /// </summary>
        /// <remarks>Error codes follow the format DAL-AM-1XX</remarks>
        /// <param name="item">The IAuditFields item to set</param>
        /// <param name="userName">Optional user name to manually set</param>
        /// <returns>True if the create data has been set</returns>
        public static bool SetAuditFields(IAuditFieldsWithId item, string userName = null)
        {
            if (String.IsNullOrEmpty(userName))
            {
                userName = (System.Threading.Thread.CurrentPrincipal != null && System.Threading.Thread.CurrentPrincipal.Identity != null && !string.IsNullOrEmpty(System.Threading.Thread.CurrentPrincipal.Identity.Name) ? System.Threading.Thread.CurrentPrincipal.Identity.Name : string.Empty);
            }
            bool newItem = false;
            //generate a new object id if not set
            if (String.IsNullOrEmpty(item.Id))
            {
                //generate Id
                item.Id = ObjectId.GenerateNewId().ToString();
                newItem = true;
            }
            //add in the created values 
            if(newItem || item.CreatedDate == new DateTime(1900, 1, 1))
            {
                item.CreatedBy = userName;
                item.CreatedDate = DateTime.UtcNow;
                newItem = true;
            }
            item.UpdatedBy = userName;
            item.UpdatedDate = DateTime.UtcNow;
            return newItem;
        }

        /// <summary>
        /// Method to Audit an item and store it in the database
        /// </summary>
        /// <remarks>Error codes follow the format DAL-AM-2XX</remarks>
        /// <typeparam name="T">The type of the object to audit</typeparam>
        /// <param name="database">The IMongoDatabase to store the audit in</param>
        /// <param name="item">The item to audit</param>
        /// <param name="newItem"></param>
        /// <param name="collectionName">Optional collection name to audit the item in</param>
        /// <param name="auditName">Optional name of the user who audited the item</param>
        public static async Task AuditItem<T>(IMongoDatabase database, T item, bool newItem, string collectionName = "Audit", string auditName = null, int auditCapSize = 100)
        {
            if (String.IsNullOrEmpty(auditName))
            {
                auditName = (System.Threading.Thread.CurrentPrincipal != null && System.Threading.Thread.CurrentPrincipal.Identity != null && !string.IsNullOrEmpty(System.Threading.Thread.CurrentPrincipal.Identity.Name) ? System.Threading.Thread.CurrentPrincipal.Identity.Name : string.Empty);
            }

            string id = (item is IAuditFieldsWithId) ? (item as IAuditFieldsWithId).Id : ObjectId.GenerateNewId().ToString();
            //if (item is IClientId) id += ":" + (item as IClientId).ClientId;
            var auditContainer = new AuditContainer<T>()
            {
                Id = id,
                AuditItems = new List<T>()
            };
            auditContainer.AuditItems.Add(item);
                        
            //try upsert
            var collection = database.GetCollection<BsonDocument>(collectionName);
            var filterBuilder = Builders<BsonDocument>.Filter;
            var filter = filterBuilder.Eq("_id", id);
            var update = Builders<BsonDocument>.Update.Push<T>("auditItems", item);
            var result = await collection.UpdateOneAsync(filter, update);

            if (!result.IsModifiedCountAvailable || result.ModifiedCount == 0)
            {
                //insert audit item
                await database.GetCollection<AuditContainer<T>>(collectionName).InsertOneAsync(auditContainer);
            }
            else
            {
                if (auditCapSize > 0)
                {
                    //cap the array
                    filter = filter & filterBuilder.Exists("auditItems." + auditCapSize.ToString());
                    var updatePop = Builders<BsonDocument>.Update.PopFirst("auditItems");
                    var popResult = await collection.UpdateOneAsync(filter, updatePop);
                }
            }
            
        }

        /// <summary>
        /// Retrieves a list of all the audit items 
        /// </summary>
        /// <remarks>Error codes follow the format DAL-AM-3XX</remarks>
        /// <typeparam name="T">The type of the object to audit</typeparam>
        /// <param name="database">The IMongoDatabase to store the audit in</param>
        /// <param name="id">The id of the item within the audit trail</param>
        /// <param name="collectionName">The name of the audit collection</param>
        /// <returns>A JSON string of the matching audit item</returns>
        public static async Task<SearchResult<T>> RetrieveAuditContainer<T>(IMongoDatabase database, string id, string clientId = null, string collectionName = "Audit", int startAt = 0, int pageSize = 500)
        {
            SearchResult<T> result = new SearchResult<T>()
            {
                PageSize = pageSize,
                StartAt = startAt,
                Results = new List<T>()
            };
            //make the id contain the client id for AuditContainers and null the sent client id
            //if (!String.IsNullOrEmpty(clientId)) id += ":" + clientId;
            var item = await SearchManager.Find<AuditContainer<T>>(database, collectionName, id, null);
            if (item != null)
            {
                result.TotalCount = item.AuditItems.Count();
                item.AuditItems.Reverse();
                result.Results = item.AuditItems.Skip(result.StartAt).Take(pageSize);
            } 
            return result;
        }


        /// <summary>
        /// Retrieves a list of all the audit items 
        /// </summary>
        /// <remarks>Error codes follow the format DAL-AM-4XX</remarks>
        /// <typeparam name="T">The type of the object to audit</typeparam>
        /// <param name="database">The IMongoDatabase to store the audit in</param>
        /// <param name="id">The id of the item within the audit trail</param>
        /// <param name="collectionName">The name of the audit collection</param>
        /// <returns>A JSON string of the matching audit item</returns>
        public static async Task<SearchResult<T>> RetrieveAuditContainerSliced<T>(IMongoDatabase database, string id, string collectionName = "Audit", int startAt = 0, int pageSize = 500)
        {
            SearchResult<T> result = new SearchResult<T>()
            {
                PageSize = pageSize,
                StartAt = startAt,
                Results = new List<T>()
            };

            var filter = Builders<AuditContainer<T>>.Filter.Eq(f => f.Id, id);
            var projection = (startAt <= 0) ? Builders<AuditContainer<T>>.Projection.Include("AuditItems").Slice("AuditItems", -result.PageSize) : Builders<AuditContainer<T>>.Projection.Include("AuditItems").Slice("AuditItems", -result.StartAt, result.PageSize);
            var results = await database.GetCollection<AuditContainer<T>>(collectionName)
                .Find(filter)
                .Project<AuditContainer<T>>(projection)
                .FirstOrDefaultAsync();
            if (result != null) result.Results = results.AuditItems;
            result.Results.Reverse();
            return result;
        }
    }
}
