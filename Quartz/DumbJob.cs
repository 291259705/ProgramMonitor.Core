using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;

namespace ConApp0926
{
    internal class DumbJob:IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            object result = dataMap.Get("JobSays");

            Console.WriteLine("DumbJob 开始执行...{0} ", result);
        }
    }
}
