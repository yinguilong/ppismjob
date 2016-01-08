using Nancy.ViewEngines.Razor;
using Quartz;
using System.Collections.Generic;
using PPismJob.Web.Modules;
using Nancy.Helpers;
namespace PPismJob.Web
{
    public class QuartzRazorConfiguration : IRazorConfiguration
    {
        public IEnumerable<string> GetAssemblyNames()
        {
            yield return typeof(QuartzConsoleModule).Assembly.FullName;
            yield return typeof(JobKey).Assembly.FullName;
            yield return typeof(HttpUtility).Assembly.FullName;
        }

        public IEnumerable<string> GetDefaultNamespaces()
        {
            yield return typeof(QuartzConsoleModule).Namespace;
            yield return typeof(PPismJob.Web.Views.HtmlHelpers).Namespace;
            yield return typeof(HttpUtility).Namespace;
        }


        public bool AutoIncludeModelNamespace
        {
            get { return false; }
        }
    }
}