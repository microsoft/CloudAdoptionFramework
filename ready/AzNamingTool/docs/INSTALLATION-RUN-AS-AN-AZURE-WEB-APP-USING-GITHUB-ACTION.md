[Overview](/ready/AzNamingTool/README.md) | [Installation](/ready/AzNamingTool/docs/INSTALLATION.md) | [Updating](/ready/AzNamingTool/docs/UPDATING.md) | [Using the API](/ready/AzNamingTool/docs/USINGTHEAPI.md) | [Release Notes](/ready/AzNamingTool/RELEASENOTES.md) | [Version History](/ready/AzNamingTool/docs/VERSIONHISTORY.md) | [FAQ](/ready/AzNamingTool/docs/FAQ.md) | [Contributors](/ready/AzNamingTool/docs/CONTRIBUTORS.md)

# Run as an Azure Web App Using GitHub Action (App Service running a .NET application)

* [Choosing an Installation Option](/ready/AzNamingTool/docs/INSTALLATION.md)
* [Overview]($overview)
* [Steps](#steps)

## Overview
This process will allow you to deploy the Azure Naming Tool as a .NET application as an Azure Web App. This is the fastest deployment option and allows you to deploy and utilize your installation in minutes. The provided GitHub Action will deploy your repository code on every commit. This installation process provides the most secure, scalable solution for your installation.<br /><br />
At a high level, this is the installation process:<br />

1. [Fork the Cloud Adoption Framework Repository](#1-fork-the-cloud-adoption-framework-repository)<br /><br />
2. [Create an Azure Web App](#2-create-an-azure-web-app)<br />
   The App Service can be created manually, or leverage the **Deploy to Azure** options below. <br /><br />
3. [Enable Azure Web App Authentication](#3-enable-azure-web-app-authentication)<br />
   Require usrs to autheticate to Azure AD to access the App Service.<br /><br />
5. [Generate Azure Web App Credentials](#4-generate-azure-web-app-credentials)<br />
   These credentials will be used for GitHub to authenticate to your Azure subscription.<br /><br />
5. [Create GitHub secrets](#5-create-github-secrets)<br />
   These secrets are used by the GitHub Action to build the application in the Azure App Service.<br /><br />
5. [Enable GitHub Action](#6-enable-github-action)<br />
   Enable automative deployment of the tool when GitHub repository is updated.<br /><br />

## Steps

### 1. Fork the Cloud Adoption Framework Repository
1. Scroll up to the top-left corner of this page.
2. Click on the **CloudAdoptionFramework** link to open the root of this repository.
3. Click the **Fork** option in the top-right menu.
4. Select your desired **Owner** and **Repository name** and click **Create fork**.
5. Click the green **<>Code** button
6. Click the **.github/workflows** link.

  ![Run as Azure Web App 1](/ready/AzNamingTool/wwwroot/Screenshots/RunAsWebApp1.png)

7. Click the **.deploy-azure-naming-tool-to-azure-webapps-dotnet-core.yml** link.

  ![Run as Azure Web App 2](/ready/AzNamingTool/wwwroot/Screenshots/RunAsWebApp2.png)

8. Review the instructions for creating the required GitHub secrets.

> <br />**NOTES:**<br />
> * The GitHub Action will not successfully deploy until the secrets are created.<br />
> * You must create an Azure Web App and configure the GitHub Action secrets to deploy to your Azure Web App.<br /><br />
<br />

***
### 2. Create an Azure Web App
For an automated deployment of a Web App, utilize the button below and fill in the required information. Then proceed to step 4.    

[![Deploy to Azure](https://aka.ms/deploytoazurebutton)](https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2Fmicrosoft%2FCloudAdoptionFramework%2Fmaster%2Fready%2FAzNamingTool%2FDeployments%2FAppService-WebApp%2Fsolution.json)
[![Deploy to Azure Gov](https://aka.ms/deploytoazuregovbutton)](https://portal.azure.us/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2Fmicrosoft%2FCloudAdoptionFramework%2Fmaster%2Fready%2FAzNamingTool%2FDeployments%2FAppService-WebApp%2Fsolution.json)

1. Create a new Azure Web App in the Azure portal.
2. For the **Publish** option, select **Code**.
3. For the **Runtime stack**, select **.NET 6**.

  ![Web App Basics](/ready/AzNamingTool/wwwroot/Screenshots/WebAppInstallation1.png)

4. Download the **Publish Profile** for use within the GitHub Action secret.

```PowerShell
Get-AzWebApp -Name <webappname> | Get-AzWebAppPublishingProfile -OutputFile <filename> | Out-Null
```  

** OR **

![Web App Details](/ready/AzNamingTool/wwwroot/Screenshots/WebAppInstallation2.png)
<br /><br />
***
### 3. Enable Azure Web App Authentication

1. In the Azure Portal for your Azure Web App, Navigate to the **Authentication** blade.
2. Select **Add identity provider**.
3. In the **Identity provider** section, select **Microsoft**.
4. Enter the desired **Name**. All other options can be left as default.
5. Click **Add**.

  ![Web App Authentication](/ready/AzNamingTool/wwwroot/Screenshots/WebAppAuthentication1.png)

<br /><br />
***

### 4. Generate Azure Web App Credentials  
1. In a command prompt, execute the following command to create credentials for the Azure Web App:

```
az ad sp create-for-rbac --name "[YOUR CREDENTIAL NAME - ENTER ANY VALUE]" --role contributor --scopes /subscriptions/[YOUR SUBSCRIPTION ID]/resourceGroups/[YOUR RESORUCE GROUP NAME] --sdk-auth
```

3. Copy the returned value for later use.

``` JSON
{
  "clientId": "[YOUR CLIENT ID]",
  "clientSecret": "[YOUR CLIENT SECRET]",
  "subscriptionId": "[YOUR SUBSCRIPTION ID]",
  "tenantId": "[YOUR TENANT ID]",
  "activeDirectoryEndpointUrl": "https://login.microsoftonline.com",
  "resourceManagerEndpointUrl": "https://management.azure.com/",
  "activeDirectoryGraphResourceId": "https://graph.windows.net/",
  "sqlManagementEndpointUrl": "https://management.core.windows.net:8443/",
  "galleryEndpointUrl": "https://gallery.azure.com/",
  "managementEndpointUrl": "https://management.core.windows.net/"
}
```
<br /><br />
***

### 5. Create GitHub Secrets
1. In your GitHub repository, click **Settings** in the top menu.
2. Click **Secrets** in the left menu.
3. Click **New repository secret**.
4. Enter **AZURE_WEBAPP_PUBLISH_PROFILE** as the **Name**.
5. Enter the **Publish Profile** data for your Azure Web App as the **Value**.
6. Click **Add secret**.
7. Click **New repository secret**.
8. Enter **AZURE_WEBAPP_NAME** as the **Name**.
9. Enter the name of your Azure Web App as the **Value**.
10. Click **Add secret**.
11. Click **New repository secret**.
12. Enter **AZURE_CREDENTIALS** as the **Name**.
13. Enter the **Azure Wwb App Credentials** JSON as the **Value**:

  ![GitHub Secrets](/ready/AzNamingTool/wwwroot/Screenshots/GitHubActionInstallation1.png)

<br /><br />
***

### 6. Enable GitHub Action
1. In your GitHub repository, click the **Actions** tab.
2. Click on **I understand my workflows, go ahead and enable them**.
3. Select the **Azure Naming Tool - Build and deploy to an Azure Web App** workflow in the left navigation. 
4. Click **Run workflow**.
5. Confirm the workflow completes successfully.

  ![GitHub Workflow](/ready/AzNamingTool/wwwroot/Screenshots/GitHubActionInstallation2.png)

6. Access your Azure Web App to confirm the site is successfully deployed.
