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
using Microsoft.AspNetCore.Mvc.ModelBinding;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AzureNamingTool.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiKey]
    public class AdminController : ControllerBase
    {
        private ServiceResponse serviceResponse = new();
        private SiteConfiguration config = ConfigurationHelper.GetConfigurationData();

        // POST api/<AdminController>
        /// <summary>
        /// This function will update the Admin Password. 
        /// </summary>
        /// <param name="password">string - New Admin Password</param>
        /// <param name="adminpassword">string - Current Admin Password</param>
        /// <returns>string - Successful update</returns>
        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> UpdatePassword([BindRequired][FromHeader(Name = "AdminPassword")] string adminpassword, [FromBody] string password)
        {
            try
            {
                if (!String.IsNullOrEmpty(adminpassword))
                {
                    if (adminpassword == GeneralHelper.DecryptString(config.AdminPassword, config.SALTKey))
                    {
                        serviceResponse = await AdminService.UpdatePassword(password);
                        return (serviceResponse.Success ? Ok("SUCCESS"): Ok("FAILURE - There was a problem updating the password."));
                    }
                    else
                    {
                        return Ok("FAILURE - Incorrect Admin Password.");
                    }

                }
                else
                {
                    return Ok("FAILURE - You must provide teh Admin Password.");
                }
            }
            catch (Exception ex)
            {
                AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                return BadRequest(ex);
            }
        }

        // POST api/<AdminController>
        /// <summary>
        /// This function will update the API Key. 
        /// </summary>
        /// <param name="apikey">string - New API Key</param>
        /// <param name="adminpassword">string - Current Admin Password</param>
        /// <returns>dttring - Successful update</returns>
        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> UpdateAPIKey([BindRequired][FromHeader(Name = "AdminPassword")] string adminpassword, [FromBody] string apikey)
        {
            try
            {
                if (!String.IsNullOrEmpty(adminpassword))
                {
                    if (adminpassword == GeneralHelper.DecryptString(config.AdminPassword, config.SALTKey))
                    {
                        serviceResponse = await AdminService.UpdateAPIKey(apikey);
                        return (serviceResponse.Success ? Ok("SUCCESS") : Ok("FAILURE - There was a problem updating the API Key."));
                    }
                    else
                    {
                        return Ok("FAILURE - Incorrect Admin Password.");
                    }

                }
                else
                {
                    return Ok("FAILURE - You must provide teh Admin Password.");
                }
            }
            catch (Exception ex)
            {
                AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                return BadRequest(ex);
            }
        }

        // POST api/<AdminController>
        /// <summary>
        /// This function will generate a new API Key. 
        /// </summary>
        /// <param name="adminpassword">string - Current Admin Password</param>
        /// <returns>string - Successful update / Generated API Key</returns>

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> GenerateAPIKey([BindRequired][FromHeader(Name = "AdminPassword")] string adminpassword)
        {
            try
            {
                if (!String.IsNullOrEmpty(adminpassword))
                {
                    if (adminpassword == GeneralHelper.DecryptString(config.AdminPassword, config.SALTKey))
                    {
                        serviceResponse = await AdminService.GenerateAPIKey();
                        return (serviceResponse.Success ? Ok("SUCCESS") : Ok("FAILURE - There was a problem generating the API Key."));
                    }
                    else
                    {
                        return Ok("FAILURE - Incorrect Admin Password.");
                    }

                }
                else
                {
                    return Ok("FAILURE - You must provide teh Admin Password.");
                }
            }
            catch (Exception ex)
            {
                AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                return BadRequest(ex);
            }
        }

        /// <summary>
        /// This function will return the admin log data.
        /// </summary>
        /// <returns>json - Current admin log data</returns>
        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> GetAdminLog([BindRequired][FromHeader(Name = "AdminPassword")] string adminpassword)
        {
            try
            {
                serviceResponse = await AdminLogService.GetItems();
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

        /// <summary>
        /// This function will purge the admin log data.
        /// </summary>
        /// <returns>dttring - Successful operation</returns>
        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> PurgeAdminLog([BindRequired][FromHeader(Name = "AdminPassword")] string adminpassword)
        {
            try
            {
                serviceResponse = await AdminLogService.DeleteAllItems();
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

        /// <summary>
        /// This function will return the generated names data.
        /// </summary>
        /// <returns>json - Current generated names data</returns>
        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> GetGeneratedNamesLog()
        {
            try
            {
                serviceResponse = await GeneratedNamesService.GetItems();
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

        /// <summary>
        /// This function will purge the generated names data.
        /// </summary>
        /// <returns>dttring - Successful operation</returns>
        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> PurgeGeneratedNamesLog([BindRequired][FromHeader(Name = "AdminPassword")] string adminpassword)
        {
            try
            {
                serviceResponse = await GeneratedNamesService.DeleteAllItems();
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

        /// <summary>
        /// This function will reset the site configuration. THIS CANNOT BE UNDONE!
        /// </summary>
        /// <returns>dttring - Successful operation</returns>
        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> ResetSiteConfiguration([BindRequired][FromHeader(Name = "AdminPassword")] string adminpassword)
        {
            try
            {

                if (ConfigurationHelper.ResetSiteConfiguration())
                {
                    return Ok("Site configuration reset suceeded!");
                }
                else
                {
                    return BadRequest("Site configuration reset failed!");
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