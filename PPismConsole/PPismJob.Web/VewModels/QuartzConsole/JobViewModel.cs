#region

using System;
using System.Collections.Generic;
using Quartz;
using PPismJob.Web.Modules;

#endregion

namespace PPismJob.Web.VewModels.QuartzConsole
{
    public class JobViewModel
    {
        public JobKey JobKey { get; set; }
        public DateTimeOffset? NextScheduledRun { get; set; }
        public DateTimeOffset? LastRun { get; set; }

        public string SchedulerName { get; set; }
        public List<JobTriggerViewModel> Triggers { get; set; }

        public bool IsRunning { get; set; }
    }
}