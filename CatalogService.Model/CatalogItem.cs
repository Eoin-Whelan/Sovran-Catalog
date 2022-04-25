using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CatalogService.Model
{
    /// <summary>
    /// Class representing given Catalog item within a merchant's catalog document.
    /// </summary>
    public class CatalogItem
    {
        [Required]
        public string itemName { get; set; }
        [Required]
        public decimal itemPrice { get; set; }
        [Required]
        public Dictionary<string, int> itemQty { get; set; }
        [Required]
        public string itemDesc { get; set; }
        public string? itemImg { get; set; }
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        [Required]
        public bool IsDeleted { get; set; } = false;
    }
}
