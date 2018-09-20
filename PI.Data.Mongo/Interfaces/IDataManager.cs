using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PI.Data.Mongo.Interfaces
{
    public interface IDataManager
    {
        /// <summary>
        /// Flag to audit new items, set to false by default
        /// switch to True to audit new items as well as updated items
        /// </summary>
        bool AuditNewItems { get; set; }

        /// <summary>
        /// Flag to advise if full audit storage is applied for the data manager
        /// </summary>
        bool AuditByDefault { get; set; }
        
        /// <summary>
        /// A Dictionary to provide granular audit settings for individual methods
        /// </summary>
        Dictionary<string, string> AuditSettings { get; set; }

        /// <summary>
        /// Audits the sent item
        /// </summary>
        /// <typeparam name="T">The type of the object to audit</typeparam>
        /// <param name="item">The item to audit</param>
        /// <param name="auditCollectionName">The collection name for the audit</param>
        /// <param name="auditSettingsKey">Optional audit setting key to reference the AuditSettings dictionary</param>
        /// <param name="auditUser">Optional string for the audit user</param>
        Task AuditItem<T>(T item, string auditCollectionName, string auditSettingsKey = null, string auditUser = null);

    }
}
