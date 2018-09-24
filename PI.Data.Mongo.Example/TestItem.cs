using System;
using System.Collections.Generic;
using System.Text;

using PI.Data.Mongo.Models;

namespace PI.Data.Mongo.Example
{
    public class TestItem : BaseIdWithQuickSearchModel
    {

        /// <summary>
        /// Gets/sets the name of the client
        /// </summary>
        public string Name { get; set; }

    }
}
