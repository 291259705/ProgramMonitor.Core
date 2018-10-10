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

        public static int listenCount = 0;
        public static int ClearInterval = 5;
        public static double ListenInterval = 0.1;

        public static ILog Logger = log4net.LogManager.GetLogger("ProgramMonitor");
        private static readonly object _sysObj = new object();

        static Common()
        {
            Common.SyncContext = SynchronizationContext.Current;
            Common.ProgramInfos = new ConcurrentDictionary<string, ProgramInfo>();
            Common.ListenCalls = new ConcurrentDictionary<string, IListenCall>();
            Common.ManualStopProgramIds = new ConcurrentBag<string>();

            AutoLoadProgramInfos();
        }

        public static void SaveProgramStartInfo(ProgramInfo programInfo, IListenCall listenCall)
        {

            programInfo.RunState = 0;
            ProgramInfos.AddOrUpdate(programInfo.Id, programInfo, (key, value) => programInfo);
            ListenCalls.AddOrUpdate(programInfo.Id, listenCall, (key, value) => listenCall);

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

                IListenCall listenCall = null;
                ListenCalls.TryRemove(programId, out listenCall);

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

            WriteLog(string.Format("程序名：{0}，版本：{1}，正在运行", programInfo.name, programInfo.Version), false);
        }

        /// <summary>
        /// 定时加载项目信息(用于服务器监控处理)
        /// </summary>
        public static void AutoLoadProgramInfos()
        {
            if (loadTimer == null)
            {
                loadTimer = new System.Timers.Timer(5 * 1000);
                loadTimer.Elapsed += delegate(object sender, ElapsedEventArgs e)
                {
                    //定时自动加载监听项目，异常不做处理
                    try
                    {
                        AutoListenPrograms();
                    }
                    catch (Exception ex)
                    {
                        WriteLog(ex.Message, true);
                    }
                };
            }
            else
            {
                loadTimer.Interval = 5 * 1000;
            }

            loadTimer.Start();
        }

        /// <summary>
        /// 监听项目信息
        /// </summary>
        public static void AutoListenPrograms()
        {
            //定时调用客户端未初始，或是有新的变动
            if (listenTimer == null || listenCount != ProgramInfos.Values.Count)
            {
                lock (_sysObj)
                {
                    if (listenTimer == null || listenCount != ProgramInfos.Values.Count)
                    {
                        listenCount = ProgramInfos.Values.Count;

                        //重新初始化timer
                        listenTimer = new Timer(ListenInterval * 60 * 1000);
                        listenTimer.Elapsed += delegate(object sender, ElapsedEventArgs e)
                        {
                            try
                            {
                                //遍历监听接口
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

                                    //停止时间超过5分钟，发送通知
                                    if (programInfo.RunState == -1 && programInfo.StopTime.AddMinutes(5) < DateTime.Now)
                                    {
                                        //发送通知,未实现

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
                                }

                            }
                            catch (Exception ex)
                            {
                                WriteLog(ex.Message, true);
                            }
                        };

                        listenTimer.Start();
                    }
                }
            }
        }
    }
}