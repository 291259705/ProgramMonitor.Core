using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ProgramMonitor.Core
{
    [DataContract]
    public class ProgramInfo
    {
        [DataMember]
        public string Id { get; internal set; }

        [DataMember]

        public string name { get; set; }

        [DataMember]

        public string Version { get; set; }

        [DataMember]
        public string InstalledLocation { get; set; }

        [DataMember]

        public string Description { get; set; }

        [DataMember] private int runState = -1;


        /// <summary>
        /// 运行状态，-1:停止，0:表示启动，1:表示运行中
        /// </summary>
        public int RunState
        {
            get
            {
                return runState;
            }
            set
            {
                this.UpdateStateTime = DateTime.Now;
                if (value < 0)
                {
                    runState = -1;
                    this.StopTime = this.UpdateStateTime;
                }
                else if (value == 0)
                {
                    runState = 0;
                    this.StartTime = this.UpdateStateTime;
                }
                else
                {
                    runState = 1;
                }
            }
        }

        [DataMember]
        public DateTime UpdateStateTime { get; private set; }

        [DataMember]
        public DateTime StartTime { get; private set; }

        [DataMember]
        public DateTime StopTime { get; private set; }

    }
}