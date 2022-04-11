using CatalogService.Business;
using CatalogService.Model;
using CatalogService.Model.Settings;
using CatalogService.Utilities.Cloudinary;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
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
        //private readonly ISovranLogger _logger;
        //private readonly ILogger<CatalogController> _logger;
        private readonly ICatalogHandler _handler;
        public CatalogController(ICatalogHandler handler, CatalogDatabaseSettings settings)
        {
            //_logger = logger;
            _handler = handler;
            _settings = settings;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="image"></param>
        /// <returns></returns>
        [HttpPost("insertMerchant")]
        public async Task<IActionResult> InsertMerchant([FromBody]CatalogEntry entry)
        {
            try
            {
                //await _logger.LogPayload(entry);


                var result = await _handler.InsertMerchant(entry);
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
                return StatusCode(500, ex.Message);
            }
        }

        [Route("updateMerchant")]
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

        [Route("pullCatalog")]
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

        [Route("addListing")]
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

        [Route("updateListing")]
        [HttpPost]
        public async Task<IActionResult> updateListing(string userName, CatalogItem updatedItem)
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


        [Route("deleteListing")]
        [HttpPost]
        public async Task<IActionResult> updateListing(string userName, string itemId)
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

        [Route("uploadImg")]
        [HttpPost]
        public async Task<IActionResult> uploadImg([FromBody] Image submission)
        {
            try
            {
                //string encoded;


                //using (var ms = new MemoryStream())
                //{
                //    encodedImg.CopyTo(ms);
                //    var fileBytes = ms.ToArray();
                //    encoded = Convert.ToBase64String(fileBytes);
                //}

                //byte[] bytes = Convert.FromBase64String(encoded);
                //string base64ImageRepresentation = Convert.ToBase64String(encodedImg);
                ImageHandler handler = new ImageHandler();
                var result = handler.PostImage(submission.EncodedImg, submission.Username, "profileImg");
                return Ok(result);
            }
            catch (Exception e)
            {
                return StatusCode(500);
            }
        }

        public class Image
        {
            public string EncodedImg { get; set; }
            public string Username { get; set; } 
        }
    }
}