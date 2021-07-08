# Azure AD Enablement

Conditional Access is the tool used by Azure Active Directory to bring signals together, to make decisions, and enforce organizational policies. Conditional Access is at the heart of the new identity driven control plane.

Conditional Access policies at their simplest are if-then statements, if a user wants to access a resource, then they must complete an action. Example: A payroll manager wants to access the payroll application and is required to perform multi-factor authentication to access it.

Administrators are faced with two primary goals:

- Empower users to be productive wherever and whenever
- Protect the organization's assets

By using Conditional Access policies, you can apply the right access controls when needed to keep your organization secure and stay out of your user's way when not needed.

## Azure AD Security Defaults

For organizations that are truly looking for a "quick start" with conditional access policies or you use the free tier of Azure Active Directory, then using [Azure Active Directory's Security defaults](https://docs.microsoft.com/azure/active-directory/fundamentals/concept-fundamentals-security-defaults) is the right place to start. These are a basic set of identity security mechanisms recommended by Microsoft. The include the following policies.

- All users in your tenant must register for multi-factor authentication (MFA), but it's not enforced for all users to use for most interactions. Only when those users perform security-interesting activities such as logging on from a new device, performing critical tasks, using a new application, will MFA be triggered.
- All users with privileged access (e.g. Global, SharePoint, Exchange, User, Security administrators), are required to use MFA for most authentication operations.
- Legacy authentication attempts are blocked, this includes things like Office 2010, SMTP, and POP3. Basically any authentication flow that does not support the potential for MFA to be involved.
- All users interacting with Azure via the Portal, Azure CLI/PowerShell, or similar will require MFA.

All MFA usage above is exclusively performed via the Microsoft Authenticator app.

## Conditional Access

If you are looking for "more than" the quick start provided by Azure AD Security defaults, require the use of another MFA flow (such as hardware tokens or telephony-based flows), and you already use Azure AD premium licensing, then it is recommended that you use Conditional Access. This supports all of the same features of the Azure AD Security defaults, but allows you to have much more flexibility and extensibility in its implementation. Azure AD Security defaults and Conditional Access are mutually exclusive, they cannot both be enabled at the same time.

However, it makes sense that if you're just starting out with Conditional Access, you at least [enable the same level of protection offered by Azure AD Security defaults](https://docs.microsoft.com/azure/active-directory/conditional-access/concept-conditional-access-policy-common) via Azure AD Conditional Access.  So let's start there...

You have multiple choices for configuring Conditional Access in Azure AD, such [direct Graph API calls](https://github.com/Azure-Samples/azure-ad-conditional-access-apis/tree/main/01-configure/graphapi), [Graph API Templates](https://github.com/Azure-Samples/azure-ad-conditional-access-apis/tree/main/01-configure/templates), or [PowerShell scripts](https://github.com/Azure-Samples/azure-ad-conditional-access-apis/tree/main/01-configure/powershell).

All changes to Azure AD should always follow your Safe Deployment Practices. Conditional Access policies are no exception. Ideally you'll deploy them across your user population gradually to mitigate support impact via detection of issues early in small user populations. For guidance on that advanced deployment methodology, see our [Conditional Access approval flow](https://github.com/Azure-Samples/azure-ad-conditional-access-apis/tree/main/03-deploy) guidance.

For this workshop, we'll use basic PowerShell scripts to set up the environment.

// TODO, would this be better as Graph API templates instead?
// TODO, should we always have a "Test Group (Audit)" -> "Test Group (On)" -> "All (Audit)" -> "All (On)" flow? to simulate a controlled rollout?

> Next Step: Prereqs  (TODO Break into another page)

CRITICAL:

Because this impact the security profile of Azure AD, ensure you are doing this workshop in an isolated tenant that is free of any business impact caused by changes to policy. In this tenant you need to at least hold the Azure AD role of [Conditional Access administrator](https://docs.microsoft.com/azure/active-directory/roles/permissions-reference#conditional-access-administrator). (For demo purposes, Security administrator or Global administrator will work as well.)

1. Open a Windows PowerShell terminal.  PowerShell Core is not supported for this flow at this time.

1. Install the latest version of the AzureAD V2 PowerShell module.

   ```powershell
   Install-Module -Name AzureAD -Force -Scope CurrentUser

   # Must see AzureAD listed at a version >= 2.0.2.106
   Get-InstalledModule -Name AzureAD
   ```

   Note: This PowerShell module is different than the [Az PowerShell module](https://docs.microsoft.com/powershell/module/az.resources/?view=azps-4.8.0#active-directory) which contains Azure AD commands, but is limited to mostly identity and group operations.

1. Connect to your test Azure AD Tenant with your **Conditional Access administrator** user.

   ```powershell
   Connect-AzureAD -TenantId <your-test-tenant-guid>
   ```

1. Disable Azure AD Security defaults. TODO

1. Apply common Conditional Access policies

   1. Deploy: Block legacy authentication

      Condition: All Users, No Exceptions, Legacy App Types
      Control: Block

      ```powershell
      $conditions = New-Object -TypeName Microsoft.Open.MSGraph.Model.ConditionalAccessConditionSet
      $conditions.Applications = New-Object -TypeName Microsoft.Open.MSGraph.Model.ConditionalAccessApplicationCondition
      $conditions.Applications.IncludeApplications = "All"
      $conditions.ClientAppTypes = @('ExchangeActiveSync', 'Other')
      $conditions.Users = New-Object -TypeName Microsoft.Open.MSGraph.Model.ConditionalAccessUserCondition
      $conditions.Users.IncludeUsers = "All"
      $controls = New-Object -TypeName Microsoft.Open.MSGraph.Model.ConditionalAccessGrantControls
      $controls._Operator = "OR"
      $controls.BuiltInControls = "Block"

      New-AzureADMSConditionalAccessPolicy -DisplayName "Block legacy authentication" -State "enabledForReportingButNotEnforced" -Conditions $conditions -GrantControls $controls
      ```

   1. Deploy: Require MFA for administrators

      Condition:
      Exclusions: Emergency access/break-glass accounts, service accounts/service principals
      Control: Block

      ```powershell
      $conditions = New-Object -TypeName Microsoft.Open.MSGraph.Model.ConditionalAccessConditionSet
      $conditions.Applications = New-Object -TypeName Microsoft.Open.MSGraph.Model.ConditionalAccessApplicationCondition
      $conditions.Applications.IncludeApplications = "All"
      $conditions.ClientAppTypes = "All"
      $conditions.Users = New-Object -TypeName Microsoft.Open.MSGraph.Model.ConditionalAccessUserCondition
      $conditions.Users.IncludeRoles = **TODO Query All Recommended Roles**
      $conditions.Users.ExcludeUsers = **TODO Exclude Break-Glass User once we create it, query for -- should be group based instead**
      $controls = New-Object -TypeName Microsoft.Open.MSGraph.Model.ConditionalAccessGrantControls
      $controls._Operator = "OR"
      $controls.BuiltInControls = "Mfa"

      New-AzureADMSConditionalAccessPolicy -DisplayName "Require MFA for administrators" -State "enabledForReportingButNotEnforced" -Conditions $conditions -GrantControls $controls
      ```

   1. Deploy: Require MFA for Azure Resource Management

      Condition:
      Exclusions: Emergency access/break-glass accounts, service accounts/service principals
      Control: Block

      ```powershell
      $conditions = New-Object -TypeName Microsoft.Open.MSGraph.Model.ConditionalAccessConditionSet
      $conditions.Applications = New-Object -TypeName Microsoft.Open.MSGraph.Model.ConditionalAccessApplicationCondition
      $conditions.Applications.IncludeApplications = "797f4846-ba00-4fd7-ba43-dac1f8f63013" // TODO validate if this is static or needs to be queried -- This is Azure Management
      $conditions.ClientAppTypes = "All"
      $conditions.Users = New-Object -TypeName Microsoft.Open.MSGraph.Model.ConditionalAccessUserCondition
      $conditions.Users.IncludeUsers = **All**
      $conditions.Users.ExcludeUsers = **TODO Exclude Break-Glass User once we create it, query for -- should be group based instead**
      $controls = New-Object -TypeName Microsoft.Open.MSGraph.Model.ConditionalAccessGrantControls
      $controls._Operator = "OR"
      $controls.BuiltInControls = "mfa"

      New-AzureADMSConditionalAccessPolicy -DisplayName "Require MFA for Azure Resource Management" -State "enabledForReportingButNotEnforced" -Conditions $conditions -GrantControls $controls
      ```

   1. Deploy: Require MFA for all users

      Condition:
      Exclusions: Emergency access/break-glass accounts, service accounts/service principals
      Control: Block

      ```powershell
      $conditions = New-Object -TypeName Microsoft.Open.MSGraph.Model.ConditionalAccessConditionSet
      $conditions.Applications = New-Object -TypeName Microsoft.Open.MSGraph.Model.ConditionalAccessApplicationCondition
      $conditions.Applications.IncludeApplications = "All"
      $conditions.ClientAppTypes = "All"
      $conditions.Users = New-Object -TypeName Microsoft.Open.MSGraph.Model.ConditionalAccessUserCondition
      $conditions.Users.IncludeUsers = "All"
      $conditions.Users.ExcludeUsers = **TODO Exclude Break-Glass User once we create it, query for - should be group based instead**
      $controls = New-Object -TypeName Microsoft.Open.MSGraph.Model.ConditionalAccessGrantControls
      $controls._Operator = "OR"
      $controls.BuiltInControls = "mfa"

      New-AzureADMSConditionalAccessPolicy -DisplayName "Require MFA for all users" -State "enabledForReportingButNotEnforced" -Conditions $conditions -GrantControls $controls
      ```

## TODO

- Should we include a sign-in risk-based CA?  If so, should that be merged in with one of the above?
- Should we include a user risk-based CA?  If so, should that be merged in with one of the above?
- Should we include a block access by location CA?
- Should we include a compliant device (audit only) CA?
