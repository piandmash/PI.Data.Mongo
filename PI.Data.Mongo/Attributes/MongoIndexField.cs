using System;
using System.Collections.Generic;
using System.Text;

namespace PI.Data.Mongo.Attributes
{
    public enum IndexDirection
    {
        Ascending,
        Descending
    }

    public class MongoIndexField : Attribute
    {
        public MongoIndexField(string indexName, IndexDirection direction) : this(indexName, 0, direction)
        {
        }

        public MongoIndexField(string indexName) : this(indexName, 0, IndexDirection.Ascending)
        {
        }

        public MongoIndexField(string indexName, int columnIndex) : this(indexName, columnIndex, IndexDirection.Ascending)
        {

        }

        public MongoIndexField(string indexName, int columnIndex, IndexDirection direction)
        {

            Direction = direction;
            ColumnIndex = columnIndex;
            IndexName = indexName;

        }

        /// <summary>
        /// Matching index names are grouped as 1 index
        /// </summary>
        public string IndexName { get; set; }

        /// <summary>
        /// Direction of the index
        /// </summary>
        public IndexDirection Direction { get; set; } = IndexDirection.Ascending;

        /// <summary>
        /// The order of the column in the index. If not set, the fields will be ordered randomly
        /// </summary>
        public int ColumnIndex { get; set; }
    }
}
