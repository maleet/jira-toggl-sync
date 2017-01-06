using System;
using System.Linq;
using JiraTogglSync.Services;

namespace JiraTogglSync.CommandLine
{
    class Program
    {
        static void Main(string[] args)
        {
            var togglApiKey = ConfigurationHelper.GetEncryptedValueFromConfig("toggl-api-key", () => AskFor("Toggl API Key"));
            var toggl = new TogglService(togglApiKey);
            Console.WriteLine("Toggl: Connected as {0}", toggl.GetUserInformation());

            var jiraInstance = ConfigurationHelper.GetValueFromConfig("jira-instance", () => AskFor("JIRA Instance"));
            var jiraUsername = ConfigurationHelper.GetValueFromConfig("jira-username", () => AskFor("JIRA Username"));
            var jiraPassword = ConfigurationHelper.GetEncryptedValueFromConfig("jira-password", () => AskFor("JIRA Password"));
            var jiraKeyPrefixes = ConfigurationHelper.GetValueFromConfig("jira-prefixes", () => AskFor("JIRA Prefixes without the hyphen (comma-separated)"));
            var jiraWorklogStrategy = ConfigurationHelper.GetValueFromConfig("jira-worklogStrategy",
                () => AskFor("JIRA Worklog strategy (AutoAdjustRemainingEstimate, RetainRemainingEstimate (default))"));
            var jira = new JiraRestService(jiraInstance, jiraUsername, jiraPassword, jiraWorklogStrategy);
            Console.WriteLine("JIRA: Connected as {0}", jira.GetUserInformation());

            DateTime? fromDate = null;
            DateTime parsedFromDate;
            if (DateTime.TryParse(ConfigurationHelper.GetValueFromConfig("syncFrom", () => AskFor("Sync from date")),
                out parsedFromDate))
            {
                fromDate = parsedFromDate;
            }
            DateTime? toDate = null;
            DateTime parsedToDate;
            if (DateTime.TryParse(ConfigurationHelper.GetValueFromConfig("syncTo", () => AskFor("Sync to date")), out parsedToDate))
            {
                toDate = parsedToDate;
            }

            if (fromDate == null)
            {
                var syncDays = int.Parse(ConfigurationHelper.GetValueFromConfig("syncDays", () => AskFor("Sync how many days")));
                fromDate = DateTime.Now.Date.AddDays(-syncDays);
            }
            if (toDate == null)
            {
                toDate = DateTime.Now.Date.AddDays(1);
            }

            var roundingToMinutes = int.Parse(ConfigurationHelper.GetValueFromConfig("roundingToMinutes", () => AskFor("Round duration to X minutes")));

            var sync = new WorksheetSyncService(toggl, jira, jiraKeyPrefixes.Split(','));
            
            var suggestions = sync.GetSuggestions(fromDate.Value, toDate.Value).ToList();
            suggestions.ForEach(x => x.WorkLog.ForEach(y => y.Round(roundingToMinutes)));

            if (!suggestions.Any())
            {
                Console.Write("No entries to sync");
            }
            else
            {
                foreach (var issue in suggestions.OrderBy(entry => entry.WorkLog.First().Start))
                {
                    var issueTitle = issue.ToString();
                    Console.WriteLine(issueTitle);
                    Console.WriteLine(new String('=', issueTitle.Length));

                    foreach (var entry in issue.WorkLog.Where(entry => entry.RoundedDuration.Ticks > 0))
                    {
                        Console.Write(entry + " (y/n)");
                        if (Console.ReadKey(true).KeyChar == 'y')
                        {
                            sync.AddWorkLog(entry);
                            Console.Write(" Done");
                        }
                        Console.WriteLine();
                    }
                }
                Console.WriteLine($"Synced from {fromDate} to {toDate}");
            }
            
            Console.ReadLine();
        }

        private static string AskFor(string what)
        {
            Console.Write("Please enter your {0}: ", what);
            return Console.ReadLine();
        }
    }
}