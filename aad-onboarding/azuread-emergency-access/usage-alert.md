# Steps

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

## TODO

Group Membership Changes Alerts for the Emergency Access Group
