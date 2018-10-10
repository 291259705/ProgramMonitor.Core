using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProgramMonitor.Core;

namespace Client.Test
{
    public class CallBack : IListenCall
    {
        public int Linsten(string programId)
        {
            Console.WriteLine(programId);
            return 0;
        }
    }
}
