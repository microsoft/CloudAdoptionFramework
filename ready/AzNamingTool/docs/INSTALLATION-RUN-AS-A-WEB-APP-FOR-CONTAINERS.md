[Overview](/ready/AzNamingTool/README.md) | [Installation](/ready/AzNamingTool/docs/INSTALLATION.md) | [Updating](/ready/AzNamingTool/docs/UPDATING.md) | [Using the API](/ready/AzNamingTool/docs/USINGTHEAPI.md) | [Release Notes](/ready/AzNamingTool/RELEASENOTES.md) | [Version History](/ready/AzNamingTool/docs/VERSIONHISTORY.md) | [FAQ](/ready/AzNamingTool/docs/FAQ.md) | [Contributors](/ready/AzNamingTool/docs/CONTRIBUTORS.md)

# Run as a Web App for Containers (App Service running a container)

* [Choosing an Installation Option](/ready/AzNamingTool/docs/INSTALLATION.md)
* [Overview]($overview)
* [Steps](#steps)
<br />
## Overview
The Azure Naming Tool requires persistent storage for the configuration files when run as a container. The following processes will explain how to create this volume for your Azure App Service Container. All configuration JSON files will be stored in the volume to ensure the configuration is persisted.
<br />
<br />

> <br />**NOTE:**<br />
> For many of the steps, a sample process is provided. However, there are many ways to accomplish each step.<br /><br />
## Steps
1. Scroll up to the top-left corner of this page.
2. Click on the **CloudAdoptionFramework** link to open the root of this repository.
3. Click the green **<>Code** button and select **Download ZIP**.
4. Open your Downloads folder using File Explorer.
5. Extract the contents of the ZIP archive.

> <br />**NOTE:**<br />
> Validate the project files extracted successfully and match the contents in the GitHub repository.<br /><br />

6. Open a **Command Prompt**.
7. Change the directory to the **AzNamingTool** folder. For example:

```cmd
cd .\Downloads\CloudAdoptionFramework-master\CloudAdoptionFramework-master\ready\AzNamingTool
```

8. Run the following **Docker command** to build the image:

```cmd
docker build -t azurenamingtool .
```
  
> <br />**NOTE:**<br />
> Ensure the '.' is included in the command<br /><br />
  
9. Create an Azure Container Registry: [Microsoft Docs reference](https://docs.microsoft.com/azure/container-registry/container-registry-get-started-portal#:~:text=%20Quickstart%3A%20Create%20an%20Azure%20container%20registry%20using,must%20log%20in%20to%20the%20registry...%20More%20)
10. Build and publish your image to the Azure Container Registry: [Microsoft Docs reference](https://docs.microsoft.com/azure/container-registry/container-registry-get-started-docker-cli?tabs=azure-cli)
11. Create an Azure Files file share for persistent storage: [Microsoft Docs reference](https://docs.microsoft.com/azure/storage/files/storage-how-to-create-file-share?tabs=azure-portal)
  
  ![FileShare](/ready/AzNamingTool/wwwroot/Screenshots/FileShare.png)

12. Create an Azure App Service - Web App: [Microsoft Docs reference](https://docs.microsoft.com/azure/app-service/quickstart-custom-container?tabs=dotnet&pivots=container-linux)
13. Mount the file share as local storage for the Azure App Service: [Microsoft Docs reference](https://docs.microsoft.com/azure/app-service/configure-connect-to-azure-storage?tabs=portal&pivots=container-linux)
  
  ![MountStorage](/ready/AzNamingTool/wwwroot/Screenshots/MountStorage.png)

14. Deploy the image from the Azure Container Registry to the Azure App Service: [Microsoft Docs reference](https://docs.microsoft.com/azure/app-service/deploy-ci-cd-custom-container?tabs=acr&pivots=container-linux)
15. Access the site using your Azure App Service URL.

> **NOTE:**
> It is recommended that you enable authentication on your Container App to prevent unauthorized access. [Authentication and authorization in Azure Container Apps](https://docs.microsoft.com/azure/container-apps/authentication)
