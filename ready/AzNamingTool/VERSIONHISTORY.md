[Overview](./) | [Installation](INSTALLATION.md) | [Updating](UPDATING.md) | [Using the API](USINGTHEAPI.md) | [Version History](VERSIONHISTORY.md) 

# Azure Naming Tool v2 - Version History

<img src="./wwwroot/images/AzureNamingToolLogo.png?raw=true" alt="Azure Naming Tool" title="Azure Naming Tool" height="150"/>

## Version 2.1.1 (current)
### Bug fixes
- Added logic to prevent duplicate resource type short names
- Added notification to Configuration page for duplicate resource types short names to prompt for refresh
- Updated Refresh Resource Types utility to refresh short names by default
- Updated /repository/resourcetypes.json with new data
- Added abilityt o exlcude resource type 
- Added enhanced Admin Log messaging
- Moved Admin Log link to top of navigation
- Added Resource Type to Generated Names Log
- Added VERSIONHISTORY.md file
### Notes
- This update includes resource type configuration changes. It is recommended that users refresh their Resource Type configuration using the built-in tool on the Configuration page.

***

## Version 2.1.0
### Features
- Updated RequestName API function to only require basic text for name generation. This simplifies name generation via the API.
- Created RequestNameWithComponents API function for legacy name generation
- Moved Admin Log Message and Generated Name logging to new LogHelper class
- Updated API documentation
- Updated GitHub documentation to separate files
	- README.md - Overview of the Azure Naming Tool
	- INSTALLATION.md - Instructions for installing the Azure Naming Tool
	- UPDATING.md - Instructions for updating an installation
	- USINGTHEAPI.md - Instructions for integrating with the API

***

## Version 2.0.0
- Iinital release