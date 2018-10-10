using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using log4net;
using Timer = System.Timers.Timer;

namespace ProgramMonitor.Core
{
    public class Common
    {
        public static ConcurrentDictionary<string, ProgramInfo> ProgramInfos = null;
        public static ConcurrentDictionary<string, IListenCall> ListenCalls = null;

        public static ConcurrentBag<string> ManualStopProgramIds = null;
        public static Timer loadTimer = null;

        public static Timer listenTimer = null;
        public static SynchronizationContext SyncContext = null;

        public static Action<ProgramInfo, bool> RefreshListView;
        public static Action<ProgramInfo, bool> RefreshTabControl;

        public static int ClearInterval = 5;
        public static int ListenInterval = 2;
        public static bool Listening = false;
        public static string DbConnString = null;
        public static string[] NoticePhoneNos = null;
        public static string NoticeWxUserIds = null;

        public static ILog Logger = log4net.LogManager.GetLogger("ProgramMonitor");


        public const string SqlProviderName = "System.Data.SqlClient";


        public static void SaveProgramStartInfo(ProgramInfo programInfo, IListenCall listenCall)
        {

            programInfo.RunState = 0;
            ProgramInfos.AddOrUpdate(programInfo.Id, programInfo, (key, value) => programInfo);
            ListenCalls.AddOrUpdate(programInfo.Id, listenCall, (key, value) => listenCall);

            RefreshListView(programInfo, false);
            RefreshTabControl(programInfo, true);

            WriteLog(string.Format("程序名称：{0}，版本：{1} 已经启动运行", programInfo.name, programInfo.Version), false);
        }

        /// <summary>
        /// 日志处理
        /// </summary>
        /// <param name="msg">日志信息</param>
        /// <param name="isError">错误日志</param>
        private static void WriteLog(string msg, bool isError)
        {
            if (isError) Logger.Error(msg);
            else Logger.Info(msg);
        }

        internal static void SaveProgramStopInfo(string programId)
        {
            ProgramInfo programInfo;
            if (ProgramInfos.TryGetValue(programId, out programInfo))
            {
                programInfo.RunState = -1;
                RefreshListView(programInfo, false);

                IListenCall listenCall = null;
                ListenCalls.TryRemove(programId, out listenCall);
                RefreshTabControl(programInfo, true);

                WriteLog(string.Format("程序名：{0}，版本：{1}，已停止运行", programInfo.name, programInfo.Version), false);
            }
        }

        internal static void SaveProgramRunningInfo(ProgramInfo programInfo, IListenCall listenCall)
        {
            if (!ProgramInfos.ContainsKey(programInfo.Id) || !ListenCalls.ContainsKey(programInfo.Id))
            {
                SaveProgramStartInfo(programInfo, listenCall);
            }
            programInfo.RunState = 1;
            RefreshTabControl(programInfo, true);
            WriteLog(string.Format("程序名：{0}，版本：{1}，正在运行", programInfo.name, programInfo.Version), false);
        }

        /// <summary>
        /// 加载项目信息
        /// </summary>
        public static void AutoLoadProgramInfos()
        {
            if (loadTimer == null)
            {
                loadTimer = new System.Timers.Timer(1*60*1000);
                loadTimer.Elapsed += delegate(object sender, ElapsedEventArgs e)
                {
                    var timer = (Timer) sender;

                    try
                    {
                        timer.Stop();
                        foreach (var info in ProgramInfos)
                        {
                            var programInfo = info.Value;
                            RefreshListView(programInfo, false);
                        }
                    }
                    finally
                    {
                        if (Listening)
                        {
                            timer.Start();
                        }
                    }
                };
            }
            else
            {
                loadTimer.Interval = 1*60*1000;
            }
            loadTimer.Start();
        }

        /// <summary>
        /// 监听项目信息
        /// </summary>
        public static void AutoListenPrograms()
        {
            if (listenTimer == null)
            {
                listenTimer = new Timer(ListenInterval*60*1000);
                listenTimer.Elapsed += delegate(object sender, ElapsedEventArgs e)
                {
                    var timer = (Timer) sender;
                    try
                    {
                        timer.Stop();
                        foreach (var info in ListenCalls)
                        {
                            bool needUpdateStatInfo = false;
                            var listenCall = info.Value;
                            var programInfo = ProgramInfos[info.Key];
                            int oldState = programInfo.RunState;
                            try
                            {
                                programInfo.RunState = listenCall.Linsten(programInfo.Id);
                            }
                            catch (Exception)
                            {
                                if (programInfo.RunState != -1)
                                {
                                    programInfo.RunState = -1;
                                    needUpdateStatInfo = true;
                                }
                            }

                            if (programInfo.RunState == -1 && programInfo.StopTime.AddMinutes(5) < DateTime.Now)
                            {
                                //发送接受短信通知

                                programInfo.RunState = -1; // 重新刷新状态，重置停止时间
                            }

                            if (oldState != programInfo.RunState)
                            {
                                needUpdateStatInfo = true;

                                WriteLog(string.Format("程序名：{0}，版本：{1}，运行状态变更为：{2}",
                                    programInfo.name,
                                    programInfo.Version,
                                    programInfo.RunState), false);
                            }
                            RefreshTabControl(programInfo, needUpdateStatInfo);
                        }

                    }
                    finally
                    {
                        if (Listening)
                        {
                            timer.Start();
                        }
                    }
                };
            }
            else
            {
                listenTimer.Interval = ListenInterval*60*1000;
            }
            listenTimer.Start();
        }
    }
}
