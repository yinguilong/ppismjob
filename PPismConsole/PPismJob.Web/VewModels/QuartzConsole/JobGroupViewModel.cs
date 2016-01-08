#region

using System.Collections.Generic;
using PPismJob.Web.Modules;

#endregion

namespace PPismJob.Web.VewModels.QuartzConsole
{
    public class JobGroupViewModel
    {
        public List<JobViewModel> JobDetails { get; set; }
    }
}