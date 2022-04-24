using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatalogService.Model
{
    /// <summary>
    /// Used as an ORM-level mapping of a given catalog entry on MongoDb
    /// </summary>
    public class CatalogEntry
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        [BsonRequired]
        public string userName { get; set; }
        [BsonRequired]
        public string email { get; set; }
        public string? instagram { get; set; }
        public string? twitter { get; set; }

        [BsonRequired]
        public List<CatalogItem> catalog { get; set; }
        public string? profileImg { get; set; }
    }
}
