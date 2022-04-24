using CatalogService.Business;
using CatalogService.Model;
using CatalogService.Model.Settings;
using CatalogService.Utilities.Cloudinary;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Sovran.Logger;
using System.Text.Json;

namespace CatalogService.Controllers
{
    /// <summary>
    /// CatalogController is an API for all Catalog service business logic.
    /// 
    /// This functionality extends to:
    /// - Inserting a new catalog (Registration flow)
    /// - Inserting a new item for a given merchant.
    /// - Update an existing item's details.
    /// - Updating a merchant's public details (e.g.Support information)
    /// - Retrieving a given merchant's entire catalog (Browsing purposes).
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class CatalogController : ControllerBase
    {
        private readonly CatalogDatabaseSettings _settings;
        private readonly ISovranLogger _logger;
        private readonly ICatalogHandler _handler;
        public CatalogController(ISovranLogger logger, ICatalogHandler handler, CatalogDatabaseSettings settings)
        {
            _logger = logger;
            _handler = handler;
            _settings = settings;
        }


        /// <summary>
        /// InsertMerchant - Registration flow API call. Inserts new catalog with single item.
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        [HttpPost("/Catalog/InsertMerchant")]
        public async Task<IActionResult> InsertMerchant([FromBody]CatalogEntry entry)
        {
            try
            {
                _logger.LogPayload(entry);
                _logger.LogActivity("Initializing registration flow. Username: " + entry.userName);

                var result = await _handler.InsertMerchant(entry);
                if (result)
                {
                    _logger.LogActivity("Successful registration flow. Username: " + entry.userName);
                    JsonResult response = new JsonResult(result);
                    return Ok(response);
                }
                else
                {
                    _logger.LogActivity("Unsuccessful registration flow. Username: " + entry.userName);
                    return BadRequest();
                    return StatusCode(500);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception caught. Ex: "+ ex.Message);
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// UpdateMerchant - Update flow API call. Updates a given merchant with a dictionary of key/value details.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="updatedDetails">The updated details.</param>
        /// <returns></returns>
        [Route("/Catalog/UpdateMerchant")]
        [HttpPost]
        public async Task<IActionResult> updateMerchant(string userName, Dictionary<string, string> updatedDetails)
        {
            try
            {
                var result = await _handler.UpdateMerchant(userName, updatedDetails);
                if (result)
                {
                    JsonResult response = new JsonResult(result);
                    return Ok(response);
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception e)
            {
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Pulls a given username's catalog for storefront display.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <returns></returns>
        [Route("/Catalog/PullCatalog")]
        [HttpGet]
        public async Task<IActionResult> pullCatalog(string username)
        {
            try
            {

                var result = await _handler.RetrieveCatalog(username);
                if(result != null)
                {
                    JsonResult response = new JsonResult(result);
                    return Ok(response);
                }
                else
                {
                    return BadRequest("No user matching request");
                }
            }
            catch(Exception ex)
            {
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Adds a new product listing to a given merchant's catalog.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="newItem">The new item.</param>
        /// <returns></returns>
        [Route("/Catalog/AddListing")]
        [HttpPost]
        public async Task<IActionResult> addListing(string username, CatalogItem newItem)
        {
            try
            {

                var result = await _handler.InsertItem(username, newItem);
                if (result)
                {
                    JsonResult response = new JsonResult(result);
                    return Ok(response);
                }
                else
                {
                    return StatusCode(500);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Updates an existing listing with new details.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="updatedItem">The updated item.</param>
        /// <returns></returns>
        [Route("/Catalog/UpdateListing")]
        [HttpPost]
        public async Task<IActionResult> UpdateListing(string userName, CatalogItem updatedItem)
        {
            try
            {
                var result = await _handler.UpdateItem(userName, updatedItem);
                return Ok();
            }
            catch(Exception e)
            {
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Deletes an existing listing from a given merchant's catalog.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="itemId">The item identifier.</param>
        /// <returns></returns>
        [Route("/Catalog/DeleteListing")]
        [HttpPost]
        public async Task<IActionResult> DeleteListing(string userName, string itemId)
        {
            try
            {
                var result = await _handler.DeleteItem(userName, itemId);
                return Ok();
            }
            catch (Exception e)
            {
                return StatusCode(500);
            }
        }
    }
}