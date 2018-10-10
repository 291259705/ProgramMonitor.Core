using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ProgramMonitor.Core
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant, InstanceContextMode = InstanceContextMode.PerCall)]
    public class ListenService : ProgramMonitor.Core.IListenService
    {
        public void Start(ProgramInfo propInfo)
        {
            var listenCall = OperationContext.Current.GetCallbackChannel<IListenCall>();
            Common.SaveProgramStartInfo(propInfo, listenCall);
        }

        public void Stop(string programId)
        {
            Common.SaveProgramStopInfo(programId);
        }

        public void ReportRunning(ProgramInfo programInfo)
        {
            Console.WriteLine(string.Format("{0}-{1}", programInfo.Id, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));

            var listenCall = OperationContext.Current.GetCallbackChannel<IListenCall>();
            Common.SaveProgramRunningInfo(programInfo, listenCall);
        }

        public List<ProgramInfo> ListPrograms()
        {
            return Common.ProgramInfos.Values.OrderBy(c => c.name).ToList();
        }
    }
}
