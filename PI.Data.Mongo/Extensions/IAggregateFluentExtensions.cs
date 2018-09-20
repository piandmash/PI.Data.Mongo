using System;
using System.Reflection;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Misc;
using System.Linq.Expressions;

namespace PI.Data.Mongo.Extensions
{
    // <summary>
    /// Extension methods for <see cref="IAggregateFluent{TResult}"/>
    /// </summary>
    public static class IAggregateFluentExtensions
    {

        /// <summary>
        /// Appends a match stage to the pipeline.
        /// </summary>
        /// <remarks>
        /// Taken from the post on StackTrace @ https://stackoverflow.com/questions/45530988/mongodb-using-sample-with-c-sharp-driver
        /// </remarks>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="aggregate">The aggregate.</param>
        /// <param name="count">The sample count to retrieve.</param>
        /// <returns>
        /// The fluent aggregate interface.
        /// </returns>
        public static IAggregateFluent<TResult> Sample<TResult>(this IAggregateFluent<TResult> aggregate, int count)
        {
            Ensure.IsNotNull(aggregate, nameof(aggregate));
            var new_agg = aggregate.Skip(10);
            var stage = new_agg.Stages[new_agg.Stages.Count - 1];
            var newDoc = new MongoDB.Bson.BsonDocument {
                { "$sample", new MongoDB.Bson.BsonDocument {
                        {"size", count}
                    } }
            };
            stage.GetType().GetField("_document"
             , BindingFlags.Instance | BindingFlags.NonPublic)
                 .SetValue(stage, newDoc);

            return new_agg;
        }

        /// <summary>
        /// Appends a match stage to the pipeline.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="aggregate">The aggregate.</param>
        /// <param name="filter">The filter.</param>
        /// <returns>
        /// The fluent aggregate interface.
        /// </returns>
        public static IAggregateFluent<TResult> Match2<TResult>(this IAggregateFluent<TResult> aggregate, Expression<Func<TResult, bool>> filter)
        {
            Ensure.IsNotNull(aggregate, nameof(aggregate));
            return aggregate.AppendStage(PipelineStageDefinitionBuilder.Match(filter));
        }

        /// <summary>
        /// Appends a project stage to the pipeline.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="aggregate">The aggregate.</param>
        /// <param name="projection">The projection.</param>
        /// <returns>
        /// The fluent aggregate interface.
        /// </returns>
        public static IAggregateFluent<BsonDocument> ProjectPete<TResult>(this IAggregateFluent<TResult> aggregate, ProjectionDefinition<TResult, BsonDocument> projection)
        {
            Ensure.IsNotNull(aggregate, nameof(aggregate));
            return aggregate.AppendStage(PipelineStageDefinitionBuilder.Project(projection));
        }
    }
}
