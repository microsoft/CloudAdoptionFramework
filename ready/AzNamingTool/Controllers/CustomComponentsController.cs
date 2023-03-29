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
using Microsoft.Extensions.Options;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AzureNamingTool.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiKey]
    public class CustomComponentsController : ControllerBase
    {
        private ServiceResponse serviceResponse = new();
        // GET: api/<CustomComponentsController>
        /// <summary>
        /// This function will return the custom components data. 
        /// </summary>
        /// <returns>json - Current custom components data</returns>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                // Get list of items
                serviceResponse = await CustomComponentService.GetItems();
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

        // GET api/<CustomComponentsController>/sample
        /// <summary>
        /// This function will return the custom components data for the specifc parent component type.
        /// </summary>
        /// <param name="parenttype">string - Parent Component Type Name</param>
        /// <returns>json - Current custom components data</returns>
        [Route("[action]/{parenttype}")]
        [HttpGet]
        public async Task<IActionResult> GetByParentType(string parenttype)
        {
            try
            {
                // Get list of items
                serviceResponse = await CustomComponentService.GetItemsByParentType(GeneralHelper.NormalizeName(parenttype, true));
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

        // GET api/<CustomComponentsController>/5
        /// <summary>
        /// This function will return the specifed custom component data.
        /// </summary>
        /// <param name="id">int - Custom Component id</param>
        /// <returns>json - Custom component data</returns>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                // Get list of items
                serviceResponse = await CustomComponentService.GetItem(id);
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

        // POST api/<CustomComponentsController>
        /// <summary>
        /// This function will create/update the specified custom component data.
        /// </summary>
        /// <param name="item">CustomComponent (json) - Custom component data</param>
        /// <returns>bool - PASS/FAIL</returns>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CustomComponent item)
        {
            try
            {
                serviceResponse = await CustomComponentService.PostItem(item);
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

        // POST api/<CustomComponentsController>
        /// <summary>
        /// This function will update all custom components data.
        /// </summary>
        /// <param name="items">List-CustomComponent (json) - All custom components data. (Legacy functionality).</param>
        /// <returns>bool - PASS/FAIL</returns>
        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> PostConfig([FromBody] List<CustomComponent> items)
        {
            try
            {
                serviceResponse = await CustomComponentService.PostConfig(items);
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

        // POST api/<CustomComponentsController>
        /// <summary>
        /// This function will update all custom components data.
        /// </summary>
        /// <param name="config">CustomComponmentConfig (json) - Full custom components data with parent component data.</param>
        /// <returns>bool - PASS/FAIL</returns>
        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> PostConfigWithParentData([FromBody] CustomComponmentConfig config)
        {
            try
            {
                List<ResourceComponent> currentresourcecomponents = new();
                List<CustomComponent> newcustomcomponents = new();
                // Get the current resource components
                serviceResponse = await ResourceComponentService.GetItems(true);
                if (serviceResponse.Success)
                {
                    currentresourcecomponents = serviceResponse.ResponseObject;

                    // Loop through the posted components
                    foreach (ResourceComponent thisparentcomponent in config.ParentComponents)
                    {
                        // Check if the posted component exists in the current components
                        if (!currentresourcecomponents.Exists(x => x.Name == thisparentcomponent.Name))
                        {
                            // Add the custom component
                            ResourceComponent newcustomcomponent = new()
                            {
                                Name = thisparentcomponent.Name,
                                DisplayName = thisparentcomponent.Name,
                                IsCustom = true
                            };
                            serviceResponse = await ResourceComponentService.PostItem(newcustomcomponent);

                            if (serviceResponse.Success)
                            {
                                // Add the new custom component to the list
                                currentresourcecomponents.Add(newcustomcomponent);
                            }
                            else
                            {
                                return BadRequest(serviceResponse.ResponseObject);
                            }
                        }
                    }

                    if (config.CustomComponents.Count > 0)
                    {
                        // Loop through custom components to make sure the parent exists
                        foreach (CustomComponent thiscustomcomponent in config.CustomComponents)
                        {
                            if (currentresourcecomponents.Where(x => GeneralHelper.NormalizeName(x.Name, true) == thiscustomcomponent.ParentComponent).Any())
                            {
                                newcustomcomponents.Add(thiscustomcomponent);
                            }
                        }

                        // Update the custom component options
                        serviceResponse = await CustomComponentService.PostConfig(newcustomcomponents);
                        if (!serviceResponse.Success)
                        {
                            return BadRequest(serviceResponse.ResponseObject);
                        }
                    }

                    return Ok("Custom Component configuration updated!");
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

        // DELETE api/<CustomComponentsController>/5
        /// <summary>
        /// This function will delete the specifed custom component data.
        /// </summary>
        /// <param name="id">int - Custom component id</param>
        /// <returns>bool - PASS/FAIL</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                serviceResponse = await CustomComponentService.DeleteItem(id);
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