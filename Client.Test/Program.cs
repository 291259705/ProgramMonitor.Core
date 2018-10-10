using System;
using System.Threading;
using ProgramMonitor.Core;

namespace Client.Test
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var client = ListenClient.GetNewInstance(new ProgramInfo
                {
                    name = "任务1",
                    Version = "1.0"
                },
                /*"127.0.0.1:808"*/
                "127.0.0.1:18730"
                , 5);
            client.ReportStart();


            client = ListenClient.GetNewInstance(new ProgramInfo
                {
                    name = "任务2",
                    Version = "1.0"
                },
                /*"127.0.0.1:808"*/
                "127.0.0.1:18730"
                , 6);
            client.ReportStart();

            Console.Read();
        }
    }
}