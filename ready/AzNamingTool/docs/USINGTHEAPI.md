[Overview](/ready/AzNamingTool/README.md) | [Installation](/ready/AzNamingTool/docs/INSTALLATION.md) | [Updating](/ready/AzNamingTool/docs/UPDATING.md) | [Using the API](/ready/AzNamingTool/docs/USINGTHEAPI.md) | [Release Notes](/ready/AzNamingTool/RELEASENOTES.md) | [Version History](/ready/AzNamingTool/docs/VERSIONHISTORY.md) | [FAQ](/ready/AzNamingTool/docs/FAQ.md) | [Contributors](/ready/AzNamingTool/docs/CONTRIBUTORS.md)

# Using the API

[Overview](#overview)

[How to use the API](#how-to-use-the-api)

## Overview
The Azure Naming Tool includes a fully-functional REST API for integrating with your existing systems. The API is enabled by default and an OpenAPI definition file is included, detailing all aspects of the API. 

><br />**NOTE**<br />
>The current API is very developer-focused. We will continue to improve the functionality to expose more capabilities over time. <br /><br />

This page details how to integrate thew API into your existing architecture.

><br/>**NOTE**<br />
>All API calls require an API Key. This key can be found in the **Admin** section when authenticated to the site. <br /><br />

## How to use the API
The OpenAPI definition (Swagger file) documents all the classes required to interact with the API.  The required models will vary, depending on which API function is being requested. The OpenAPI definition details the individual models required for each function. 

### Retrieving data

The following process details a sample API integration to return components and their options:

	- Retrieve the API Key from the Admin section (this value will be passed with all API calls)
	- Retrieve the current components
		○ The site configuration contains a list of the current selected components and their respective order. To retrieve these values:
			§ Call the **ResourceComponents (GET /api/ResourceComonents)** function
				□ API Key: [Your API Key]
				□ Admin : false
			§ This API call will return the list of current selected components in JSON format. 
	- Retrieve the available options for each component
		○ For each component in the **ResourceComponents** results, call the respective API call for the component type to return the available component options. 
			§ Example:
				□ Component Name: ResourceOrg
					§ Call the **ResourceOrgs(GET /api/ResourceOrgs)** function
						□ API Key: [Your API Key]
		○ Complete this action for all desired components to retreive the availble options for the component


### Generating Names
To generate a resource type name programmatically, the API requires a JSON payload of all the components being requested. This includes all configured components and the desired resource type. 

The process is as follows:

	- API receives request for name generation, containing the desired components and their values
		○ [ComponentType] : [Short Name Value]
		○ Example
			§ "ResourceLocation":"eu"
	- The API will validate the following
		○ The provided resource type is valid
		○ The required components are present
		○ The provided component values are valid options for the component (as per the configuration)
		○ The generated name is valid, as per the resource type configuration
		○ Optional components for the provided resource type are not required.
		○ Excluded components for the provided resource type are ignored.
	- The generated name is returned.


	Sample Generate Name Request
		{
		  "ResourceEnvironment": "prd",
		  "ResourceInstance": "2",
		  "ResourceLocation": "eu",
		  "ResourceProjAppSvc": "spa",
		  "ResourceType": "redis"
		}
		
