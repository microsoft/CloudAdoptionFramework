# Privileged Identity Management

## Overview

> To use PIM the directory must have a valid Azure AD Premium P2 license.

Privileged Identity Management (PIM) is a service in Azure Active Directory (Azure AD) that enables you to manage, control, and monitor access to important resources in your organization. Privileged Identity Management provides time-based and approval-based role activation to mitigate the risks of excessive, unnecessary, or misused access permissions on resources that you care about.

Here are some of the key features of Privileged Identity Management:

- Provide just-in-time privileged access to Azure AD and Azure resources
- Assign time-bound access to resources using start and end dates
- Require approval to activate privileged roles
- Enforce multi-factor authentication to activate any role
- Use justification to understand why users activate
- Get notifications when privileged roles are activated
- Conduct access reviews to ensure users still need roles
- Download audit history for internal or external audit

PIM it is activated on usage. It can be activated automatically when a user who is active in a privileged role in an Azure AD organization with a Premium P2 license goes to Roles and administrators in Azure AD and selects a role (or even just visits Privileged Identity Management)

## Deploy PIM

### Prepare

1. Enforce principle of least privilege. Before using PIM, it is recommended to create an access review for sensitive roles and remove any user/group that does not need to be part of the role. Following the steps in [start an access review for Azure resources roles in Privileged Identity Management](https://docs.microsoft.com/azure/active-directory/privileged-identity-management/pim-resource-roles-start-access-review), you can set up an access review for every Azure resource role that has one or more members.

2. Decide which role assignments should be protected by Privileged Identity Management.

3. Before activating PIM for the critical roles, make sure you have the break-glass emergency access accounts with permanent access to those roles. This step is mandatory to make sure you won't lock yourself out in case of a misconfiguration.

4. Install the required Powershell modules needed to manage PIM with Powershell:

    ```powershell
    ./scripts/prepare.ps1
    ```

5. Set the initial variables and connect to Azure AD (TenantId can be found in **Azure Active Directory** > **Properties** > **Directory ID**):

    ```powershell
    $SubscriptionId = "[Subscription-Id]"
    $AccountId      = "[Admin-Email]"
    $TenantId       = "[Tenant-Id]"
    ./scripts/connect.ps1
    ```

6. Onboard the subscription with PIM:

    ```powershell
    ./scripts/onboard.ps1
    ```

### Make Role Assignations Eligible

Convert all users assignations to selected roles as eligible instead of active, while skipping the break glass users, using [Azure Portal Assign Roles](https://docs.microsoft.com/azure/active-directory/privileged-identity-management/pim-resource-roles-assign-roles) or

```powershell
$Groups = "Owner", "Security Admin"
./scripts/roles-make-eligible.ps1
```

### Configure Role Settings

Configure activation settings for selected roles using [Azure Portal Role Settings](https://docs.microsoft.com/azure/active-directory/privileged-identity-management/pim-resource-roles-configure-role-settings) or

```powershell
$Groups = "Owner", "Security Admin"
./scripts/roles-apply-settings.ps1
```

### Manage Role Assignations Requests

1. Request activation of a resource that requires approval

   [Azure Portal Request Activation](https://docs.microsoft.com/azure/active-directory/privileged-identity-management/pim-resource-roles-activate-your-rolese) or

    ```powershell
    $UserEmail = "[Specify the user email that requires the group activation]"
    $GroupName = "[Specify the group name you need to activate]"
    $Reason    = "[Specify why you need to activate this group]"
    ./scripts/request-open.ps1
    ```

2. View pending requests

    [Azure Portal Pending Requests](https://docs.microsoft.com/azure/active-directory/privileged-identity-management/pim-resource-roles-approval-workflow#view-pending-requests) or

    ```powershell
    ./scripts/request-pending.ps1
    ```

3. Approve requests for group elevation

    [Azure Portal Approve](https://docs.microsoft.com/azure/active-directory/privileged-identity-management/pim-resource-roles-approval-workflow#approve-requests) or

    ```powershell
    $RoleAssignmentRequestId = "[RoleAssignmentRequestId to approve, found with 'View pending requests']"
    ./scripts/request-approve.ps1
    ```

4. Deny requests for group elevation

    [Azure Portal Deny](https://docs.microsoft.com/azure/active-directory/privileged-identity-management/pim-resource-roles-approval-workflow#deny-requests) or

    ```powershell
    $RoleAssignmentRequestId = "[RoleAssignmentRequestId to deny, found with 'View pending requests']"
    $Reason                  = "[Specify why you want to reject this activation]"
    ./scripts/request-deny.ps1
    ```

### Audit

View requests and login history for all privileged resources using [Azure Portal Audit History](https://docs.microsoft.com/azure/active-directory/privileged-identity-management/azure-pim-resource-rbac) or

```powershell
$Groups = "Owner", "Security Admin"
./scripts/audit.ps1
```

## References

- <https://docs.microsoft.com/en-us/azure/active-directory/privileged-identity-management/>
- <https://docs.microsoft.com/en-us/powershell/module/azuread/?view=azureadps-2.0-preview#privileged-role-management>
- <https://docs.microsoft.com/en-us/azure/governance/policy/samples/azure-security-benchmark#privileged-access>
- <https://docs.microsoft.com/en-us/graph/api/resources/privilegedidentitymanagement-root?view=graph-rest-beta>
- <http://www.anujchaudhary.com/search/label/Privileged%20Identity%20Management>
