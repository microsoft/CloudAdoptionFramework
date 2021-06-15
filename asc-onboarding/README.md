# Azure Security Center Onboarding Sample

## Policies

Policies templates were built based on the [Azure Security Center Policy Definitions](https://github.com/Azure/Azure-Security-Center/tree/main/Pricing%20%26%20Settings/Azure%20Policy%20definitions)

- **policies\ASC-Enable-AzureDefender-for-ARM.json**: Enables standard pricing for ARM in ASC.
- **policies\ASC-Enable-AzureDefender-for-DNS.json**: Enables standard pricing for DNS in ASC.
- **policies\ASC-Enable-AzureDefender-for-Servers.json**: Enables standard pricing for VMs in ASC.
- **policies\ASC-Enable-SecurityContacts.json**: Enables and configures security contact information in ASC.

A full list of built-in policies available in azure can be found [here](https://github.com/Azure/azure-policy)

## Templates

### templates\deploy-asc.json

Deploys azure security center in a subscription using standard price tier for azure defender.

It only enables it for ARM, DNS and Servers.

### templates\deploy-policies.json

- Assigns the [Azure Security Benchmark](https://docs.microsoft.com/security/benchmark/azure/) policies as the default initiative for the subscription.
- Creates a new policy set including all the custom policies defined inside the policies folder.
- Assigns the newly created policy set to the subscription to enforce ASC configuration.

## Notes

- These scripts/templates are meant to be run at subscription level in an empty/new subscription. We do not recommend to use these scripts in an existent subscription.

- To register newly created subscriptions, customers have to create a remediation task for the policies. This is because subscriptions are not a top-level ARM resource, so they currently do not trigger a policy evaluation when they are created.

- When using a custom log workspace, in multi geo deployments, the log agent will still send logs to the a single workspace (located in one of the geos). This can cause undesired costs associated with cross-geo traffic. One possible solution can be found in [Azure Security Center Repository](https://github.com/Azure/Azure-Security-Center/tree/main/Pricing%20%26%20Settings/Azure%20Policy%20definitions/Workspace%20Management/Regional%20Workspaces)

## Execution

Run ```./setup.ps1``` script and follow up instructions in the screen.
