[Overview](./) | [Installation](INSTALLATION.md) | [Updating](UPDATING.md) | [Using the API](USINGTHEAPI.md) | [Version History](VERSIONHISTORY.md) | [FAQ](FAQ.md) | [Contributors](CONTRIBUTORS.md)

# Azure Naming Tool v2 - Version History

<img src="./wwwroot/images/AzureNamingToolLogo.png?raw=true" alt="Azure Naming Tool" title="Azure Naming Tool" height="150" />

## Version 2.5.0 (current)

### IMPORTANT NOTES
This update removes GLOBALLY OPTIONAL functionality for components. The new functionality allows a component to be ADDED/REMOVED as OPTIONAL/EXLCUDE for all resource types, using the Configuration page.

To migrate existing GLOBALLY OPTIONAL components:
- On the Configuration page, click the **Edit** button for the component you wish to migrate.
- Expand the **Globally Optional Configuration** section.
- Click **ADD** to add the component as OPTIONAL for all resource types.

![Edit Component](./wwwroot/Screenshots/EditComponent1.png)

### Features
- Added ability to set component as OPTIONAL/EXCLUDE for all resource types
- Removed GLOBALLY OPTIONAL functionality for components (replaced with new functionality)
- Added Version Alert to Configuration Page
- Migrated Admin modal to Admin Page
- Added Version Details section to Admin Page
- Added Site Settings section to Admin Page
- Added "Allow duplicate name generation" setting to Admin Page
- Updated Generate page functionality to prevent duplicate names, if enabled
- Updated Configuration/Generate page styling/formatting
- Updated documentation pages
- Updated screen shots
- Added ResetSiteConfiguration function to Admin API
- Updated 2.5.0 version notes

### Bug fixes
- Added "Working" modal for long-running operations
- Formatting/styling updates to Configuration & Generate pages
- Added Resource Type Property value to Reference page
- Updated Configuration File alert to only display once per session
- Updated Reset Configuration to properly reset all values

***

## Version 2.4.0
### Features
- Added ability to set component as globally optional
- Add Actions Legend to Configuration page
- Added Admin Log / Generated Names Log functions to Admin controller
- Updated Edit modal styling
- Added functionality to generate multiple resource types names on Generate page
- Updated formatting/styling on Generate page
- Updated validation on Generate page
- Updated formatting/styling on Configuration page
- Added ability for admin to delete generated names from log
- Added Instructions link to site pages
- Updated Instructions with latest screenshots/details
- Updated 2.4.0 version notes

### Bug fixes
- Formatting/styling updates to Configuration & Generate pages
- Disabled Latest News feed by default
- Updated Admin Modal section header style for dark mode
- Refactored helper classes

***

## Version 2.3.2

### Bug fixes
- Updated Reference/Generate pages to reload configuration data, if empty
- Updated site to exclude Custom Component if no options are entered
- Updated resourcetypes.json regex patterns
- Updated cache reload process to ensure all data is loaded
- Updated Admin Page styling
- Updated comments in API
- Updated Reference page to include delimiter for samples.
- Added warning to Generate page for resource types with a lower-level scope.
- Updated Nuget packages

***

## Version 2.3.1

### Bug fixes
- Updated Configuration page to export Admin configuration, if selected
- Updated ResourceTypes.cs model to not require Short Name.

***

## Version 2.3.0
### Features
- Added Search functionality to Admin Log
- Added Search functionality to Generated Names Log
- Added caching throughout application for performance/optimization
- Added functionality to check for latest version. Admins will be prompted if the installed version is out of date.
- Added functionality to check for Resource Type and Location file versions. Admins will be prompted if the installed version is out of date.
- Added FAQ.md in GitHub
- Added CONTRIBUTORS.md in GitHub
- Added GitHub Action for deployment to Azure Web App
- Added "Deploy to Azure" options for installation

### Bug fixes
- Fixed grammar/formatting issues in GitHub documentation
- Updated resourcetypes.json with latest Azure resources
- Added information/update prompt to Configuration page if resaourcetypes.json contains types with duplicate short names
- Added option to enable/disbale Latest News feed on Home page
- Added option to exlcude Admin configuration from Global Backup functionality
- Updated API to handle resource types with duplicate names
- Added Admin Log and Generated Names log data to Global Configuration backup/restore functionality
- Updated Installation.md with new guidance

***

## Version 2.2.0
### Features
- Added VERSIONHISTORY.md file
- Added ability to exclude resource type component for resource types
- Added Latest Azure Naming Tool News feed to Home page (from twitter.com/azurenamingtool feed)
- Added Custom Component functionality
	- Updated Configuration page with new functionality
	- Updated Reference page to display custom components if present
	- Updated Generate page to display custom components if present
	- Updated API - RequestName function to accept Custom Components

### Bug fixes
- Added logic to prevent duplicate resource type short names
- Added notification to Configuration page for duplicate resource types short names to prompt for refresh
- Updated Refresh Resource Types utility to refresh short names by default
- Updated /repository/resourcetypes.json with new data
- Added enhanced Admin Log messaging
- Moved Admin Log link to top of navigation
- Added Resource Type to Generated Names Log
- Updated Configuration page button styling
- Updated Generate page component selection logic

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
- Initial release
