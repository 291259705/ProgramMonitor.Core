using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ProgramMonitor.Core
{
    [ServiceContract(Namespace = "", SessionMode = SessionMode.Required, CallbackContract = typeof(IListenCall))]
    public interface IListenService
    {
        [OperationContract(IsOneWay = true)]
        void Start(ProgramInfo propInfo);

        [OperationContract(IsOneWay = true)]
        void Stop(string programId);

        [OperationContract(IsOneWay = true)]
        void ReportRunning(ProgramInfo programInfo);

        [OperationContract]
        List<ProgramInfo> ListPrograms();
    }
}