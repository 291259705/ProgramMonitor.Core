using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ProgramMonitor.Core
{
    public interface IListenCall
    {
        [OperationContract]
        int Linsten(string programId);
    }
}
