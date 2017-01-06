using System;
using System.Collections.Generic;
using System.Linq;
using Toggl;
using Toggl.QueryObjects;
using Toggl.Services;

namespace JiraTogglSync.Services
{
    public class TogglService : IWorksheetSourceService
    {
        private readonly string _apiKey;
        private string _syncedKey = "Jira-Synced";

        public TogglService(string apiKey)
        {
            _apiKey = apiKey;
        }

        public string GetUserInformation()
        {
            var userService = new UserService(_apiKey);
            var currentUser = userService.GetCurrent();
            return string.Format("{0} ({1})", currentUser.FullName, currentUser.Email);
        }

        public IEnumerable<WorkLogEntry> GetEntries(DateTime startDate, DateTime endDate)
        {
            var timeEntryService = new TimeEntryService(_apiKey);

            var hours = timeEntryService
                .List(new TimeEntryParams
                {
                    StartDate = startDate,
                    EndDate = endDate,
                })
                .Where(entry => entry.TagNames == null || !entry.TagNames.Contains(_syncedKey))
                .Where(w => !string.IsNullOrEmpty(w.Description) && w.Stop != null);

            return hours.Select(ToWorkLogEntry);
        }

        public void SetWorkLogSynced(WorkLogEntry entry)
        {
            var timeEntryService = new TimeEntryService(_apiKey);

            var timeEntry = timeEntryService.Get(entry.Id.Value);

            if (timeEntry.TagNames == null)
            {
                timeEntry.TagNames = new List<string>();
            }

            if (!timeEntry.TagNames.Contains(_syncedKey))
            {
                timeEntry.TagNames.Add(_syncedKey);
            }

            timeEntryService.Edit(timeEntry);
        }

        private static WorkLogEntry ToWorkLogEntry(TimeEntry arg)
        {
            return new WorkLogEntry
            {
                Start = DateTime.Parse(arg.Start),
                Stop = DateTime.Parse(arg.Stop),
                Description = arg.Description,
                Tags = arg.TagNames,
                Id = arg.Id
            };
        }
    }
}