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
    public class ResourceFunctionsController : ControllerBase
    {
        private ServiceResponse serviceResponse = new();
        // GET: api/<ResourceFunctionsController>
        /// <summary>
        /// This function will return the functions data. 
        /// </summary>
        /// <returns>json - Current functions data</returns>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                // Get list of items
                serviceResponse = await ResourceFunctionService.GetItems();
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

        // GET api/<ResourceFunctionsController>/5
        /// <summary>
        /// This function will return the specifed function data.
        /// </summary>
        /// <param name="id">int - Function id</param>
        /// <returns>json - Function data</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                serviceResponse = await ResourceFunctionService.GetItem(id);
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

        // POST api/<ResourceFunctionsController>
        /// <summary>
        /// This function will create/update the specified function data.
        /// </summary>
        /// <param name="item">ResourceFunction (json) - Function data</param>
        /// <returns>bool - PASS/FAIL</returns>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ResourceFunction item)
        {
            try
            {
                serviceResponse = await ResourceFunctionService.PostItem(item);
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

        // POST api/<ResourceFunctionsController>
        /// <summary>
        /// This function will update all functions data.
        /// </summary>
        /// <param name="items">List - ResourceFunction (json) - All functions data</param>
        /// <returns>bool - PASS/FAIL</returns>
        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> PostConfig([FromBody] List<ResourceFunction> items)
        {
            try
            {
                serviceResponse = await ResourceFunctionService.PostConfig(items);
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

        // DELETE api/<ResourceFunctionsController>/5
        /// <summary>
        /// This function will delete the specifed function data.
        /// </summary>
        /// <param name="id">int - Function id</param>
        /// <returns>bool - PASS/FAIL</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                serviceResponse = await ResourceFunctionService.DeleteItem(id);
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
    }
}