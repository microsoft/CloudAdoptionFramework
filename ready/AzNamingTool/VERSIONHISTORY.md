[Overview](./) | [Installation](INSTALLATION.md) | [Updating](UPDATING.md) | [Using the API](USINGTHEAPI.md) | [Version History](VERSIONHISTORY.md) 

# Azure Naming Tool v2 - Version History

<img src="./wwwroot/images/AzureNamingToolLogo.png?raw=true" alt="Azure Naming Tool" title="Azure Naming Tool" height="150"/>

## Version 2.2.0 (current)
### Features
- Added VERSIONHISTORY.md file
- Added ability to exclude resource type component for resource types
- Added Latest Azure Naming Tool News feed to Home page (from twitter.com/azurenamingtool feed)
- Added Custom Component functionality
	- Updated Configuration page with new functionality
	- Updated Reference page to display custom components, if present
	- Updated Generate page to display custom components, if present
	- Updated API - RequestName function to accept Custom Components
	- Added new API functions for Custom Components

### Bug fixes
- Updated Refresh Resource Types utility to refresh short names by default
- Updated /repository/resourcetypes.json with new data
- Added enhanced Admin Log messaging
- Moved Admin Log link to top of navigation
- Added Resource Type to Generated Names Log
- Updated Configuration page button styling
- Updated Generate page component selection logic
- Updated generated name validation to handle empty delimiter

***

## Version 2.1.0
### Features
- Updated RequestName API function to only require basic text for name generation. This simplifies name generation via the API.
- Created RequestNameWithComponents API function for legacy name generation
- Updated GitHub documentation to separate files
	- README.md - Overview of the Azure Naming Tool
	- INSTALLATION.md - Instructions for installing the Azure Naming Tool
	- UPDATING.md - Instructions for updating an installation
	- USINGTHEAPI.md - Instructions for integrating with the API

### Bug fixes
- Moved Admin Log Message and Generated Name logging to new LogHelper class
- Updated API documentation

***

## Version 2.0.0
- Iinital release