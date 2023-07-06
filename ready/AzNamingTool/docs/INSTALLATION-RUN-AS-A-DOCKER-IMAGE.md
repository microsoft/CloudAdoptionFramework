[Overview](/ready/AzNamingTool/README.md) | [Installation](/ready/AzNamingTool/docs/INSTALLATION.md) | [Updating](/ready/AzNamingTool/docs/UPDATING.md) | [Using the API](/ready/AzNamingTool/docs/USINGTHEAPI.md) | [Release Notes](/ready/AzNamingTool/RELEASENOTES.md) | [Version History](/ready/AzNamingTool/docs/VERSIONHISTORY.md) | [FAQ](/ready/AzNamingTool/docs/FAQ.md) | [Contributors](/ready/AzNamingTool/docs/CONTRIBUTORS.md)

#  Run as a Docker Image
* [Choosing an Installation Option](/ready/AzNamingTool/docs/INSTALLATION.md)
* [Overview]($overview)
* [Steps](#steps)

## Overview
This process will allow you to deploy the Azure Naming Tool using Docker in your local environment.
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

9. Run the following **Docker command** to create a new container and mount a new volume:

```cmd
docker run -d -p 8081:80 --mount source=azurenamingtoolvol,target=/app/settings azurenamingtool:latest
```

> <br />**NOTES:**  <br />
> * Substitute 8081 for any port not in use on your machine
> * You may see warnings in the command prompt regarding DataProtection and keys. These indicate that the keys are not persisted and are only local to the container instance.<br /><br />

10. Access the site using the following URL: *http://localhost:8081*
  
> <br />**NOTE:**<br />
> Substitute 8081 for the port you used in the docker run command<br /><br />
