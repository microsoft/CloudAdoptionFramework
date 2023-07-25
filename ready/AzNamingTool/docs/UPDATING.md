[Overview](/ready/AzNamingTool/README.md) | [Installation](/ready/AzNamingTool/docs/INSTALLATION.md) | [Updating](/ready/AzNamingTool/docs/UPDATING.md) | [Using the API](/ready/AzNamingTool/docs/USINGTHEAPI.md) | [Release Notes](/ready/AzNamingTool/RELEASENOTES.md) | [Version History](/ready/AzNamingTool/docs/VERSIONHISTORY.md) | [FAQ](/ready/AzNamingTool/docs/FAQ.md) | [Contributors](/ready/AzNamingTool/docs/CONTRIBUTORS.md)

# Updating

[Overview](#overview)

[Backup Settings](#backup-settings)

[Restore Settings](#restore-settings)

[How to update - Using GitHub Action](#how-to-update---using-github-action)

[How to update - Running as a container](#how-to-update---running-as-a-container)

[How to update - Running as .NET site](#how-to-update---running-as-net-site)

## Overview

The Azure Naming Tool is designed for easy updates, regardless of the hosting scenario. The tool will receive regular updates, often requiring refreshing the local version with updated functionality. All configuration files are stored in the **/settings** folder. This folder can be backed up/restored manually, or via the **Configuration Page** to ensure all settings are retained during updating. This page details the updating process.

## Backup Settings

Before any update, it is recommended that you download the current configuration to a safe location. This file can be used to restore your configuration in the event your settings are modified. 

To back up your configuration:

	1. Access the site in your current environment
	2. Login in as the Administrator
	3. On the Configuration Page, scroll to General Configuration and expand Global Configuration
	4. Under Export the current Global Configuration, select Export
	5. Save the globalconfig.json file to a safe location

## Restore Settings

The Azure Naming Tool allows the ability to back up and restore all site configurations. This process involves generating a global configuration file that contains all settings. This file can be retained for disaster recovery and restore purposes.

To restore the settings:

	1. Access the site in your current environment
	2. Login in as the Administrator
	3. On the Configuration Page, scroll to General Configuration and expand Global Configuration 
	4. Open the globalconfig.json file in any text editor
	5. Under Import Global Configuration, paste the contents of the globalconfig.json file and select Import
	6. Confirm the configuration settings are applied

## How to update - Using GitHub Action

The Azure Naming Tool includes a GitHub Action for deploying to an Azure Web App for simplified deployment. When running as an Azure Web App using the GitHub Action, you will only  need to pull the latest code from the Cloud Adoption Framework repository to your fork repository to update your installation. Once the code uis updated the GitHub Action should execute and deploy the latest code to your Azure Web App.  

To update the Azure Naming Tool running as an Azure Web App using the GitHub Action:

	1. In your GitHub repo, click on the **Code** tab
	2. Click on Sync fork
	3. Click Update branch
	4. Once the fork is updated, click on Actions
	5. Confirm the Azure Naming Tool - Build and deploy to an Azure Web App workflow completed successfully
	6. Once the process is complete, confirm the site is running
	7. Login in using the existing Admin password
	8. On the Configuration Page, confirm your existing configuration is applied

If the settings are not applied, please see the **[Restore Settings](#restore-settings)** instructions. 

## How to update - Running as a container

When running as a container, the Azure Naming Tool utilizes a separate volume for storage of the configuration files. When updating, this volume should not be modified to prevent any loss of the existing configuration. 

To update the Azure Naming Tool running as a container within a Docker environment:

	1. Complete the Backup Settings instructions above.
	2. Complete the Run as a Docker container instructions to update your local codebase and Docker image
	3. Once the process is complete, confirm the site is running
	4. Login in using the existing Admin password
	5. On the Configuration Page, confirm your existing configuration is applied
		a. If the Docker volume was not modified, all the existing settings should be retained

If the settings are not applied, please see the **[Restore Settings](#restore-settings)** instructions. 

## How to update - Running as .NET site
When running the Azure Naming Tool as a stand-alone application, all configurations files are stored in the **/settings**  in the root folder of the site. This folder will need to be retained when updating.

To update the Azure Naming Tool running a stand-alone site:

	1. Complete the Backup Settings instructions above
	2. In your site directory, copy the /settings folder to a safe location
	3. Complete the Run as a Stand-Alone Site instructions to update your local codebase
	4. Copy the /settings folder from your safe location back to the application root
	5. Once the process is complete, confirm the site is running
	6. Login in using the existing Admin password
	7. On the Configuration Page, confirm your existing configuration is applied

If the settings are not applied, please see the **[Restore Settings](#restore-settings)** instructions. 
