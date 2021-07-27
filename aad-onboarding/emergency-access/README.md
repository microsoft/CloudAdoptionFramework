# Facilitating emergency access

_Emergency access_ accounts (typically known as "break-glass" accounts), are highly-privileged accounts in your Azure AD tenant. They are not assigned to specific individuals. And most importantly, usage is expressly limited to emergency scenarios where normal individual-based administrative accounts literally cannot be used. This is usually due to unexpected external influence, such as current solo Global Administrator needs to be terminated, a natural disaster impacting multi-factor auth, or a misconfiguration of conditional access policies.

## Guidance

* Create two or more emergency access accounts.
* They should be cloud-only accounts that use the *.onmicrosoft.com domain and are not federated nor synchronized with an on-premises environment
* they are not federated or synchronized from an on-premises environment
* they are not associated with any individual
* Auth mechanism should be distinct from that used by other admin accounts, including other emergency access accounts. This means if Azure AD MFA is your primary access method, then use a third party MFA for the emergency account
* The credential must not expire or be in scope of any automated cleanup due to lack of use
* The role assignment to Global Administrator should be permanent, not JIT.
* At least one account should be excluded from phone-based MFA
* At least one account should be excluded from all conditional-access policies
* If using passwords, they should not expire and password should be at least 16 characters long and randomly generated.

## Credential storage

Because these identities are not tied to individuals, yet need to be access by authorized individuals in time of need, the storage of the credentials (smartcard or password) becomes challenging. When deciding how to store that credential consider the impact of people leaving the company, 3rd party outages, 1st party outages, natural disasters. Consider writing down parts of the password on separate pieces of paper, stored in secure, separate locations that are resistance to flooding or fire or impact.

## Monitor usage

Use Azure Log Analytics to monitor the sign-in logs and trigger email and SMS alerts to your current AD Admins whenever emergency access accounts sign in.

### Instructions to set up this alert

1. [Send Azure AD sign-in logs to Azure Monitor.](https://docs.microsoft.com/azure/active-directory/reports-monitoring/howto-integrate-activity-logs-with-log-analytics)

1. Object Object IDs for each break-glass account

1. Create the following log search alert.

   ```kusto
   SigninLogs
   | project UserId
   | where UserId in ('guid-a', 'guid-b')
   ```

1. Schedule to run once every 5 minutes.

1. Set the severity level to **Critical** (Sev 0)

1. Set up an action group to notify the admins

## Account validation

* Each emergency-access account should be tested at least every 90 days.
* Ensure all security-monitoring staff is aware that a test is being performed
* Not only should access be tested, but all expected sign-in alerts should be validated as well
* Ensure that all administrators and security officers are trained on this process

## Deploy

The script below will create two emergency access users, sets their password policy, and assign them the Global Administrator role.

```powershell
$TenantId = "<your tenant guid>"
.\create-emergency-access-users.ps1
```

> AzureAD powershell modules are not yet supported in .NET Core, and therefore the script won't work in PowerShell Core (7.X). You must use Windows Powershell.
