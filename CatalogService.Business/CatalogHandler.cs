using CatalogService.Model;
using CatalogService.Model.Settings;
using CatalogService.Utilities.Cloudinary;
using MongoDB.Bson;
using MongoDB.Driver;
using Sovran.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CatalogService.Business
{
    /// <summary>
    /// Primary handler class for all but one CRUD operation on catalog service.
    /// </summary>
    public class CatalogHandler : ICatalogHandler
    {
        private readonly CatalogDatabaseSettings _settings;
        private readonly MongoClient _client;
        private readonly MongoClientSettings _clientSettings;
        private IMongoCollection<CatalogEntry> _catalogs;
        private IMongoDatabase _db;
        private readonly ISovranLogger _logger;
        private readonly IImageHandler _imageHandler;

        /// <summary>
        /// Receives CatalogDb config class and Sovran logger through dependency injection.
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="logger"></param>
        public CatalogHandler(CatalogDatabaseSettings settings, ISovranLogger logger, IImageHandler imageHandler)
        {
            _settings = settings;
            _clientSettings = MongoClientSettings.FromConnectionString(settings.ConnectionString);
            _clientSettings.ServerApi = new ServerApi(ServerApiVersion.V1);
            _logger = logger;
            _imageHandler = imageHandler;
        }

        /// <summary>
        /// Inserts new merchant catalog. Used during registration flow.
        /// </summary>
        /// <param name="newMerchant">The new merchant.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        public async Task<bool> InsertMerchant(CatalogEntry newMerchant)
        {
            _logger.LogActivity("Initiating insertion flow for " + newMerchant.userName);
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
                    _logger.LogActivity("Merchant already exists: " + newMerchant.userName);
                    return false;
                }
            }
            catch(Exception ex)
            {
                _logger.LogError("INSERTION FLOW EXCEPTION: " + ex.Message);
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Retrieves a given merchant catalog.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <returns></returns>
        public async Task<CatalogEntry> RetrieveCatalog(string username)
        {
            _logger.LogActivity("Initiating insertion flow for " + username);
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
                _logger.LogError("Exception caught in catalog retrieval: " + ex.Message);
            }
            return result;
        }

        /// <summary>
        /// Inserts new catalog item to the catalog of a given merchant.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="newItem">The new item.</param>
        /// <returns></returns>
        public async Task<bool> InsertItem(string userName, CatalogItem newItem)
        {
            _logger.LogActivity("Initiating item insertion flow for " + userName);
            try
            {
                if(newItem.itemImg != null)
                {
                    var itemImageUrl = _imageHandler.PostProductImg(newItem.itemImg, userName, newItem.itemName);
                    newItem.itemImg = itemImageUrl;
                }
                var client = new MongoClient(_clientSettings);
                var database = client.GetDatabase("sovran");
                _catalogs = database.GetCollection<CatalogEntry>("catalogs");
                var foundUser = await RetrieveCatalog(userName);
                if (foundUser != null)
                {
                    newItem.Id = ObjectId.GenerateNewId().ToString();
                    foundUser.catalog.Add(newItem);
                    var result = await _catalogs.ReplaceOneAsync(x => x.userName.ToLower() == userName.ToLower(), foundUser);
                    if (result.IsAcknowledged)
                    {
                        _logger.LogActivity("Item insertion successful | Username: " + userName + " | Item " + newItem.Id);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    _logger.LogError("Item insertion unsuccessful. User not found: " + userName);
                    return false;
                }
            }
            catch(Exception ex)
            {
                return false;
            }
        }
        /// <summary>
        /// Private method to verify if a given username exists. Used in deterring duplicate registration.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Updates a given merchant with new details passed. Used in Update flow from Account service.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="updatedDetails">The updated details.</param>
        /// <returns></returns>
        public async Task<bool> UpdateMerchant(string userName, Dictionary<string, string> updatedDetails)
        {
            _logger.LogActivity("Initiating merchant details update flow for " + userName);
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
                    _logger.LogError(userName + " not found.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception caught in merchant details update flow.");
                return false;
            }
        }

        /// <summary>
        /// Current unimplemented merchant removal. No delete functionality exists within this service.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public Task<bool> RemoveMerchant(string userName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Deletes a given item from a merchant's catalog. Searches by Id.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="itemId">The item identifier.</param>
        /// <returns></returns>
        public async Task<bool> DeleteItem(string userName, string itemId)
        {
            _logger.LogActivity("Initiating merchant item deletion flow for " + userName);

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

        /// <summary>
        /// Allows for an item to be marked as not for sale. Not used in any current use case flow.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="itemId">The item identifier.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Updates the a given item's details, using a given username and item id to late and update accordingly..
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="newItemDetails">The new item details.</param>
        /// <returns></returns>
        public async Task<bool> UpdateItem(string userName, CatalogItem newItemDetails)
        {
            _logger.LogActivity("Initiating merchant catalog item detail update flow for " + userName + ". Item being updated: " + newItemDetails.itemName);
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
                        _logger.LogActivity("Merchant catalog item detail update complete.");
                        return true;
                    }
                    _logger.LogActivity("Update nack. Result dump:" + result.ToString());
                    return false;
                }
            }
            catch(Exception ex)
            {
                _logger.LogError("Exception encountered during item update. Ex: " + ex.Message);
            }
            return false;
        }
    }
}
