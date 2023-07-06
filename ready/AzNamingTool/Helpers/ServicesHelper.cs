using AzureNamingTool.Models;
using AzureNamingTool.Services;

namespace AzureNamingTool.Helpers
{
    public class ServicesHelper
    {
        private static ServiceResponse serviceReponse = new();

        public static async Task<ServicesData> LoadServicesData(ServicesData servicesData, bool admin)
        {
            try
            {
                serviceReponse = await ResourceComponentService.GetItems(admin);
                servicesData.ResourceComponents = (List<ResourceComponent>)serviceReponse.ResponseObject;
                serviceReponse = await ResourceDelimiterService.GetItems(admin);
                servicesData.ResourceDelimiters = (List<ResourceDelimiter>)serviceReponse.ResponseObject;
                serviceReponse = await ResourceEnvironmentService.GetItems();
                servicesData.ResourceEnvironments = (List<ResourceEnvironment>)serviceReponse.ResponseObject;
                serviceReponse = await ResourceLocationService.GetItems(admin);
                servicesData.ResourceLocations = (List<ResourceLocation>)serviceReponse.ResponseObject;
                serviceReponse = await ResourceOrgService.GetItems();
                servicesData.ResourceOrgs = (List<ResourceOrg>)serviceReponse.ResponseObject;
                serviceReponse = await ResourceProjAppSvcService.GetItems();
                servicesData.ResourceProjAppSvcs = (List<ResourceProjAppSvc>)serviceReponse.ResponseObject;
                serviceReponse = await ResourceTypeService.GetItems(admin);
                servicesData.ResourceTypes = (List<ResourceType>)serviceReponse.ResponseObject;
                serviceReponse = await ResourceUnitDeptService.GetItems();
                servicesData.ResourceUnitDepts = (List<ResourceUnitDept>)serviceReponse.ResponseObject;
                serviceReponse = await ResourceFunctionService.GetItems();
                servicesData.ResourceFunctions = (List<ResourceFunction>)serviceReponse.ResponseObject;
                serviceReponse = await CustomComponentService.GetItems();
                servicesData.CustomComponents = (List<CustomComponent>)serviceReponse.ResponseObject;
                serviceReponse = await GeneratedNamesService.GetItems();
                servicesData.GeneratedNames = (List<GeneratedName>)serviceReponse.ResponseObject;
                serviceReponse = await AdminLogService.GetItems();
                servicesData.AdminLogMessages = (List<AdminLogMessage>)serviceReponse.ResponseObject;
                serviceReponse = await AdminUserService.GetItems();
                servicesData.AdminUsers = (List<AdminUser>)serviceReponse.ResponseObject;
                return servicesData;
            }
            catch(Exception ex)
            {
                AdminLogService.PostItem(new AdminLogMessage() { Title = "ERROR", Message = ex.Message });
                return servicesData;
            }
        }
    }
}
