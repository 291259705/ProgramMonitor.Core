using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.ServiceModel;
using System.Text;
using System.Timers;
using log4net;

namespace ProgramMonitor.Core
{
    public class ListenClient : IListenCall
    {
        //静态变量
        private static readonly object syscObject = new object();
        private static ListenClient instance;
        private readonly int autoReportRunningInterval = 5;

        //变量
        private readonly ProgramInfo programInfo;

        private DuplexChannelFactory<IListenService> listenServiceFactory;
        private IListenService proxyListenService;
        private Timer reportRunningTimer;
        private string serviceHostAddr;

        private ListenClient(ProgramInfo programInfo, string serviceHostAddr = null, int autoReportRunningInterval = 300)
        {
            programInfo.Id = CreateProgramId();

            this.programInfo = programInfo;
            this.serviceHostAddr = serviceHostAddr;
            if (autoReportRunningInterval > 60) // 最低1分钟的间隔
                this.autoReportRunningInterval = autoReportRunningInterval;

            BuildAutoReportRuningTimer();
        }

        /// <summary>
        ///     回调方法
        /// </summary>
        /// <param name="programId">参数</param>
        /// <returns>返回状态</returns>
        public int Linsten(string programId)
        {
            var log = LogManager.GetLogger("ListenClient");
            Console.WriteLine("ListenClient 信息调用 ... {0} ..{1} ", programId, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            if (programInfo.Id.Equals(programId, StringComparison.OrdinalIgnoreCase))
                if (programInfo.RunState >= 0)
                    return 1;
            return -1;
        }

        /// <summary>
        ///     构建监听服务工厂
        /// </summary>
        private void BuildListenClientService()
        {
            if (listenServiceFactory == null)
            {
                if (string.IsNullOrEmpty(serviceHostAddr))
                    serviceHostAddr = ConfigurationManager.AppSettings["ServiceHostAddr"];

                var instanceContext = new InstanceContext(instance);
                var binding = new NetTcpBinding();
                binding.ReceiveTimeout = new TimeSpan(0, 5, 0);
                binding.SendTimeout = new TimeSpan(0, 5, 0);

                //使用时，请使用正确的接口
                var baseAddress = new Uri(string.Format("net.tcp://{0}/ListenService", serviceHostAddr)); //联调地址
                //baseAddress = new Uri("net.tcp://localhost/ListenService.svc"); // 发布之后的地址

                listenServiceFactory = new DuplexChannelFactory<IListenService>(instanceContext, binding, new EndpointAddress(baseAddress));
            }

            proxyListenService = listenServiceFactory.CreateChannel();
        }

        private void BuildAutoReportRuningTimer()
        {
            reportRunningTimer = new Timer(autoReportRunningInterval * 1000);
            reportRunningTimer.Elapsed += (s, e) => { ReportRunning(); };
        }

        /// <summary>
        ///     读取实例
        /// </summary>
        /// <param name="programInfo">工程项目信息</param>
        /// <param name="serviceHoseAddr">服务主机地址</param>
        /// <param name="autoReportRunningInterval">自动运行时间</param>
        /// <returns></returns>
        public static ListenClient GetNewInstance(ProgramInfo programInfo, string serviceHoseAddr = null, int autoReportRunningInterval = 300)
        {
            instance = new ListenClient(programInfo, serviceHoseAddr, autoReportRunningInterval);
            instance.BuildListenClientService();

            return instance;
        }

        public void ReportStart()
        {
            proxyListenService.Start(programInfo);
            reportRunningTimer.Start();
        }

        public void ReportStop()
        {
            proxyListenService.Stop(programInfo.Id);
            reportRunningTimer.Stop();
        }

        public void ReportRunning()
        {
            try
            {
                proxyListenService.ReportRunning(programInfo);
            }
            catch (Exception)
            {
                BuildListenClientService();
            }
        }

        /// <summary>
        ///     CreateProgramId方法，因为心跳监控系统主要是依据ProgramId来识别每个不同的程序，故ProgramId非常重要，
        ///     而我这里采用的是文件的 MD5值+进程数作为ProgramId，因为有些程序是允许开启多个的实例的，如果不加进程数，那么心跳监控系统就无法识别多个同一个程序到底是哪个
        /// </summary>
        /// <returns></returns>
        private string CreateProgramId()
        {
            var currProcess = Process.GetCurrentProcess();
            var procCount = Process.GetProcessesByName(currProcess.ProcessName).Length;
            var currentProgramPath = currProcess.MainModule.FileName;
            var id = new Random().Next(0, 100);
            return GetMD5HashFromFile(currentProgramPath) + "_" + procCount;  //同一个实例只有一个id,这里需要修改成需的业务
        }

        private string GetMD5HashFromFile(string fileName)
        {
            try
            {
                byte[] hashData = null;
                using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                {
                    MD5 md5 = new MD5CryptoServiceProvider();
                    hashData = md5.ComputeHash(fs);
                    fs.Close();
                }

                var sb = new StringBuilder();
                for (var i = 0; i < hashData.Length; i++)
                    //转换为16进制显示
                    sb.Append(hashData[i].ToString("x2"));
                return sb.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("GetMD5HashFromFile Error:" + ex.Message);
            }
        }
    }
}