using AzureNamingTool.Models;
using AzureNamingTool.Helpers;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Text;
using System.Collections;
using System.Threading;
using AzureNamingTool.Services;
using AzureNamingTool.Attributes;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AzureNamingTool.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiKey]
    public class ResourceNamingRequestsController : ControllerBase
    {
        // POST api/<ResourceNamingRequestsController>
        /// <summary>
        /// This function will generate a resoure type name for specifed component values. This function requires full definition for all components. It is recommended to use the RequestName API function for name generation.   
        /// </summary>
        /// <param name="request">ResourceNameRequestWithComponents (json) - Resource Name Request data</param>
        /// <returns>string - Name generation response</returns>
        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> RequestNameWithComponents([FromBody] ResourceNameRequestWithComponents request)
        {
            try
            {
                ResourceNameResponse resourceNameRequestResponse = await ResourceNamingRequestService.RequestNameWithComponents(request);

                if (resourceNameRequestResponse.Success)
                {
                    return Ok(resourceNameRequestResponse);
                }
                else
                {
                    return BadRequest(resourceNameRequestResponse);
                }
            }
            catch (Exception ex)
            {
                AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                return BadRequest(ex.Message);
            }
        }

        // POST api/<ResourceNamingRequestsController>
        /// <summary>
        /// This function will generate a resoure type name for specifed component values, using a simple data format.  
        /// </summary>
        /// <param name="request">ResourceNameRequest (json) - Resource Name Request data</param>
        /// <returns>string - Name generation response</returns>
        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> RequestName([FromBody] ResourceNameRequest request)
        {
            try
            {
                request.CreatedBy = "API";
                ResourceNameResponse resourceNameRequestResponse = await ResourceNamingRequestService.RequestName(request);

                if (resourceNameRequestResponse.Success)
                {
                    return Ok(resourceNameRequestResponse);
                }
                else
                {
                    return BadRequest(resourceNameRequestResponse);
                }
            }
            catch (Exception ex)
            {
                AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                return BadRequest(ex.Message);
            }
        }
    }
}
