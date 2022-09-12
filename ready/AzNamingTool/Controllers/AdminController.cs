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
using Microsoft.AspNetCore.DataProtection.KeyManagement;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AzureNamingTool.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiKey]
    public class AdminController : ControllerBase
    {
        private ServiceResponse serviceResponse = new();
        private Config config = GeneralHelper.GetConfigurationData();

        // POST api/<AdminController>
        /// <summary>
        /// This function will update the admin password. 
        /// </summary>
        /// <param name="password">string - New admin password.</param>
        /// <returns>string - Successful update</returns>

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> UpdatePassword([FromBody] string password)
        {
            try
            {
                serviceResponse = await AdminService.UpdatePassword(password);
                if (serviceResponse.Success)
                {
                    return Ok("SUCCESS");
                }
                else
                {
                    return Ok("FAILURE - There was a problem updating the password.");
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogAdminMessage("ERROR", ex.Message);
                return BadRequest(ex);
            }
        }

        // POST api/<AdminController>
        /// <summary>
        /// This function will update the admin password. 
        /// </summary>
        /// <param name="apikey">string - New API Key.</param>
        /// <returns>dttring - Successful update</returns>

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> UpdateAPIKey([FromBody] string apikey)
        {
            try
            {
                serviceResponse = await AdminService.UpdateAPIKey(apikey);
                if (serviceResponse.Success)
                {
                    return Ok("SUCCESS");
                }
                else
                {
                    return Ok("FAILURE - There was a problem updating the API Key.");
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogAdminMessage("ERROR", ex.Message);
                return BadRequest(ex);
            }
        }

        // POST api/<AdminController>
        /// <summary>
        /// This function will update the admin password. 
        /// </summary>
        /// <param name="newpassword">string - New API Key.</param>
        /// <returns>string - Successful update</returns>

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> GenerateAPIKey()
        {
            try
            {
                serviceResponse = await AdminService.GenerateAPIKey();
                if (serviceResponse.Success)
                {
                    return Ok("SUCCESS - API Key: " + serviceResponse.ResponseObject);
                }
                else
                {
                    return Ok("FAILURE - There was a problem generating the API Key.");
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogAdminMessage("ERROR", ex.Message);
                return BadRequest(ex);
            }
        }

    }
}