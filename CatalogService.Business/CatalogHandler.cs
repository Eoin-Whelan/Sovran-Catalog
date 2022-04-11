using CatalogService.Model;
using CatalogService.Model.Settings;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CatalogService.Business
{
    public class CatalogHandler : ICatalogHandler
    {
        private readonly CatalogDatabaseSettings _settings;
        private readonly MongoClient _client;
        private readonly MongoClientSettings _clientSettings;
        private IMongoCollection<CatalogEntry> _catalogs;
        private IMongoDatabase _db;
        //ISovranLogger _logger;
        public CatalogHandler(CatalogDatabaseSettings settings)//, ISovranLogger logger)
        {
            _settings = settings;
            _clientSettings = MongoClientSettings.FromConnectionString("mongodb+srv://overseer:DunwallRats@sovranmerchantcatalog.r38ic.mongodb.net/sovran?retryWrites=true&w=majority");
            _clientSettings.ServerApi = new ServerApi(ServerApiVersion.V1);
            //_logger = logger;
        }

        public async Task<bool> InsertMerchant(CatalogEntry newMerchant)
        {
            var client = new MongoClient(_clientSettings);
            var database = client.GetDatabase("sovran");
            _catalogs = database.GetCollection<CatalogEntry>("catalogs");

            try
            {
                if (!DoesExist(newMerchant.userName))
                {
                    newMerchant.Id = ObjectId.GenerateNewId().ToString();
                    newMerchant.catalog[0].Id = ObjectId.GenerateNewId().ToString();
                    await _catalogs.InsertOneAsync(newMerchant);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<CatalogEntry> RetrieveCatalog(string username)
        {
            var client = new MongoClient(_clientSettings);
            var database = client.GetDatabase("sovran");
            _catalogs = database.GetCollection<CatalogEntry>("catalogs");

            CatalogEntry result = null;
            try
            {
                result = await _catalogs.Find<CatalogEntry>(c => c.userName.ToLower() == username.ToLower()).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return result;
        }

        public async Task<bool> InsertItem(string userName, CatalogItem newItem)
        {
            var client = new MongoClient(_clientSettings);
            var database = client.GetDatabase("sovran");
            _catalogs = database.GetCollection<CatalogEntry>("catalogs");

            try{
                var foundUser = await RetrieveCatalog(userName);
                if (foundUser != null)
                {
                    newItem.Id = ObjectId.GenerateNewId().ToString();
                    foundUser.catalog.Add(newItem);
                    var result = await _catalogs.ReplaceOneAsync(x => x.userName.ToLower() == userName.ToLower(), foundUser);
                    if (result.IsAcknowledged)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch(Exception ex)
            {
                return false;
            }
        }
        private bool DoesExist(string userName)
        {
            var client = new MongoClient(_clientSettings);
            var database = client.GetDatabase("sovran");
            _catalogs = database.GetCollection<CatalogEntry>("catalogs");
            try
            {
                var filter = new BsonDocument("userName", userName);
                var options = new ListCollectionNamesOptions { Filter = filter };

                return database.ListCollectionNames(options).Any();
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> UpdateMerchant(string userName, Dictionary<string, string> updatedDetails)
        {
            var client = new MongoClient(_clientSettings);
            var database = client.GetDatabase("sovran");
            _catalogs = database.GetCollection<CatalogEntry>("catalogs");
            List<UpdateResult> updateResults = new List<UpdateResult>();
            try
            {
                var foundUser = await RetrieveCatalog(userName);
                if (foundUser != null)
                {
                    var filter = Builders<CatalogEntry>.Filter.Eq("userName", userName);
                    foreach (KeyValuePair<string, string> entry in updatedDetails)
                    {
                        var update = Builders<CatalogEntry>.Update.Set(entry.Key, entry.Value);

                        updateResults.Add(_catalogs.UpdateOne(filter, update));
                    }
                    foreach(var result in updateResults)
                    {
                        if (!result.IsAcknowledged)
                        {
                            return false;
                        }
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public Task<bool> RemoveMerchant(string userName)
        {
            throw new NotImplementedException();
        }


        public async Task<bool> DeleteItem(string userName, string itemId)
        {
            var client = new MongoClient(_clientSettings);
            var database = client.GetDatabase("sovran");
            _catalogs = database.GetCollection<CatalogEntry>("catalogs");

            var foundUser = await RetrieveCatalog(userName);
            try
            {
                if (foundUser != null)
                {
                    var filter = Builders<CatalogEntry>.Filter.Eq(x => x.Id, foundUser.Id)
                        & Builders<CatalogEntry>.Filter.ElemMatch(x => x.catalog, Builders<CatalogItem>.Filter.Eq(x => x.Id, itemId));


                    var pullItem = Builders<CatalogEntry>
                        .Update.PullFilter(s => s.catalog, x => x.Id == itemId);

                    var result = await _catalogs.UpdateOneAsync(filter, pullItem);
                    if (result.IsAcknowledged)
                    {
                        return true;
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {

            }
            return false;
        }

        public async Task<bool> MarkItemAsUnavailable(string userName, string itemId)
        {
            var client = new MongoClient(_clientSettings);
            var database = client.GetDatabase("sovran");
            _catalogs = database.GetCollection<CatalogEntry>("catalogs");

            var foundUser = await RetrieveCatalog(userName);
            try
            {
                if (foundUser != null)
                {
                    var filter = Builders<CatalogEntry>.Filter.Eq(x => x.Id, foundUser.Id)
                        & Builders<CatalogEntry>.Filter.ElemMatch(x => x.catalog, Builders<CatalogItem>.Filter.Eq(x => x.Id, itemId));
                    var update = Builders<CatalogEntry>.Update.Set(x => x.catalog[-1].IsDeleted, true);

                    var result = await _catalogs.UpdateOneAsync(filter, update);

                    if (result.IsAcknowledged)
                    {
                        return true;
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {

            }
            return false;
        }

        public async Task<bool> UpdateItem(string userName, CatalogItem newItemDetails)
        {
            var client = new MongoClient(_clientSettings);
            var database = client.GetDatabase("sovran");
            _catalogs = database.GetCollection<CatalogEntry>("catalogs");

            var foundUser = await RetrieveCatalog(userName);
            try
            {
                if (foundUser != null)
                {
                    var filter = Builders<CatalogEntry>.Filter.Eq(x => x.Id, foundUser.Id)
                        & Builders<CatalogEntry>.Filter.ElemMatch(x => x.catalog, Builders<CatalogItem>.Filter.Eq(x => x.Id, newItemDetails.Id));
                    var update = Builders<CatalogEntry>.Update.Set(x => x.catalog[-1], newItemDetails);

                    var result = await _catalogs.UpdateOneAsync(filter, update);
                    if (result.IsAcknowledged)
                    {
                        return true;
                    }
                    return false;
                }
            }
            catch(Exception ex)
            {

            }
            return false;
        }
    }
}
