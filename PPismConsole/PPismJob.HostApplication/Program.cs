using PPismJob.Web;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PPismJob.HostApplication
{
    class Program
    {
        static void Main(string[] args)
        {

            var nancyHost = QuartzConsoleBootstrapper.StartDefault(GetFactory());

            Console.ReadLine();
            nancyHost.Stop();
        }
        private static ISchedulerFactory GetFactory()
        {
            ISchedulerFactory schedFact = new StdSchedulerFactory();

            var sched = schedFact.GetScheduler();
            sched.Start();
            //CreateJob<TestJob>(sched, "PPismJob.HostApplication.TestJob", (TriggerBuilder t) => t.WithCronSchedule("0/10 * * * * ?"));
            return schedFact;
        }

        private static void CreateJob<T>(IScheduler sched, string name, Func<TriggerBuilder, TriggerBuilder> triggerBuilder)where T:IJob
        {
          
            var job = JobBuilder.Create<T>().WithIdentity(name).Build();
            var trigger =
                TriggerBuilder.Create()
                              .ForJob(job);
            sched.ScheduleJob(job, triggerBuilder(trigger).Build());
        }
    }
    internal class HelloJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            Console.WriteLine("Hello!");
            Thread.Sleep(5000);
        }
    }
}
