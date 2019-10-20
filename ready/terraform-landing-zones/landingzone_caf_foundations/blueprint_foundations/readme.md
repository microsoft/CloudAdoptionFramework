# blueprint_foundations
Blueprint_foundations sets the basics of security, auditing, and logs as described below. 

## Capabilities
 - Resource groups
    - Core resource groups needed for hub.
 - Activity Logging
    - Auditing all subscription activities and archiving
        - Storage Account
        - Event Hubs
 - Diagnostics Logging
    - All operations logs kept for x days
        - Storage Account
        - Event Hubs
 - Log Analytics
    - Stores all the operations logs
    - Common solutions for deep application best practices review:
        - NetworkMonitoring
        - ADAssessment
        - ADReplication
        - AgentHealthAssessment
        - DnsAnalytics
        - KeyVaultAnalytics
- Security Center
    - Security hygiene metrics and alerts

## Customization 
The provided foundations.auto.tfvars allows you to deploy your first version of blueprint_foundations and see the typical options

## Foundations
The output of blueprint_foundations will be stored in Azure Storage Account and will be read by subsequent modules. 
Please do not modify the provided output variables but add additional below if you need to extend the model.

# Contribute
Pull requests are welcome to evolve the framework and integrate new features!