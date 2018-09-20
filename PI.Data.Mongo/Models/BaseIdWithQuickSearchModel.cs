using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System;
using PI.Utilities.Models;
using PI.Utilities.Interfaces;

namespace PI.Data.Mongo.Models
{
    /// <summary>
    /// Basic implementation used by all models requiring an id
    /// </summary>
    public abstract class BaseIdWithQuickSearchModel : BaseIdModel, IAuditFields, IAuditFieldsWithId, IQuickSearchValue
    {

        /// <summary>
        /// Gets/sets a quick search value for the object
        /// </summary>
        public virtual string QuickSearchValue { get; set; }

    }
}
