using PI.Utilities.Interfaces;

namespace PI.Data.Mongo.Models
{
    /// <summary>
    /// Class used to create a new client
    /// </summary>
    public class BaseNameProperties : IBaseNameProperties
    {
        /// <summary>
        /// Gets/sets the name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets/sets a quick search value for the object
        /// </summary>
        public virtual string QuickSearchValue { get; set; }

    }
}
