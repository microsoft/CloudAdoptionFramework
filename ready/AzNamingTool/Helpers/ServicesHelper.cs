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
                serviceReponse = await ResourceLocationService.GetItems();
                servicesData.ResourceLocations = (List<ResourceLocation>)serviceReponse.ResponseObject;
                serviceReponse = await ResourceOrgService.GetItems();
                servicesData.ResourceOrgs = (List<ResourceOrg>)serviceReponse.ResponseObject;
                serviceReponse = await ResourceProjAppSvcService.GetItems();
                servicesData.ResourceProjAppSvcs = (List<ResourceProjAppSvc>)serviceReponse.ResponseObject;
                serviceReponse = await ResourceTypeService.GetItems();
                servicesData.ResourceTypes = (List<ResourceType>)serviceReponse.ResponseObject;
                serviceReponse = await ResourceUnitDeptService.GetItems();
                servicesData.ResourceUnitDepts = (List<ResourceUnitDept>)serviceReponse.ResponseObject;
                serviceReponse = await ResourceFunctionService.GetItems();
                servicesData.ResourceFunctions = (List<ResourceFunction>)serviceReponse.ResponseObject;
                return servicesData;
            }
            catch(Exception ex)
            {
                LogHelper.LogAdminMessage("ERROR", ex.Message);
                return servicesData;
            }
        }
    }
}
