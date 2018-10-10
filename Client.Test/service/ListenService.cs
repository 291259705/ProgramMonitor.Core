using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using ProgramMonitor.Core;

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
            var listenCall = OperationContext.Current.GetCallbackChannel<IListenCall>();
            Common.SaveProgramRunningInfo(programInfo,listenCall);
        }

        public List<ProgramInfo> ListPrograms()
        {
            throw new NotImplementedException();
        }
    }
}