
# Roles & Permissions

To manage Azure Security Center organization-wide, it is necessary that customers have named a team who is responsible for monitoring and governing their Azure environment from a security perspective.

Customers need to make sure that the central security team has been assigned the necessary RBAC rights on the appropriate scope to follow the deployment steps in this document. We recommend to follow the principle of least privilege when assigning permissions and suggest to assign the following built-in roles:

Action | RBAC Role
------ | ---------
Need to view configurations, update the security policy, and dismiss recommendations and alerts in Security Center. | Security Admin
Need to have read and write access to Azure resources for remediation (this includes assigning the appropriate permission to the managed identity used by a deployIfNotExists or modify policy) | Contributor

In addition to the roles that need to be assigned to the central security team, other personas in the customer’s organization like security auditors or a central SOC team may also need to have read access to the company’s security state. In this case, we recommend to grant them Security Reader permissions.

## Automation

```powershell
$SubscriptionId = "[Subscription-Id]"
az role assignment create --role "Security Admin" --assignee-object-id '{AD-Group-ObjectID}' --scope "/subscriptions/$($SubscriptionId)"
az role assignment create --role "Contributor"    --assignee-object-id '{AD-Group-ObjectID}' --scope "/subscriptions/$($SubscriptionId)"
```

## Policies

The following custom policies can be used to restrict how RBAC is being applied within your subscription.
They are based on the work published in [Azure Community Policies](https://github.com/Azure/Community-Policy) repository.

Name | Description | Effect(s)
---- | ----------- | ---------
Allowed Role Definitions | This policy defines a list of role definitions that _can_ be used in IAM. | Deny, Disabled
Allowed Role Definitions For Principal IDs | This policy defines a list of role definitions that _can_ be assigned to specific Principal IDs in IAM. This is useful in the example where you don't want an SPN having it's rights elevated. | Deny, Disabled
Disallowed Role Definitions | This policy defines a list of role definitions that _cannot_ be used in IAM. | Deny, Disabled
Audit Role Assignments Principal Type | This policy audits for any Role Assignments for a specific Principal Type (e.g. User/Group/ServicePrincipal) | audit

### Allowed Role Definitions

This policy defines a list of role definitions that _can_ be used in IAM.

```powershell
$SubscriptionId = "[Subscription-Id]"

az policy definition create --name 'allowed-role-definitions' --display-name 'Allowed Role Definitions' --description 'This policy defines a white list of role definitions that can be used in IAM' --rules 'policies\allowed-role-definitions\azurepolicy.rules.json' --params 'policies\allowed-role-definitions\azurepolicy.parameters.json' --mode All

az policy assignment create --name "allowed-role-definitions-assignment" --scope "/subscriptions/$($SubscriptionId)" --policy "allowed-role-definitions"
```

### Allowed Role Definitions For Principal IDs

This policy defines a white list of role definitions that can be assigned to specific Principal IDs in IAM. This is useful in the example where you don't want an SPN having it's rights elevated.

```powershell
$SubscriptionId = "[Subscription-Id]"

az policy definition create --name 'allowed-role-definitions-for-principal-ids' --display-name 'Allowed Role Definitions for Principal Ids' --description 'This policy defines a list of role definitions that can be assigned to specific Principal IDs in IAM' --rules 'policies\allowed-role-definitions-for-principal-ids\azurepolicy.rules.json' --params 'policies\allowed-role-definitions-for-principal-ids\azurepolicy.parameters.json' --mode All

az policy assignment create --name "allowed-role-definitions-for-principal-ids-assignment" --scope "/subscriptions/$($SubscriptionId)" --policy "allowed-role-definitions-for-principal-ids"
```

### Disallowed Role Definitions

This policy defines a black list of role definitions that can not be used in IAM

```powershell
$SubscriptionId = "[Subscription-Id]"

az policy definition create --name 'disallowed-role-definitions' --display-name 'Disallowed Role Definitions' --description 'This policy defines a list of role definitions that cannot be used in IAM' --rules 'policies\disallowed-role-definitions\azurepolicy.rules.json' --params 'policies\disallowed-role-definitions\azurepolicy.parameters.json' --mode All

az policy assignment create --name "disallowed-role-definitions-assignment" --scope "/subscriptions/$($SubscriptionId)" --policy "disallowed-role-definitions"
```

### Audit Role Assignments Principal Type

This policy defines a list of role definitions that _cannot_ be used in IAM.

```powershell
$SubscriptionId = "[Subscription-Id]"

az policy definition create --name 'audit-role-assignments-principaltype' --display-name 'Audit Role Assignments Principal Type' --description 'This policy audits for any Role Assignments for a specific Principal Type' --rules 'policies\audit-role-assignments-principaltype\azurepolicy.rules.json' --params 'policies\audit-role-assignments-principaltype\azurepolicy.parameters.json' --mode All

az policy assignment create --name "audit-role-assignments-principaltype-assignment" --scope "/subscriptions/$($SubscriptionId)" --policy "audit-role-assignments-principaltype"
```
