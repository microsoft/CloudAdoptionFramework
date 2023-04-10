using AzureNamingTool.Models;
using AzureNamingTool.Helpers;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AzureNamingTool.Services;
using AzureNamingTool.Attributes;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AzureNamingTool.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiKey]
    public class ResourceLocationsController : ControllerBase
    {
        private ServiceResponse serviceResponse = new();
        // GET: api/<ResourceLocationsController>
        /// <summary>
        /// This function will return the locations data. 
        /// </summary>
        /// <returns>json - Current locations data</returns>
        [HttpGet]
        public async Task<IActionResult> Get(bool admin = false)
        {
            try
            {
                serviceResponse = await ResourceLocationService.GetItems(admin);
                if (serviceResponse.Success)
                {
                    return Ok(serviceResponse.ResponseObject);
                }
                else
                {
                    return BadRequest(serviceResponse.ResponseObject);
                }
            }
            catch (Exception ex)
            {
                AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                return BadRequest(ex);
            }
        }

        // GET api/<ResourceLocationsController>/5
        /// <summary>
        /// This function will return the specifed location data.
        /// </summary>
        /// <param name="id">int - Location id</param>
        /// <returns>json - Location data</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                serviceResponse = await ResourceLocationService.GetItem(id);
                if (serviceResponse.Success)
                {
                    return Ok(serviceResponse.ResponseObject);
                }
                else
                {
                    return BadRequest(serviceResponse.ResponseObject);
                }
            }
            catch (Exception ex)
            {
                AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                return BadRequest(ex);
            }
        }

        //// POST api/<ResourceLocationsController>
        //[HttpPost]
        //public async Task<IActionResult> Post([FromBody] ResourceLocation item)
        //{
        //    try
        //    {
        //        serviceResponse = await ResourceLocationService.PostItem(item);
        //        if (serviceResponse.Success)
        //        {
        //            return Ok(serviceResponse.ResponseObject);
        //        }
        //        else
        //        {
        //            return BadRequest(serviceResponse.ResponseObject);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex);
        //    }
        //}

        // POST api/<ResourceLocationsController>
        /// <summary>
        /// This function will update all locations data.
        /// </summary>
        /// <param name="items">List - ResourceLocation (json) - All locations data</param>
        /// <returns>bool - PASS/FAIL</returns>
        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> PostConfig([FromBody] List<ResourceLocation> items)
        {
            try
            {
                serviceResponse = await ResourceLocationService.PostConfig(items);
                if (serviceResponse.Success)
                {
                    return Ok(serviceResponse.ResponseObject);
                }
                else
                {
                    return BadRequest(serviceResponse.ResponseObject);
                }
            }
            catch (Exception ex)
            {
                AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                return BadRequest(ex);
            }
        }

        //// DELETE api/<ResourceLocationsController>/5
        //[HttpDelete("{id}")]
        //public async Task<IActionResult> Delete(int id)
        //{
        //    try
        //    {
        //        serviceResponse = await ResourceLocationService.DeleteItem(id);
        //        if (serviceResponse.Success)
        //        {
        //            return Ok(serviceResponse.ResponseObject);
        //        }
        //        else
        //        {
        //            return BadRequest(serviceResponse.ResponseObject);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex);
        //    }
        //}
    }
}
