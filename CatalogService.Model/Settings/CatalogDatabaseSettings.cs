using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatalogService.Model.Settings
{
    public class CatalogDatabaseSettings
    {
        public string ConnectionString { get; set; } = null!;
        public string DatabaseName { get; set; } = null!;
        public string CatalogsCollectionName { get; set; } = null!;

        /*
        public CatalogDatabaseSettings(string conn, string database, string catalog)
        {
            ConnectionString = conn;
            DatabaseName = database;
            CatalogsCollectionName = catalog;
        }*/
    }
}
