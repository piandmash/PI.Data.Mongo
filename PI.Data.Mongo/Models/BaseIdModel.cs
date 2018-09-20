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
    public abstract class BaseIdModel : BaseAuditModel, IAuditFields, IAuditFieldsWithId
    {

        private string _id = ObjectId.GenerateNewId().ToString();
        /// <summary>
        /// Gets/sets the object id
        /// </summary>
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonId(IdGenerator = typeof(ObjectIdGenerator))]
        public string Id { get { return _id; } set
            {
                ObjectId id;
                if(! ObjectId.TryParse(value, out id)) throw new Exception("You need to set the id as an ObjectId") ;
                _id = id.ToString();
            }
        }
        

    }
}
