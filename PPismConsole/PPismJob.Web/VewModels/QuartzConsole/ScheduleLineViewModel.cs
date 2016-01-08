using System;
using Quartz;

namespace PPismJob.Web.VewModels.QuartzConsole
{
    public class ScheduleLineViewModel
    {
        public DateTimeOffset Time { get; set; }

        public TriggerKey Name { get; set; }
    }

}