using AzureNamingTool.Attributes;
using AzureNamingTool.Helpers;
using AzureNamingTool.Models;
using AzureNamingTool.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AzureNamingTool.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiKey]
    public class ResourceDelimitersController : ControllerBase
    {
        private ServiceResponse serviceResponse = new();

        // GET api/<ResourceDelimitersController>
        /// <summary>
        /// This function will return the delimiters data.
        /// </summary>
        /// <param name="admin">bool - All/Only-enabled delimiters flag</param>
        /// <returns>json - Current delimiters data</returns>
        [HttpGet]
        public async Task<IActionResult> Get(bool admin = false)
        {
            try
            {
                serviceResponse = await ResourceDelimiterService.GetItems(admin);
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

        // GET api/<ResourceDelimitersController>/5
        /// <summary>
        /// This function will return the specifed resource delimiter data.
        /// </summary>
        /// <param name="id">int - Resource Delimiter id</param>
        /// <returns>json - Resource delimiter data</returns>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                // Get list of items
                serviceResponse = await ResourceDelimiterService.GetItem(id);
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



        // POST api/<ResourceDelimitersController>
        /// <summary>
        /// This function will create/update the specified delimiter data.
        /// </summary>
        /// <param name="item">ResourceDelimiter (json) - Delimiter data</param>
        /// <returns>bool - PASS/FAIL</returns>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ResourceDelimiter item)
        {
            try
            {
                serviceResponse = await ResourceDelimiterService.PostItem(item);
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

        // POST api/<resourcedelimitersController>
        /// <summary>
        /// This function will update all delimiters data.
        /// </summary>
        /// <param name="items">List - ResourceDelimiter (json) - All delimiters data</param>
        /// <returns>bool - PASS/FAIL</returns>
        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> PostConfig([FromBody] List<ResourceDelimiter> items)
        {
            try
            {
                serviceResponse = await ResourceDelimiterService.PostConfig(items);
                if (serviceResponse.Success)
                {
                    return Ok(serviceResponse.ResponseObject);
                }
                else
                {
                    return BadRequest(serviceResponse.ResponseObject);
                }
            }
            catch(Exception ex)
            {
                AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                return BadRequest(ex);
            }
        }
    }
}
