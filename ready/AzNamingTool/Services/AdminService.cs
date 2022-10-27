using AzureNamingTool.Helpers;
using AzureNamingTool.Models;
using Microsoft.AspNetCore.Mvc;
using System;

namespace AzureNamingTool.Services
{
    public class AdminService
    {
        private static ServiceResponse serviceResponse = new();
        private static Config config = GeneralHelper.GetConfigurationData();
        
        public static async Task<ServiceResponse> UpdatePassword(string password)
        {
            try
            {
                if (GeneralHelper.ValidatePassword(password))
                {
                    config.AdminPassword = GeneralHelper.EncryptString(password, config.SALTKey);
                    GeneralHelper.UpdateSettings(config);
                    serviceResponse.Success = true;
                }
                else
                {
                    serviceResponse.Success = false;
                    serviceResponse.ResponseObject = "The pasword does not meet the security requirements.";
                }
            }
            catch (Exception ex)
            {
                AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                serviceResponse.Success = false;
                serviceResponse.ResponseObject = ex;
            }
            return serviceResponse;
        }

        public static async Task<ServiceResponse> GenerateAPIKey()
        {
            try
            {
                // Set the new api key
                Guid guid = Guid.NewGuid();
                config.APIKey = GeneralHelper.EncryptString(guid.ToString(), config.SALTKey);
                GeneralHelper.UpdateSettings(config);
                serviceResponse.ResponseObject = guid.ToString();
                serviceResponse.Success = true;
            }
            catch (Exception ex)
            {
                AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                serviceResponse.Success = false;
                serviceResponse.ResponseObject = ex;
            }
            return serviceResponse;
        }

        public static async Task<ServiceResponse> UpdateAPIKey(string apikey)
        {
            try
            {
                config.APIKey = GeneralHelper.EncryptString(apikey, config.SALTKey);
                GeneralHelper.UpdateSettings(config);
                serviceResponse.ResponseObject = apikey;
                serviceResponse.Success = true;
            }
            catch (Exception ex)
            {
                AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                serviceResponse.Success = false;
                serviceResponse.ResponseObject = ex;
            }
            return serviceResponse;
        }
    }
}