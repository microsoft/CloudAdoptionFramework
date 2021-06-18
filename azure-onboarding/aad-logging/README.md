# Azure AD Access Logging

Azure AD supports emitting access logs to a Log Analytics workspace. From this workspace you can run query-based alerts to notify of unexpected behavior. In this accelerator, we include a query-based alert to identify when an _emergency access_ user logs in. Because these users should be used infrequently, alerting on ALL usages of the account can be critical in the effective governance and transparency of their usage.

## Enabling

```bash
az deployment tenant create -l eastus2 -f ./scripts/tenant.json -p logAnalyticsResourceId=<log analytics resource id>
```
