using System;
using System.Configuration;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;
using BLL.Dao;
using BLL.Entities;
using BLL.Common;

namespace TimelapseApp
{
    public class Monitor
    {
        //private Timer _timer;
        private static Dictionary<int, TimelapseProcessInfo> _timelapseInfos;

        public void Start()
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            
            _timelapseInfos = new Dictionary<int, TimelapseProcessInfo>();
            //_timer = new Timer(CheckRequests, "", 0, 1000 * 60 * 5);    // 5 minutes
            CheckRequests();
        }

        protected void CheckRequests()
        {
            while (true)
            {
                //// replace this with API PROXY call
                //// ISSUE: API does not returns timelapse object details completely
                //// MAY BE: Due to some JSON Serializer length restriction
                //List<Timelapse> timelapses = TimelapseApiProxy.GetTimelapses();
                List<Timelapse> timelapses = TimelapseDao.GetList(null, null);

                string hello = "Timelapse Application Started with '" + timelapses.Count + "' requests @ " + DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss");
                Console.WriteLine(hello);
                Utils.AppLog(hello);

                if (timelapses.Count == 0)
                {
                    //_timer.Change(0, 1000 * 60 * 1);  // 1 minute
                    Thread.Sleep(1000 * 60 * 1);        // 1 minute
                    continue;
                }

                //_timer.Change(0, 1000 * 60 * 50);     // 5 minutes
                foreach (Timelapse timelapse in timelapses)
                {
                    //// for dubugging ONLY
                    //if (timelapse.ID != 26)
                    //    continue;
                    try
                    {
                        //// if timelapser has been closed due to some error or ended then 
                        //// kill its process and remove from dictionary
                        int pid = ProcessRunning(timelapse);
                        if (pid > 0 && timelapse.Status == (int)TimelapseStatus.Expired)
                        {
                            Utils.AppLog("KILL: " + timelapse.ID + " (" + timelapse.Title + ")");
                            KillProcess(pid, timelapse.ID);
                            _timelapseInfos[timelapse.ID].IsResponding = false;
                            _timelapseInfos[timelapse.ID].ProcessId = 0;
                            _timelapseInfos[timelapse.ID].Interval = 0;
                        }  

                        if (timelapse.Status != (int) TimelapseStatus.Expired)
                        {
                            if (pid > 0 && (!timelapse.IsRecording || timelapse.Status > (int) TimelapseStatus.Failed))
                            {
                                Utils.AppLog("KILL: " + timelapse.ID + " (" + timelapse.Title + ")");
                                KillProcess(pid, timelapse.ID);

                                _timelapseInfos[timelapse.ID].IsResponding = false;
                                _timelapseInfos[timelapse.ID].ProcessId = 0;
                                _timelapseInfos[timelapse.ID].Interval = 0;
                            }
                            else if (pid > 0 && timelapse.Status <= (int) TimelapseStatus.Failed)
                            {
                                if ((timelapse.DateAlways && !timelapse.TimeAlways) &&
                                    (DateTime.UtcNow.TimeOfDay < timelapse.FromDT.TimeOfDay ||
                                     DateTime.UtcNow.TimeOfDay > timelapse.ToDT.TimeOfDay))
                                {
                                    Utils.TimelapseLog(timelapse, "Timelapse out of schedule (" +
                                                                  Utils.ConvertFromUtc(timelapse.FromDT,
                                                                      timelapse.TimeZone) + "-" +
                                                                  Utils.ConvertFromUtc(timelapse.ToDT,
                                                                      timelapse.TimeZone) + ")");
                                    KillProcess(pid, timelapse.ID);
                                }
                                else if ((!timelapse.DateAlways && timelapse.TimeAlways) &&
                                         (DateTime.UtcNow < timelapse.FromDT ||
                                          DateTime.UtcNow > timelapse.ToDT))
                                {
                                    string log = "Timelapse out of schedule (" +
                                        Utils.ConvertFromUtc(timelapse.FromDT,
                                            timelapse.TimeZone) + "-" +
                                        Utils.ConvertFromUtc(timelapse.ToDT,
                                            timelapse.TimeZone) + ")";
                                    Utils.TimelapseLog(timelapse, log);
                                    if (DateTime.UtcNow > timelapse.ToDT)
                                        TimelapseDao.UpdateStatus(timelapse.Code, TimelapseStatus.Expired, log);
                                    KillProcess(pid, timelapse.ID);
                                }
                                else if ((!timelapse.DateAlways && !timelapse.TimeAlways) &&
                                         (DateTime.UtcNow < timelapse.FromDT ||
                                          DateTime.UtcNow > timelapse.ToDT))
                                {
                                    string log = "Timelapse out of schedule (" +
                                        Utils.ConvertFromUtc(timelapse.FromDT,
                                            timelapse.TimeZone) + "-" +
                                        Utils.ConvertFromUtc(timelapse.ToDT,
                                            timelapse.TimeZone) + ")";
                                    Utils.TimelapseLog(timelapse, log);
                                    if (DateTime.UtcNow > timelapse.ToDT)
                                        TimelapseDao.UpdateStatus(timelapse.Code, TimelapseStatus.Expired, log);
                                    KillProcess(pid, timelapse.ID);
                                }
                                // !!! else if (!_timelapseInfos.ContainsKey(timelapse.ID))
                                if (!_timelapseInfos.ContainsKey(timelapse.ID))
                                {
                                    Utils.AppLog("ADD: " + timelapse.ID + " (" + timelapse.Title + ")");
                                    TimelapseProcessInfo tpi = new TimelapseProcessInfo();
                                    tpi.ProcessId = pid;
                                    tpi.IsResponding = true;
                                    tpi.NextRun = Utils.SQLMinDate;
                                    tpi.Interval = timelapse.SnapsInterval;
                                    _timelapseInfos.Add(timelapse.ID, tpi);
                                }

                                TimelapseProcessInfo info = _timelapseInfos[timelapse.ID];
                                if (timelapse.SnapsInterval != info.Interval)
                                {
                                    Utils.AppLog("UPDATE: " + timelapse.ID + " (" + timelapse.Title + ")");
                                    _timelapseInfos[timelapse.ID].Interval = timelapse.SnapsInterval;
                                    _timelapseInfos[timelapse.ID].NextRun = Utils.SQLMinDate;

                                    // restarts for instant interval update
                                    KillProcess(pid, timelapse.ID); 
                                    StartTimelapser(timelapse);
                                }
                            }

                            if (pid == 0 && timelapse.IsRecording)
                            {
                                if (timelapse.Status <= (int) TimelapseStatus.Processing)
                                {
                                    Utils.AppLog("START: " + timelapse.ID + " (" + timelapse.Title + ")");
                                    StartTimelapser(timelapse);
                                }
                                else if (timelapse.Status == (int) TimelapseStatus.Failed)
                                {
                                    TimeSpan time = DateTime.UtcNow - timelapse.ModifiedDT;
                                    //// Retry every 5min in 1st hour, 15min in 2nd hour, 60min afterwards
                                    int wait = (time.TotalMinutes < 60 ? 5 : time.TotalMinutes < 120 ? 15 : 60);

                                    if (_timelapseInfos.ContainsKey(timelapse.ID))
                                    {
                                        if (DateTime.UtcNow >= _timelapseInfos[timelapse.ID].NextRun &&
                                            _timelapseInfos[timelapse.ID].NextRun != Utils.SQLMinDate)
                                        {
                                            if ((!timelapse.DateAlways || !timelapse.TimeAlways) &&
                                                (DateTime.UtcNow < timelapse.FromDT || DateTime.UtcNow > timelapse.ToDT))
                                            {
                                                Utils.TimelapseLog(timelapse, "New Timelapse out of schedule (" +
                                                                              Utils.ConvertFromUtc(timelapse.FromDT,
                                                                                  timelapse.TimeZone) + "-" +
                                                                              Utils.ConvertFromUtc(timelapse.ToDT,
                                                                                  timelapse.TimeZone) + ")");
                                            }
                                            else
                                            {
                                                Utils.AppLog("RETRY: " + timelapse.ID + " (" + timelapse.Title + ")");
                                                StartTimelapser(timelapse);
                                                _timelapseInfos[timelapse.ID].NextRun = DateTime.UtcNow.AddMinutes(wait);
                                            }
                                        }
                                        else if (_timelapseInfos[timelapse.ID].NextRun == Utils.SQLMinDate)
                                            _timelapseInfos[timelapse.ID].NextRun = DateTime.UtcNow.AddMinutes(wait);
                                    }
                                    else
                                    {
                                        Utils.AppLog("RESTART: " + timelapse.ID + " (" + timelapse.Title + ")");
                                        StartTimelapser(timelapse);
                                        //_timelapseInfos[timelapse.ID].NextRun = DateTime.UtcNow.AddMinutes(wait);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception x)
                    {
                        //_timer.Dispose();
                        Utils.AppLog("Monitor.CheckRequests Error (" + timelapse.ID + "): " + x.Message, x);
                    }
                }
                Thread.Sleep(1000 * 60 * Settings.RecheckInterval);
            }
        }

        private void StartTimelapser(Timelapse timelapse)
        {
            try
            {
                ProcessStartInfo process = new ProcessStartInfo(Settings.TimelapserExePath, timelapse.ID.ToString());
                process.UseShellExecute = true;
                process.WindowStyle = ProcessWindowStyle.Normal;    //ProcessWindowStyle.Hidden;

                Process currentProcess = Process.Start(process);

                TimelapseProcessInfo tpi = new TimelapseProcessInfo();
                tpi.ProcessId = currentProcess.Id;
                tpi.IsResponding = true;
                tpi.NextRun = Utils.SQLMinDate;
                tpi.Interval = timelapse.SnapsInterval;

                if (_timelapseInfos.ContainsKey(timelapse.ID))
                    _timelapseInfos[timelapse.ID] = tpi;
                else
                    _timelapseInfos.Add(timelapse.ID, tpi);
            }
            catch (Exception x)
            {
                Utils.AppLog("Monitor.StartTimelapser Error (" + timelapse.ID + "): " + x.Message);
            }
        }

        private int ProcessRunning(Timelapse timelapse)
        {
            try
            {
                int id = 0;
                if (_timelapseInfos.ContainsKey(timelapse.ID))
                    id = _timelapseInfos[timelapse.ID].ProcessId;
                if (id > 0)
                {
                    Process process = Process.GetProcessById(id);
                    if ((process.Id == _timelapseInfos[timelapse.ID].ProcessId) && 
                        process.ProcessName == Settings.TimelapserProcessName)
                    {
                        Utils.AppLog("Monitor.ProcessRunning (" + timelapse.ID + "): " + Settings.TimelapserProcessName + " (Status): " + process.Responding);
                        if (process.Responding)
                        {
                            _timelapseInfos[timelapse.ID].IsResponding = true;
                            _timelapseInfos[timelapse.ID].ProcessId = process.Id;
                            _timelapseInfos[timelapse.ID].Interval = timelapse.SnapsInterval;
                        }
                        else
                        {
                            try
                            {
                                _timelapseInfos[timelapse.ID].IsResponding = false;
                                _timelapseInfos[timelapse.ID].ProcessId = 0;
                                _timelapseInfos[timelapse.ID].Interval = 0;
                                KillProcess(process.Id, timelapse.ID);
                                id = 0;
                                Utils.AppLog("Monitor.ProcessRunning (" + timelapse.ID + "): " + Settings.TimelapserProcessName + " (Kill) " + process.Responding);
                            }
                            catch (Exception x)
                            {
                                id = 0;
                                Utils.AppLog("Monitor.ProcessRunning Error (" + timelapse.ID + "): " + Settings.TimelapserProcessName + " (Error): " + x.Message);
                            }
                        }
                    }
                }
                else
                {
                    Process[] processlist = Process.GetProcesses();
                    foreach (Process process in processlist)
                    {
                        //// this only works IF timelapse process is running with ProcessWindowStyle.Normal
                        if (!String.IsNullOrEmpty(process.MainWindowTitle) &&
                            process.ProcessName == Settings.TimelapserProcessName &&
                            process.MainWindowTitle.Equals("Timelapse (#" + timelapse.ID + ") - Camera (#" + timelapse.CameraId + ")"))
                        {
                            TimelapseProcessInfo tpi = new TimelapseProcessInfo();
                            tpi.ProcessId = process.Id;
                            tpi.IsResponding = process.Responding;
                            tpi.NextRun = Utils.SQLMinDate;
                            tpi.Interval = timelapse.SnapsInterval;

                            if (_timelapseInfos.ContainsKey(timelapse.ID))
                                _timelapseInfos[timelapse.ID] = tpi;
                            else
                                _timelapseInfos.Add(timelapse.ID, tpi);

                            if (process.Responding)
                            {
                                id = process.Id;
                                break;
                            }
                            else
                            {
                                id = 0;
                                KillProcess(process.Id, timelapse.ID);
                                Utils.AppLog("Monitor.ProcessRunning (" + timelapse.ID + "): " + Settings.TimelapserProcessName + " (Kill) " + process.Responding);
                                break;
                            }
                        }
                    }
                }
                return id;
            }
            catch (Exception x)
            {
                Utils.AppLog("Monitor.ProcessRunning Error (" + timelapse.ID + "): " + Settings.TimelapserProcessName + " (Checking Error): " + x.Message);
                return 0;
            }
        }

        private bool KillProcess(int pid, int tid)
        {
            try
            {
                ProcessStartInfo start = new ProcessStartInfo();
                start.FileName = "taskkill.exe";
                start.Arguments = "/pid " + pid + " /F";
                start.UseShellExecute = false;

                Process process = new Process();
                start.CreateNoWindow = true;
                process.StartInfo = start;
                process.Start();
                process.WaitForExit(500);
                
                return true;
            }
            catch (Exception x)
            {
                Utils.AppLog("KillProcess Error (" + tid + "): "+ x.Message);
                return false;
            }
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            Utils.AppLog("UnhandledException Error: " + ex.ToString());
            Environment.ExitCode = 10;
        }

        public class TimelapseProcessInfo
        {
            public TimelapseProcessInfo()
            {
                NextRun = Utils.SQLMinDate;
            }

            public DateTime NextRun { get; set; }
            public bool IsResponding { get; set; }
            public int Interval { get; set; }
            public int ProcessId { get; set; }
        }
    }
}