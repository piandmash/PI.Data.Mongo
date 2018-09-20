using MongoDB.Bson;
using MongoDB.Driver;
using PI.Utilities.Models;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PI.Data.Mongo
{
    /// <summary>
    /// Static class to facillitate standard searches
    /// </summary>
    /// <remarks>Error codes follow the format DAL-SM-XXX</remarks>
    public static class SearchManager
    {

        /// <summary>
        /// Gets a matching contest from the data store
        /// </summary>
        /// <remarks>Error codes follow the format DAL-SM-1XX</remarks>
        /// <param name="database">The mongo database</param>
        /// <param name="collectionName">The collection name</param>
        /// <param name="id">The id of the item</param>
        /// <returns>Returns true if the item exists</returns>
        public static async Task<bool> Exists(IMongoDatabase database, string collectionName, string id)
        {
            var filterBuilder = Builders<BsonDocument>.Filter;
            var filter = filterBuilder.Eq("_id", ObjectId.Parse(id));
            var collection = database.GetCollection<BsonDocument>(collectionName);
            var result = await collection.CountDocumentsAsync(filter);
            return result > 0;
        }

        /// <summary>
        /// Gets a matching contest from the data store
        /// </summary>
        /// <remarks>Error codes follow the format DAL-SM-2XX</remarks>
        /// <param name="database">The mongo database</param>
        /// <param name="collectionName">The collection name</param>
        /// <param name="id">The id of the item</param>
        /// <param name="clientId">The id of the client</param>
        /// <param name="filter">Optional filter definition to use</param>
        /// <param name="projection">Optional projection definition to project into</param>
        /// <returns>The matching Item</returns>
        public static async Task<T> Find<T>(IMongoDatabase database, string collectionName, string id = null, string clientId = null, FilterDefinition<T> filter = null, ProjectionDefinition<T, T> projection = null)
        {
            var filterBuilder = Builders<T>.Filter;
            if (filter == null) { filter = filterBuilder.Empty; }
            if (!String.IsNullOrEmpty(id)) filter = filter & filterBuilder.Eq("Id", id);
            if(!String.IsNullOrEmpty(clientId)) filter = filter & filterBuilder.Eq("ClientId", clientId);
            var collection = database.GetCollection<T>(collectionName);
            T result;
            if (projection != null)
            {
                result = await collection
                .Find(filter)
                .Project(projection)
                .FirstOrDefaultAsync();
            } else
            {
                result = await collection
                .Find(filter)
                .FirstOrDefaultAsync();
            }
            return result;
        }

        /// <summary>
        /// Gets a matching items from the data store
        /// </summary>
        /// <remarks>Error codes follow the format DAL-SM-3XX</remarks>
        /// <typeparam name="T">The type of the item to search on</typeparam>
        /// <param name="database">The database in the store</param>
        /// <param name="collectionName">The collection name to search</param>
        /// <param name="clientId">Optional clientId to filter the items on</param>
        /// <param name="searchTerm">Optional search term to search the quicksearchvalue field on</param>
        /// <param name="filter">Optional filter builder of Type T to filter with</param>
        /// <param name="projection">Optional projection definition of Type T to populate</param>
        /// <param name="filterStr">Optional string to represent the filter used</param>
        /// <param name="deleted">Optional flag to set to filter deleted items on = defaulted to false</param>
        /// <param name="ignoreDeleteFlag">Optional flag to advise if the results should be filtered by the deleted value</param>
        /// <param name="archived">Optional flag to set to filter archive items on = defaulted to false</param>
        /// <param name="ignoreArchiveFlag">Optional flag to advise if the results should be filtered by the archived value</param>
        /// <returns>A search result with them matching items</returns>
        public static async Task<SearchResult<T>> Search<T>(IMongoDatabase database, string collectionName, string clientId = null, string searchTerm = null, string sort = null, int startAt = 0, int pageSize = 10, FilterDefinition<T> filter = null, ProjectionDefinition<T, T> projection = null, string filterStr = null, bool deleted = false, bool ignoreDeleteFlag = false, bool archived = false, bool ignoreArchiveFlag = false)
        {
            SearchResult<T> result = new SearchResult<T>()
            {
                PageSize = pageSize,
                Sort = sort,
                SearchTerm = searchTerm,
                Filter = filterStr,
                StartAt = startAt
            };
            return await Search<T>(database, collectionName, result, clientId, searchTerm, filter, projection, deleted, ignoreDeleteFlag, archived, ignoreArchiveFlag);
        }

        /// <summary>
        /// Gets a matching items from the data store
        /// </summary>
        /// <remarks>Error codes follow the format DAL-SM-4XX</remarks>
        /// <typeparam name="T">The type of the item to search on</typeparam>
        /// <param name="database">The database in the store</param>
        /// <param name="collectionName">The collection name to search</param>
        /// <param name="result">A search result of Type T to populate</param>
        /// <param name="clientId">Optional clientId to filter the items on</param>
        /// <param name="searchTerm">Optional search term to search the quicksearchvalue field on</param>
        /// <param name="filter">Optional filter builder of Type T to filter with</param>
        /// <param name="projection">Optional projection definition of Type T to populate</param>
        /// <param name="deleted">Optional flag to set to filter deleted items on = defaulted to false</param>
        /// <param name="ignoreDeleteFlag">Optional flag to advise if the results should be filtered by the deleted value</param>
        /// <param name="archived">Optional flag to set to filter archive items on = defaulted to false</param>
        /// <param name="ignoreArchiveFlag">Optional flag to advise if the results should be filtered by the archived value</param>
        /// <returns>A search result with them matching items</returns>
        public static async Task<SearchResult<T>> Search<T>(IMongoDatabase database, string collectionName, SearchResult<T> result, string clientId = null, string searchTerm = null, FilterDefinition<T> filter = null, ProjectionDefinition<T, T> projection = null, bool deleted = false, bool ignoreDeleteFlag = false, bool archived = false, bool ignoreArchiveFlag = false)
        {
            //create filter
            var filterBuilder = Builders<T>.Filter;
            if (filter == null) { filter = filterBuilder.Empty; }
            if (!ignoreDeleteFlag) filter = filter & filterBuilder.Eq(ConvertCase<T>("deleted"), deleted);
            if (!ignoreArchiveFlag) filter = filter & filterBuilder.Eq(ConvertCase<T>("archived"), archived);
            if (!String.IsNullOrEmpty(clientId)) filter = filter & filterBuilder.Eq(ConvertCase<T>("clientId"), clientId);
            if (!String.IsNullOrEmpty(searchTerm)) filter = filter & Builders<T>.Filter.Regex(ConvertCase<T>("quickSearchValue"), new BsonRegularExpression(new Regex(searchTerm, RegexOptions.IgnoreCase)));
            //create collection
            var collection = database.GetCollection<T>(collectionName);
            //get total documents
            result.TotalCount = await collection.CountDocumentsAsync(filter);
            //create sort
            var sort = CreateSort<T>(result.Sort);
            if (sort == null) sort = new BsonDocument();
            //get results
            if (projection != null)
            {
                result.Results = await collection
                    .Find(filter)
                    .Project(projection)
                    .Sort(sort)
                    .Skip(result.StartAt)
                    .Limit(result.PageSize)
                    .ToListAsync<T>();
            }
            else
            {
                result.Results = await collection
                    .Find(filter)
                    .Sort(sort)
                    .Skip(result.StartAt)
                    .Limit(result.PageSize)
                    .ToListAsync<T>();
            }
            return result;
        }


        /// <summary>
        /// Creates a SortDefintiion for the sort order
        /// </summary>
        /// <remarks>Error codes follow the format DAL-SM-5XX</remarks>
        /// <typeparam name="T">The document type</typeparam>
        /// <param name="sort">The sort string</param>
        /// <returns>A generated Sort Definition</returns>
        public static SortDefinition<T> CreateSort<T>(string sort)
        {
            var builder = Builders<T>.Sort;
            SortDefinition<T> sortDef = null;
            if (String.IsNullOrEmpty(sort)) return sortDef;
            foreach (string s in sort.Split(','))
            {
                string[] sortItemParts = s.Trim().Split(' ');
                string sortItemDirection = sortItemParts.Length > 1 ? sortItemParts[1] : "asc";
                var sortField = ConvertCase<T>(sortItemParts[0]);
                if (sortItemDirection.ToLower() == "desc")
                {
                    sortDef = (sortDef == null ) ? builder.Descending(sortField) : sortDef.Descending(sortField);
                }
                else
                {
                    sortDef = (sortDef == null) ? builder.Ascending(sortField) : sortDef.Ascending(sortField);
                }
            }
            return sortDef;
        }

        /// <summary>
        /// Converts the property name from camel case if not a BSON Document
        /// </summary>
        /// <remarks>Error codes follow the format DAL-SM-6XX</remarks>
        /// <typeparam name="T">The type to test against</typeparam>
        /// <param name="propertyName">The property name to convert</param>
        /// <returns>The converted property name</returns>
        public static string ConvertCase<T>(string propertyName)
        {
            if (String.IsNullOrEmpty(propertyName)) return propertyName;
            string firstLetter = propertyName.Substring(0, 1);
            string rest = (propertyName.Length > 1) ? propertyName.Substring(1) : "";
            firstLetter = (typeof(T) != typeof(BsonDocument) && !typeof(T).IsInterface) ? firstLetter.ToUpper() : firstLetter.ToLower();
            return firstLetter + rest;
        }

        /// <summary>
        /// Creates a FilterDefinition for the sent string using the format property asc|desc, property asc|desc
        /// </summary>
        /// <remarks>Error codes follow the format DAL-SM-7XX</remarks>
        /// <typeparam name="T">The document type</typeparam>
        /// <param name="sort">The sort string</param>
        /// <returns>A generated Sort Definition</returns>
        public static FilterDefinition<T> AddFilter<T>(string filter = null, FilterDefinition<T> filterDef = null)
        {
            if (String.IsNullOrEmpty(filter)) return filterDef;
            var builder = Builders<T>.Filter;
            if (filterDef == null) filterDef = Builders<T>.Filter.Empty;
            foreach (string f in filter.Split(','))
            {
                string[] itemParts = f.Trim().Split(':');
                if(itemParts.Length == 2)
                {
                    filterDef = filterDef & builder.Eq(ConvertCase<T>(itemParts[0]), itemParts[1]);
                }
                if (itemParts.Length == 3)
                {
                    switch (itemParts[1].ToLower())
                    {
                        case "eq":
                            filterDef = filterDef & builder.Eq(ConvertCase<T>(itemParts[0]), itemParts[2]);
                            break;
                        case "lt":
                            filterDef = filterDef & builder.Lt(ConvertCase<T>(itemParts[0]), itemParts[2]);
                            break;
                        case "gt":
                            filterDef = filterDef & builder.Gt(ConvertCase<T>(itemParts[0]), itemParts[2]);
                            break;
                        case "regex":
                        case "contains":
                            filterDef = filterDef & builder.Regex(ConvertCase<T>(itemParts[0]), new BsonRegularExpression(new Regex(itemParts[2], RegexOptions.IgnoreCase)));
                            break;
                    }
                }
            }
            return filterDef;
        }

        /// <summary>
        /// Retrieves a list of all the audit items 
        /// </summary>
        /// <remarks>Error codes follow the format DAL-SM-8XX</remarks>
        /// <typeparam name="T">The type of the object to audit</typeparam>
        /// <param name="database">The IMongoDatabase to store the audit in</param>
        /// <param name="id">The id of the item within the audit trail</param>
        /// <param name="collectionName">The name of the audit collection</param>
        /// <returns>A JSON string of the matching audit item</returns>
        public static async Task<List<T>> RetrieveAuditContainer<T>(IMongoDatabase database, string id, string collectionName = "Audit", int pageSize = 500, int pageIndex = 0)
        {
            var skip = (pageIndex > 0) ? pageIndex * pageSize : 0;
            var filter = Builders<AuditContainer<T>>.Filter.Eq(f => f.Id, id);
            var projection = (skip <= 0) ? Builders<AuditContainer<T>>.Projection.Include("AuditItems").Slice("AuditItems", -pageSize) : Builders<AuditContainer<T>>.Projection.Include("AuditItems").Slice("AuditItems", -skip, pageSize);
            var result = await database.GetCollection<AuditContainer<T>>(collectionName)
                .Find(filter)
                .Project<AuditContainer<T>>(projection)
                .FirstOrDefaultAsync();
            if (result != null)
            {
                result.AuditItems.Reverse();
                return result.AuditItems;
            }
            return new List<T>();
        }


        /// <summary>
        /// Gets a matching items from the data store
        /// </summary>
        /// <remarks>Error codes follow the format DAL-SM-9XX</remarks>
        /// <typeparam name="T">The type of the object</typeparam>
        /// <param name="database">The database in the store</param>
        /// <param name="collectionName">The collection name to search</param>
        /// <param name="id">Optional id to use as the filter</param>
        /// <param name="filter">Optional filter builder of Type T to filter with</param>
        /// <param name="deleted">Optional flag to set to filter deleted items on = defaulted to false</param>
        /// <param name="ignoreDeleteFlag">Optional flag to advise if the results should be filtered by the deleted value</param>
        /// <param name="archived">Optional flag to set to filter archive items on = defaulted to false</param>
        /// <param name="ignoreArchiveFlag">Optional flag to advise if the results should be filtered by the archived value</param>
        /// <returns>The matching Items</returns>
        public static async Task<bool> ItemExists<T>(IMongoDatabase database, string collectionName, string id = null, FilterDefinition<T> filter = null, ProjectionDefinition<T, T> projection = null, bool deleted = false, bool ignoreDeleteFlag = false, bool archived = false, bool ignoreArchiveFlag = false)
        {
            //create filter
            var filterBuilder = Builders<T>.Filter;
            if (filter == null) { filter = filterBuilder.Empty; }
            if (!ignoreDeleteFlag) filter = filter & filterBuilder.Eq("Deleted", deleted);
            if (!ignoreArchiveFlag) filter = filter & filterBuilder.Eq("Archived", archived);
            if (!String.IsNullOrEmpty(id)) filter = filter & filterBuilder.Eq("Id", id);
            //create collection
            var collection = database.GetCollection<T>(collectionName);
            //get total documents
            var totalCount = await collection.CountDocumentsAsync(filter);
            //return if it exists
            return totalCount > 0;
        }
    }
}
