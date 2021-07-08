# Steps

1. [Send Azure AD sign-in logs to Azure Monitor.](https://docs.microsoft.com/azure/active-directory/reports-monitoring/howto-integrate-activity-logs-with-log-analytics)

2. Object Object IDs for each break-glass account

3. Create the following log search alert.

   ```kusto
   SigninLogs
   | project UserId
   | where UserId in ('guid-a', 'guid-b')
   ```

4. Schedule to run once every 5 minutes.

5. Set the severity level to **Critical** (Sev 0)

6. Set up an action group to notify the admins

## TODO

Group Membership Changes Alerts for the Emergency Access Group
