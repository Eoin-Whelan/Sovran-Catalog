using CatalogService.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CatalogService.Business.StockScribe;

namespace CatalogService.Business
{
    /// <summary>
    /// Contract for CatalogHandler service.
    /// </summary>
    /// <see cref="CatalogHandler"/>
    public interface ICatalogHandler
    {
        //  Merchant
        public Task<bool> InsertMerchant(CatalogEntry newMerchant);
        public Task<bool> UpdateMerchant(string username, Dictionary<string, string> updatedMerchant);
        public Task<bool> RemoveMerchant(string userName);

        //  Catalog-Specific Methods
        public Task<CatalogEntry> RetrieveCatalog(string username);
        public Task<bool> InsertItem(string userName, CatalogItem newItem);
        public Task<bool> UpdateItem(string userName, CatalogItem updatedDetails);
        public Task<bool> DeleteItem(string userName, string ItemName);


    }
}
