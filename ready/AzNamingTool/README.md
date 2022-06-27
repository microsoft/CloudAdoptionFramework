# Azure Naming Tool v2

<img src="./wwwroot/images/AzureNamingToolLogo.png?raw=true" alt="Azure Naming Tool" title="Azure Naming Tool" height="150"/>

[Overview](#overview)

[Project Structure](#project-structure)

[Important Notes](#important-notes)

[Pages](#pages)

[How To Install](#how-to-install)

* [Run as a Docker image](#run-as-a-docker-image)  

https://user-images.githubusercontent.com/106343797/171671289-15fde526-30ab-443c-80da-1367ce13abc0.mp4

* [Run as an Azure App Service Container](#run-as-an-azure-app-service-container)

## Overview

The Naming Tool was developed using a naming pattern based on [Microsoft's best practices](https://docs.microsoft.com/en-us/azure/cloud-adoption-framework/ready/azure-best-practices/naming-and-tagging). Once the organizational components have been defined by an administrator, users can use the tool to generate a name for the desired Azure resource.

## Project Structure

The Azure Naming Tool is a .NET 6 Blazor application, with a RESTful API. The UI consists of several pages to allow the configuration and generation of Azure Resource names. The API provides a programmatic interface for the functionality. The application contains Docker support, allowing the site to be run as a stand-alone application, or a container.

### Project Components

* UI/Admin
* API
* JSON configuration files
* Dockerfile

### Important Notes

The following are important notes/aspects of the Azure Naming Tool:

* The application is designed to run as a stand-alone solution, with no internet/Azure connection.
* The application can be run as a .NET 6 site, or as a Docker container.
* The site can be hosted in any environment, including internal or in a public/private cloud.
* The application uses local JSON files to store the configuration of the components.
* The application requires persistent storage. If running as a container, a volume is required to store configuration files.
* The application contains a *repository* folder, which contains the default component configuration JSON files. When deployed, these files are copied to the *settings* folder.
* The Admin interface allows configurations to be "reset", if needed. This process copies the configuration from the *repository* folder to the *settings* folder.
* The API requires an API Key for all executions. A default API Key (guid) will be generated on first launch. This value can be updated in the Admin section.
* On first launch, the application will prompt for the Admin password to be set.

  ![Admin Password Prompt](./wwwroot/Screenshots/AdminPasswordPrompt.png)

## Pages

### Home Page

The Home Page provides an overview of the tool and the components.

![Home Page](./wwwroot/Screenshots/HomePage.png)

### Configuration

The Configuration Page shows the current Name Generation configuration. This page also provides an Admin section for updating the configuration.

![Configuration Page](./wwwroot/Screenshots/ConfigurationPage.png)

### Reference

The Reference Page provides examples for each type of Azure resource. The example values do not include any excluded naming components. Optional components are always displayed and are identified below the example. Since unique names are only required at specific scopes, the examples provided are only generated for the scopes above the resource scope: resource group, resource group & region, region, global, subscription, and tenant.

![Reference Page](./wwwroot/Screenshots/ReferencePage.png)

### Generate

The Generate Page provides a dropdown menu to select an Azure resource. Once a resource is selected, naming component options are provided. Read-only components cannot be changed, like the value for a resource type or organization. Optional components, if left blank, will be null and not shown in the output. Required components do not allow a null value, and the first value in the array is set as the default.

![Generate Page](./wwwroot/Screenshots/GeneratePage.png)

## How To Install

This project contains a .NET 6 application, with Docker support. To use, complete the following:

> **NOTE:**
> The Azure Naming Tool requires persistent storage for the configuration files when run as a container. The following processes will explain how to create this volume in your respective environment. All configuration JSON files will be stored in the volume to ensure the configuration is persisted.

### Run as a Docker image

This process will allow you to deploy the Azure Naming Tool using Docker to your local environment.

* On the **<>Code** tab, select the **<>Code** button and select **Download ZIP**
* Extract the zipped files to your local machine
* Change directory to the project folder

> **NOTE:**
> Ensure you can see the project files and are not in the parent folder

* Open a **Command Prompt** and change directory to the current project folder
* Run the following **Docker command** to build the image:

```cmd
docker build -t azurenamingtool .
```
  
> **NOTE:**
> Ensure the '.' is included in the command

* Run the following **Docker command** to create a new container and mount a new volume:

```cmd
docker run -d -p 8081:80 --mount source=azurenamingtoolvol,target=/app/settings azurenamingtool:latest
```

> **NOTES:**  
> * Substitute 8081 for any port not in use on your machine
> * You may see warnings in the command prompt regarding DataProtection and keys. These indicate that the keys are not persisted and are only local to the container instances.

* Access the site using the following URL  

  *http://localhost:8081*
  
> **NOTE:**
> Substitute 8081 for the port you used in the docker run command

***

### Run as an Azure App Service Container

The Azure Naming Tool requires persistent storage for the configuration files when run as a container. The following processes will explain how to create this volume for your Azure App Service Container. All configuration JSON files will be stored in the volume to ensure the configuration is persisted.

> **NOTE:**
> For many of the steps, a sample process is provided, however, there are many ways to accomplish each step.

* On the **<>Code** tab, select the **<>Code** button and select **Download ZIP**
* Extract the zipped files to your local machine
* Change directory to the project folder

> **NOTE:**
> Ensure you can see the project files and are not in the parent folder

* Open a **Command Prompt** and change directory to the current project folder
* Run the following **Docker command** to build the image

```cmd
docker build -t azurenamingtool .
```
  
> **NOTE:**
> Ensure the '.' is included in the command
  
* Create an Azure Container Registry: [Microsoft Docs reference](https://docs.microsoft.com/en-us/azure/container-registry/container-registry-get-started-portal#:~:text=%20Quickstart%3A%20Create%20an%20Azure%20container%20registry%20using,must%20log%20in%20to%20the%20registry...%20More%20)
* Build and publish your image to the Azure Container Registry: [Microsoft Docs reference](https://docs.microsoft.com/en-us/azure/container-registry/container-registry-get-started-docker-cli?tabs=azure-cli)
* Create an Azure Files file share for persistent storage: [Microsoft Docs reference](https://docs.microsoft.com/en-us/azure/storage/files/storage-how-to-create-file-share?tabs=azure-portal)
  
  ![FileShare](./wwwroot/Screenshots/FileShare.png)

* Create an Azure App Service - Web App: [Microsoft Docs reference](https://docs.microsoft.com/en-us/azure/app-service/quickstart-custom-container?tabs=dotnet&pivots=container-linux)
* Mount the file share as local storage for the Azure App Service: [Microsoft Docs reference](https://docs.microsoft.com/en-us/azure/app-service/configure-connect-to-azure-storage?tabs=portal&pivots=container-linux)
  
  ![MountStorage](./wwwroot/Screenshots/MountStorage.png)

* Deploy the image from the Azure Container Registry to the Azure App Service: [Microsoft Docs reference](https://docs.microsoft.com/en-us/azure/app-service/deploy-ci-cd-custom-container?tabs=acr&pivots=container-linux)
* Access the site using your Azure App Service URL
