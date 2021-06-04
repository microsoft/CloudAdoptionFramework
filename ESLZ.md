- docs/reference/contoso/armTemplates/auxiliary/diagnosticsAndSecurity.json assigns policies to each subscription and also creates the log workspace in the management subscription

- docs/reference/contoso/armTemplates/auxiliary/subscriptioSecurityConfig.json template deploys/enables de ASC for a subscription, it is similar to our deploy-asc.json script. The main difference is that our script also set autoProvision while ESLZ script uses the builtin autoprovision policy using the log workspace created in the previous step. 

- ESLZ has a single policies.json file including all the policies and policySets (https://github.com/Azure/Enterprise-Scale/blob/773f3a5738c3d2b9baa2c3c8f1890cdef03637fb/docs/reference/contoso/armTemplates/auxiliary/policies.json). I think it is better to split the policies in different files for a workshop.

Onboarding                              ESLZ
ASC-Enable-Alerts                       Deploy-ASC-SecurityContacts
ASC-Enable-AzureDefender-for-ARM        Deploy-ASC-Defender-ARM
ASC-Enable-AzureDefender-for-DNS        Deploy-ASC-Defender-DNS
ASC-Enable-AzureDefender-for-Servers    Deploy-ASC-Defender-VMs
