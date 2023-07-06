[Overview](/ready/AzNamingTool/README.md) | [Installation](/ready/AzNamingTool/docs/INSTALLATION.md) | [Updating](/ready/AzNamingTool/docs/UPDATING.md) | [Using the API](/ready/AzNamingTool/docs/USINGTHEAPI.md) | [Release Notes](/ready/AzNamingTool/RELEASENOTES.md) | [Version History](/ready/AzNamingTool/docs/VERSIONHISTORY.md) | [FAQ](/ready/AzNamingTool/docs/FAQ.md) | [Contributors](/ready/AzNamingTool/docs/CONTRIBUTORS.md)

# Run as an Azure Container App

* [Choosing an Installation Option](/ready/AzNamingTool/docs/INSTALLATION.md)
* [Overview]($overview)
* [Steps](#steps)
<br />
## Overview
The Azure Naming Tool requires persistent storage for the configuration files when run as a container. The following processes will explain how to create this volume for your Azure App Service Container. All configuration JSON files will be stored in the volume to ensure the configuration is persisted.

> <br />**NOTE:**<br />
> For many of the steps, a sample process is provided, however, there are many ways to accomplish each step.<br /><br />
## Steps
1. Scroll up to the top, left corner of this page.
2. Click on the **CloudAdoptionFramework** link to open the root of this repository.
3. Click the green **<>Code** button and select **Download ZIP**.
4. Open your Downloads folder using File Explorer.
5. Extract the contents of the ZIP archive.

> <br />**NOTE:**<br />
> Validate the project files extracted successfully and match the contents in the GitHub repository.<br /><br />

6. Open a **Command Prompt**
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

12. Create an Azure Container App: [Quickstart: Deploy an existing container image with the Azure CLI](https://docs.microsoft.com/azure/container-apps/get-started-existing-container-image?tabs=bash&pivots=container-apps-public-registry)

> <br />**NOTE:**<br />
> It is possible to deploy a container app via the portal, however, setting the volume for persistent storage is much easier using the CLI.<br /><br />
  
13. Configure the Azure Container App to use an Azure Files file share for the volume: [Use storage mounts in Azure Container Apps](https://docs.microsoft.com/azure/container-apps/storage-mounts?pivots=aca-cli#azure-files)
15. Access the site using your Azure App Service URL.

> <br />**NOTE:**<br />
> It is recommended that you enable authentication on your Container App to prevent unauthorized access. [Authentication and authorization in Azure Container Apps](https://docs.microsoft.com/azure/container-apps/authentication)<br /><br />
