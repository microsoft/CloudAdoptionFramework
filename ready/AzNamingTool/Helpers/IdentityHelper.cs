using AzureNamingTool.Models;
using AzureNamingTool.Services;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System;

namespace AzureNamingTool.Helpers
{
    public class IdentityHelper
    {
        public static async Task<bool> IsAdminUser(StateContainer state, ProtectedSessionStorage session, string name)
        {
            bool result = false;
            try
            {
                // Check if the username is in the list of Admin Users
                ServiceResponse serviceResponse = await AdminUserService.GetItems();
                if (serviceResponse.Success)
                {
                    List<AdminUser> adminusers = serviceResponse.ResponseObject;
                    if (adminusers.Exists(x => x.Name.ToLower() == name.ToLower()))
                    {
                        state.SetAdmin(true);
                        await session.SetAsync("admin", true);
                        result = true;
                    }
                }
            }
            catch (Exception ex)
            {
                AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
            }
            return result;
        }

        public static async Task<string> GetCurrentUser(ProtectedSessionStorage session)
        {
            string currentuser = "System";
            try
            {
                var currentuservalue = await session.GetAsync<string>("currentuser");
                if (!String.IsNullOrEmpty(currentuservalue.Value))
                {
                    currentuser = currentuservalue.Value;
                }
            }
            catch (Exception ex)
            {
                AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
            }
            return currentuser;
        }
    }
}
