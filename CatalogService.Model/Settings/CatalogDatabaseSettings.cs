using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatalogService.Model.Settings
{
    /// <summary>
    /// Wrapper class for catalog settings used in CatalogHandler class.
    /// </summary>
    public class CatalogDatabaseSettings
    {
        /// <summary>
        /// Gets or sets the connection string.
        /// </summary>
        /// <value>
        /// Raw connection string for MongoDb.
        /// </value>
        public string ConnectionString { get; set; } = null!;
        /// <summary>
        /// Gets or sets the name of the database.
        /// </summary>
        /// <value>
        /// The name of the database for accessing.
        /// </value>
        public string DatabaseName { get; set; } = null!;
        /// <summary>
        /// Gets or sets the name of the catalogs collection.
        /// </summary>
        /// <value>
        /// Name of document collection within aforementioned database for accessing.
        /// </value>
        public string CatalogsCollectionName { get; set; } = null!;
    }
}
