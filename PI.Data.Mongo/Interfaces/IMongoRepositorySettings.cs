namespace PI.Data.Mongo.Interfaces
{
    public interface IMongoRepositorySettings
    {
        /// <summary>
        /// Gets/Sets the connection string
        /// </summary>
        string ConnectionString { get; set; }

        /// <summary>
        /// Gets/sets the database name
        /// </summary>
        string DatabaseName { get; set; }

        /// <summary>
        /// Flag to log, set to true to log
        /// </summary>
        bool Log { get; set; }

        /// <summary>
        /// Flag to advise if the store has indexing disabled, defaults to false
        /// </summary>
        bool IndexingDisabled { get; set; }
                
        /// <summary>
        /// Gets/sets the conventions pack name
        /// </summary>
        string ConventionsPackName { get; set; }
    }
}
