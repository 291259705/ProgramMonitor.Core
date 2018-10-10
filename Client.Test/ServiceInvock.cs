using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ProgramMonitor.Core;

namespace Client.Test
{
    public class ServiceInvock
    {
        public void Invock()
        {
            Common.SyncContext = SynchronizationContext.Current;
            Common.ProgramInfos = new ConcurrentDictionary<string, ProgramInfo>();
            Common.ListenCalls = new ConcurrentDictionary<string, IListenCall>();
            Common.ManualStopProgramIds = new ConcurrentBag<string>();

            Common.RefreshListView = RefreshListView;
            Common.RefreshTabControl = RefreshTabControl;
        }

        private void RefreshListView(ProgramInfo programInfo, bool needUpdateStatInfo)
        {
            Common.SyncContext.Post(o =>
            {
                string listViewItemKey = string.Format("lvItem_{0}", programInfo.Id);

                Console.WriteLine("RefreshListView");

            }, null);
        }

        private void RefreshTabControl(ProgramInfo programInfo, bool needUpdateStatInfo)
        {
            Common.SyncContext.Post(o =>
            {
                Console.WriteLine("RefreshTabControl");
            }, null);
        }



    }
}
