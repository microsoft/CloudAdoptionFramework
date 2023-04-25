using AzureNamingTool.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System.Net;
using AzureNamingTool.Services;
using AzureNamingTool.Attributes;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AzureNamingTool.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiKey]
    public class PolicyController : ControllerBase
    {
        //private ServiceResponse serviceResponse = new();
        // GET: api/<PolicyController>
        //[HttpGet]
        //[Route("[action]")]
        //public async Task<IActionResult> GetPolicyDefinition()
        //{
        //    try
        //    {
        //        serviceResponse = await PolicyService.GetPolicy();
        //        //MemoryStream stream = serviceResponse.ResponseObject;
        //        if (serviceResponse.Success)
        //        {
        //            var stream = new MemoryStream();
        //            var writer = new StreamWriter(stream);
        //            writer.Write(serviceResponse.ResponseObject);
        //            writer.Flush();
        //            stream.Position = 0;

        //            return File(stream,"application/json" , "namePolicyDefinition.json");
        //        }
        //        else
        //        {
        //            return BadRequest(serviceResponse.ResponseObject);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
        //        return BadRequest(ex);
        //    }
        //}
    }
}
