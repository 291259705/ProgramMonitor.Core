using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using log4net.Core;
using log4net.Util;
using Quartz;
using Quartz.Impl;

namespace ConApp0926
{
    internal class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            Task<IScheduler> scheduler = StdSchedulerFactory.GetDefaultScheduler();
            scheduler.Result.Start();

            IJobDetail job1 = JobBuilder.Create<HelloJob>().WithIdentity("JobSays", "Hello World").Build();

            ITrigger trigger = TriggerBuilder.Create().WithIdentity("griggerKey", "group1")
                .StartNow().WithSimpleSchedule(x => x.WithIntervalInSeconds(5).RepeatForever()).Build();

            scheduler.Result.ScheduleJob(job1, trigger);

            IJobDetail job2 = JobBuilder.Create<DumbJob>()
                .WithDescription("这是一个描述")
                .WithIdentity("JobSays", "Hello World1")
                .UsingJobData("JobSays", "1")
                .Build();

            ITrigger trig2 = TriggerBuilder.Create()
                .WithIdentity("griggerKey", "group")
                .ForJob(job2)
                .WithCronSchedule("/5 * * ? * *").Build();

            scheduler.Result.ScheduleJob(job2, trig2);

            // remove job
            //scheduler.Result.UnscheduleJob(trig2.Key);

            //开始监听程序
            //var report = ListenClient.GetInstance(new ProgramInfo()
            //{
            //    name = "心跳监控系统",
            //    Version = "1.0"
            //});

            //report.ReportRunning();
            //report.ReportStart();

            Console.Read();

        }
    }
}
