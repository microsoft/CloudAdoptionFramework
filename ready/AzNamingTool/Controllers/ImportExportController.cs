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
    public class ImportExportController : ControllerBase
    {
        private ServiceResponse serviceResponse = new();
        // GET: api/<ImportExportController>
        /// <summary>
        /// This function will export the current configuration data (all components) as a single JSON file. 
        /// </summary>
        /// <returns>json - JSON configuration file</returns>
        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> ExportConfiguration(bool includeAdmin = false)
        {
            try
            {
                serviceResponse = await ImportExportService.ExportConfig(includeAdmin);
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

        // POST api/<ImportExportController>
        /// <summary>
        /// This function will import the provided configuration data (all components). This will overwrite the existing configuration data. 
        /// </summary>
        /// <param name="configdata">ConfigurationData (json) - Tool configuration File</param>
        /// <returns>bool - PASS/FAIL</returns>
        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> ImportConfiguration([FromBody] ConfigurationData configdata)
        {
            try
            {
                serviceResponse = await ImportExportService.PostConfig(configdata);
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
