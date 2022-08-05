[Overview](./) | [Installation](INSTALLATION.md) | [Updating](UPDATING.md) | [Using the API](USINGTHEAPI.md)

# Azure Naming Tool v2 - Updating

<img src="./wwwroot/images/AzureNamingToolLogo.png?raw=true" alt="Azure Naming Tool" title="Azure Naming Tool" height="150"/>

[Overview](#overview)

[Backup Settings](#backup-settings)

[Restore Settings](#restore-settings)

[How to update - Running as a container](#updatecontainer)

[How to update - Running as .NET site](#updatesite)

## Overview

The Azure Naming Tool is designed for easy updates, regardless of the hosting scenario. The tool will receive regular updates, often requiring refreshing the local version with updated functionality. All configurations files are stored in the **/settings** folder. This folder can be backed up/restored manually, or via the **Configuration Page** to ensure all settings are retained during updating. This page details the updating process.

## Backup Settings

Before any update, it is recommended that you download the current configuration to a safe location. This file can be used to restore your configuration in the event your settings are modified. 

To back up your configuration:

	1. Access the site in your current environment
	2. Login in as the Administrator
	3. On the Configuration Page**, scroll to General Configuration and expand **Global Configuration** 
	4. Under Export the current Global Configuration, select Export
	5. Save the globalconfig.json file to a safe location

## Restore Settings

The Azure Naming Tool allows the ability to back up and restore all site configurations. This process involves generating a global configuration file that contains all settings. This file can be retained for disaster recovery and restore purposes.

To restore the settings:

	1. Access the site in your current environment
	2. Login in as the Administrator
	3. On the Configuration Page, scroll to General Configuration and expand Global Configuration 
	4. Open the globalconfig.json file in any text editor
	5. Under Import Global Configuration, paste the contents of the globalconfig.json file and select **Import**
	6. Confirm the configuration settings are applied

## Update Container

When running as a container, the Azure Naming Tool utilizes  a separate volume for storage of the configuration files. When updating, this volume should not be modified to prevent any loss of the existing configuration. 

To update the Azure Naming Tool running as a container within a Docker environment:

	1. Complete the Back up Settings instructions above.
	2. Complete the Run as a Docker container instructions to update your local codebase and Docker image
	3. Once the process is complete, confirm the site is running
	4. Login in using the existing Admin password
	5. On the Configuration Page, confirm your existing configuration is applied
		a. If the Docker volume was not modified, all the existing settings should be retained

If the settings are not applied, please see the **[Restore Settings](#restore-settings)** instructions. 

## Update Site
When running the Azure Naming Tool as a stand-alone application, all configurations files are stored in the **/settings**  in the root folder of the site. This folder will need to be retained when updating.

To update the Azure Naming Tool running a stand-alone site:

	1. Complete the Back up Settings instructions above
	2. In your site directory, copy the /settings folder to a safe location
	3. Complete the Run as a Stand-Alone Site instructions to update your local codebase
	4. Copy the /settings folder from your safe location back to the application root
	5. Once the process is complete, confirm the site is running
	6. Login in using the existing Admin password
	7. On the Configuration Page, confirm your existing configuration is applied

If the settings are not applied, please see the **[Restore Settings](#restore-settings)** instructions. 

	
