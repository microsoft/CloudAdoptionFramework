[Overview](/ready/AzNamingTool/README.md) | [Installation](/ready/AzNamingTool/docs/INSTALLATION.md) | [Updating](/ready/AzNamingTool/docs/UPDATING.md) | [Using the API](/ready/AzNamingTool/docs/USINGTHEAPI.md) | [Release Notes](/ready/AzNamingTool/RELEASENOTES.md) | [Version History](/ready/AzNamingTool/docs/VERSIONHISTORY.md) | [FAQ](/ready/AzNamingTool/docs/FAQ.md) | [Contributors](/ready/AzNamingTool/docs/CONTRIBUTORS.md)

# Choosing An Installation Option
The Azure Naming Tool was designed to be deployed in nearly any environment. This includes as a stand-alone application, or as a container. Each deployment option offers pros/cons, depending on your environment and level of experience.<br />

> <br />**NOTE:**<br />
> The Azure Naming Tool requires persistent storage for the configuration files when run as a container. The following processes will explain how to create this volume in your respective environment. All configuration JSON files will be stored in the volume to ensure the configuration is persisted.<br /><br />
  
* [**Run as an Azure Web App Using GitHub Action**](/ready/AzNamingTool/docs/INSTALLATION-RUN-AS-AN-AZURE-WEB-APP-USING-GITHUB-ACTION.md) - **RECOMMENDED OPTION**
  * App Service running a .NET application
  * Ideal for fastest deployment
  * Requires an Azure Web App
  * Utilizes provided GitHub Action for deployment
  * Requires GitHub secrets (instructions in GitHub Action workflow file)
  * Integrated for continuous deployment from GitHub
<br /><br />

* [**Run as a Docker image**](/ready/AzNamingTool/docs/INSTALLATION-RUN-AS-A-DOCKER-IMAGE.md)
  * Ideal for local deployments
  * Requires a Docker Engine environment
  * Requires a storage volume mount
<br /><br />

* [**Run as a Docker image with podman**](/ready/AzNamingTool/docs/INSTALLATION-RUN-AS-A-DOCKER-IMAGE-WITH-PODMAN.md)
  * Ideal for local deployments
  * Requires a Podman environment
  * Requires a storage volume mount
<br /><br />

* [**Run as a Web App for Containers**](/ready/AzNamingTool/docs/INSTALLATION-RUN-AS-A-WEB-APP-FOR-CONTAINERS.md)
  * App Service running a container
  * Ideal for single container installations
  * Requires an Azure App Service
  * Requires an Azure Files file share for persistent storage
<br /><br />

* [**Run as an Azure Container App**](/ready/AzNamingTool/docs/INSTALLATION-RUN-AS-AN-AZURE-CONTAINER-APP.md)
  * Ideal for multiple container installations (integration with other containers, services, etc.)
  * Requires an Azure Container App
  * Requires an Azure Files file share for persistent storage
<br /><br />

* [**Run as a Stand-Alone Site**](/ready/AzNamingTool/docs/INSTALLATION-RUN-AS-A-STAND-ALONE-SITE.md)
  * Ideal for legacy deployments
  * Requires a web server (IIS, Apache, etc.)
  * Requires building/publshing the .NET Core application
