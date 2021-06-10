# Enterprise-Scale Landing Zones

This file documents compares the workshop contents with the [Enterprise-Scale Landing Zones](https://github.com/Azure/Enterprise-Scale) implementation.

## Comparison

- docs/reference/contoso/armTemplates/auxiliary/diagnosticsAndSecurity.json assigns policies to each subscription and also creates the log workspace in the management subscription

- docs/reference/contoso/armTemplates/auxiliary/subscriptioSecurityConfig.json template deploys/enables de ASC for a subscription, it is similar to our deploy-asc.json script. The main difference is that our script also set autoProvision while ESLZ script uses the builtin autoprovision policy using the log workspace created in the previous step. 

- ESLZ has a single policies.json file including all the policies and policySets (https://github.com/Azure/Enterprise-Scale/blob/773f3a5738c3d2b9baa2c3c8f1890cdef03637fb/docs/reference/contoso/armTemplates/auxiliary/policies.json). I think it is better to split the policies in different files for a workshop.

## Policies

The following table compare the policies naming between the asc onboarding workshop and the ESLZ setup:

Onboarding | ESLZ
---------- | ----
ASC-Enable-Alerts | Deploy-ASC-SecurityContacts
ASC-Enable-AzureDefender-for-ARM | Deploy-ASC-Defender-ARM
ASC-Enable-AzureDefender-for-DNS | Deploy-ASC-Defender-DNS
ASC-Enable-AzureDefender-for-Servers | Deploy-ASC-Defender-VMs

## RBAC

- Suggested RBAC/PIM approach
  <https://github.com/Azure/Enterprise-Scale/blob/main/docs/reference/contoso/Readme.md>
  Search: "Identity and Access Management"

- RBAC is suggested but there are not templates to define custom roles. Check:
  - <https://github.com/Azure/Enterprise-Scale/issues/415>
  - <https://github.com/edm-ms/AzureLandingZone/tree/main/Identity>

- When a new subscription (landing zone) is created (in one of the management groups),
  a principal id is send as parameter an added to 'Owner' role within
  the new subscription
  <https://github.com/Azure/Enterprise-Scale/blob/main/examples/landing-zones/subscription-with-rbac/subscriptionWithRbac.json>

- Enable Service Principal to create landing zones
  As a last step the Service Principal will be granted access to the enrolment account by assigning a role 
  with the Microsoft.Subscription/subscriptions/write permission. 
  Build-in role Enrollment account subscription creator (GUID: a0bcee42-bf30-4d1b-926a-48d21664ef71) is used in this guide.

- Policy Assignment
  Assigns policy service principal (created when policy was assigned?)
  to the role definition specified by the policy definition (or owner sometimes)

  Sample:

  ```json
    {
        // Role assignment for the policy assignment to do on-behalf-of deployments
        "type": "Microsoft.Authorization/roleAssignments",
        "apiVersion": "2018-09-01-preview",
        "name": "[variables('rbacNameForLz')]",
        "dependsOn": [
            "[resourceId('Microsoft.Authorization/policyAssignments', variables('vNetPolicyAssignment'))]"
        ],
        "properties": {
            "principalType": "ServicePrincipal",
            "principalId": "[reference(resourceId('Microsoft.Authorization/policyAssignments/', variables('vNetPolicyAssignment')), '2019-06-01', 'Full').identity.principalId]",
            "roleDefinitionId": "[reference(variables('vNetPolicyDefinition'), '2019-06-01').policyRule.then.details.roleDefinitionIds[0]]"
        }
    }
    ```

  Links:

  - <https://github.com/Azure/Enterprise-Scale/blob/main/docs/reference/adventureworks/armTemplates/auxiliary/corp-policy-peering.json>
  - <https://github.com/Azure/Enterprise-Scale/blob/main/docs/reference/contoso/armTemplates/auxiliary/diagnosticsAndSecurity.json>