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

namespace TimelapseAppService
{
    public class Executor
    {
        private static Dictionary<int, TimelapseProcessInfo> _timelapseInfos;
        bool isServiceRunning = true;
        public string TimelapserExePath;

        public void Execute()
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            _timelapseInfos = new Dictionary<int, TimelapseProcessInfo>();

            // cleaning, if there is any previous garbage
            Shutdown("all");

            Utils.FileLog("Service execution started...");

            while (isServiceRunning)
            {
                // Utils.FileLog("Fetching pending timelapses..." + Environmentimelapse.NewLine);

                List<Timelapse> timelapses; //= new List<Timelapse>();
                try
                {
                    timelapses = TimelapseDao.GetList(null, null);

                    //// for testing purpose only
                    //timelapses = new List<Timelapse>();
                    //timelapses.Add(TimelapseDao.Get(220));
                    //Utils.FileLog("TESTING Timelapse Service..." + Environment.NewLine);
                }
                catch(Exception x)
                {
                    Utils.FileLog("Error fetching timelapses:" + x.Message);
                    Thread.Sleep(1000 * 10);        // 10 seconds
                    continue;
                }

                if (timelapses == null)
                {
                    Utils.FileLog("Timelapses could not be fetched !");
                    Thread.Sleep(1000 * 15);        // 15 seconds
                    continue;
                }

                if (timelapses.Count == 0)
                {
                    Utils.FileLog("No timelapse found !");
                    Thread.Sleep(1000 * 15);        // 15 seconds
                    continue;
                }

                string hello = "Found " + timelapses.Count + " timelapses";
                Console.WriteLine(hello);
                Utils.FileLog(hello);

                foreach (Timelapse timelapse in timelapses)
                {
                    try
                    {
                        if (timelapse.Status == (int)TimelapseStatus.NotFound)
                        {
                            //Utils.FileLog("Skipping timelapse... Camera has been removed at Evercam");
                            continue;
                        }

                        if (string.IsNullOrEmpty((TimelapserExePath = CopyTimelapser(timelapse.ID))))
                        {
                            Console.WriteLine("Unable to create copy of Timelapser.exe");
                            Utils.FileLog("Skipping timelapse... Unable to create copy of Timelapser.exe");
                            continue;
                        }
                        
                        //// checks if any recorder process for this timelapse is already running
                        int pid = ProcessRunning(timelapse);

                        // ToDT is in next day
                        DateTime from = new DateTime();
                        DateTime to = new DateTime();
                        if (timelapse.DateAlways)
                        {
                            from = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day,
                                timelapse.FromDT.Hour, timelapse.FromDT.Minute, timelapse.FromDT.Second);
                            to = new DateTime(from.Year, from.Month, from.Day,
                                timelapse.ToDT.Hour, timelapse.ToDT.Minute, timelapse.ToDT.Second).AddDays(1);
                        }
                        else
                        {
                            from = new DateTime(timelapse.FromDT.Year, timelapse.FromDT.Month, timelapse.FromDT.Day,
                                timelapse.FromDT.Hour, timelapse.FromDT.Minute, timelapse.FromDT.Second);
                            to = new DateTime(timelapse.ToDT.Year, timelapse.ToDT.Month, timelapse.ToDT.Day,
                                timelapse.ToDT.Hour, timelapse.ToDT.Minute, timelapse.ToDT.Second);
                        }
                        //from = Utils.ConvertToUtc(from, timelapse.TimeZone);
                        //to = Utils.ConvertToUtc(to, timelapse.TimeZone);

                        //// if timelapse recording is stopped then stop its recorder
                        if (!timelapse.IsRecording)
                        {
                            string log = "KILL: Timelapse#" + timelapse.ID + " Paused";
                            TimelapseDao.UpdateStatus(timelapse.Code, TimelapseStatus.Paused, "Recording Stopped", timelapse.TimeZone);
                            KillProcess(pid, timelapse.ID);
                            continue;
                        }
                        //// recorder is already running where as either user has paused recording or 
                        //// its scheduled/expired then kill the existing timelapse recorder.
                        //// new recorder should start as soon as user activate recording or changes the expiry date/time
                        else if (pid > 0 && (timelapse.Status == (int)TimelapseStatus.Scheduled))
                        {
                            if (timelapse.FromDT.Hour >= timelapse.ToDT.Hour)
                            {
                                if (!timelapse.DateAlways && timelapse.ToDT.Date <= timelapse.FromDT.Date)
                                {
                                    string log = "KILL: Timelapse#" + timelapse.ID + " Expired - Ext.";
                                    Utils.FileLog(log);
                                    TimelapseDao.UpdateStatus(timelapse.Code, TimelapseStatus.Expired, "Out of schedule", timelapse.TimeZone);
                                    KillProcess(pid, timelapse.ID);
                                    continue;
                                }
                                if ((DateTime.UtcNow < from || DateTime.UtcNow > to))
                                {
                                    Utils.FileLog("Out of ext. schedule (" + timelapse.FromDT + "-" + timelapse.ToDT + ")");
                                    if (timelapse.DateAlways)
                                        TimelapseDao.UpdateStatus(timelapse.Code, TimelapseStatus.Scheduled, "Recording on schedule", timelapse.TimeZone);
                                    else
                                        TimelapseDao.UpdateStatus(timelapse.Code, TimelapseStatus.Expired, "Out of schedule", timelapse.TimeZone);
                                    continue;
                                }
                            }
                            else if ((timelapse.DateAlways && !timelapse.TimeAlways) &&
                                (DateTime.UtcNow.TimeOfDay < timelapse.FromDT.TimeOfDay ||
                                DateTime.UtcNow.TimeOfDay > timelapse.ToDT.TimeOfDay))
                            {
                                string log = "KILL: Timelapse#" + timelapse.ID + " Scheduled";
                                Utils.FileLog(log);
                                TimelapseDao.UpdateStatus(timelapse.Code, TimelapseStatus.Scheduled, "Recording on schedule", timelapse.TimeZone);
                                KillProcess(pid, timelapse.ID);
                                continue;
                            }
                            else if ((!timelapse.DateAlways && !timelapse.TimeAlways) && 
                                (DateTime.UtcNow < timelapse.FromDT || DateTime.UtcNow > timelapse.ToDT))
                            {
                                string log = "KILL: Timelapse#" + timelapse.ID + " Scheduled";
                                Utils.FileLog(log);
                                TimelapseDao.UpdateStatus(timelapse.Code, TimelapseStatus.Scheduled, "Recording on schedule", timelapse.TimeZone);
                                KillProcess(pid, timelapse.ID);
                                continue;
                            }
                        }
                        else if (pid > 0 && (timelapse.Status == (int)TimelapseStatus.Expired))
                        {
                            string log = "KILL: Timelapse#" + timelapse.ID + " Expired";
                            Utils.FileLog(log);
                            TimelapseDao.UpdateStatus(timelapse.Code, TimelapseStatus.Expired, "Out of schedule", timelapse.TimeZone);
                            KillProcess(pid, timelapse.ID);
                            continue;
                        }
                        else if (pid > 0 && (timelapse.Status == (int)TimelapseStatus.NotFound))
                        {
                            string log = "KILL: Timelapse#" + timelapse.ID + " NotFound";
                            Utils.FileLog(log);
                            TimelapseDao.UpdateStatus(timelapse.Code, TimelapseStatus.NotFound, "Camera details could not be retreived from Evercam", timelapse.TimeZone);
                            KillProcess(pid, timelapse.ID);
                            continue;
                        }
                        //// recorder is already running so check if its expired or out of schedule
                        else if (pid > 0 && timelapse.Status <= (int)TimelapseStatus.Processing)
                        {
                            if (timelapse.FromDT.Hour >= timelapse.ToDT.Hour)
                            {
                                if (!timelapse.DateAlways && timelapse.ToDT.Date <= timelapse.FromDT.Date)
                                {
                                    string log = " (" + Utils.ConvertFromUtc(timelapse.ToDT, timelapse.TimeZone) + ")";
                                    Utils.FileLog("END: Timelapse#" + timelapse.ID + " Invalid timelapse. schedule " + log);
                                    KillProcess(pid, timelapse.ID);
                                    TimelapseDao.UpdateStatus(timelapse.Code, TimelapseStatus.Expired, "Out of schedule", timelapse.TimeZone);
                                    continue;
                                }
                                if ((DateTime.UtcNow < from || DateTime.UtcNow > to))
                                {
                                    Utils.FileLog("Out of ext. schedule (" + timelapse.FromDT + "-" + timelapse.ToDT + ")");
                                    if (timelapse.DateAlways)
                                        TimelapseDao.UpdateStatus(timelapse.Code, TimelapseStatus.Scheduled, "Recording on schedule", timelapse.TimeZone);
                                    else
                                        TimelapseDao.UpdateStatus(timelapse.Code, TimelapseStatus.Expired, "Out of schedule", timelapse.TimeZone);
                                    continue;
                                }
                            }
                            //// record everyday between a specific time
                            else if ((timelapse.DateAlways && !timelapse.TimeAlways) &&
                                (DateTime.UtcNow.TimeOfDay < timelapse.FromDT.TimeOfDay || 
                                DateTime.UtcNow.TimeOfDay > timelapse.ToDT.TimeOfDay))
                            {
                                string log = " (" + Utils.ConvertFromUtc(timelapse.ToDT, timelapse.TimeZone) + ")";
                                Utils.FileLog("END: Timelapse#" + timelapse.ID + " Scheduled - out of time " + log);
                                KillProcess(pid, timelapse.ID);
                                TimelapseDao.UpdateStatus(timelapse.Code, TimelapseStatus.Scheduled, "Recording on schedule", timelapse.TimeZone);
                                continue;
                            }
                            //// otherwise
                            else if (DateTime.UtcNow < timelapse.FromDT || DateTime.UtcNow > timelapse.ToDT)
                            {
                                string log = " (" + Utils.ConvertFromUtc(timelapse.ToDT, timelapse.TimeZone) + ")";
                                Utils.FileLog("END: Timelapse#" + timelapse.ID + " Scheduled - out of time " + log);
                                KillProcess(pid, timelapse.ID);
                                TimelapseDao.UpdateStatus(timelapse.Code, TimelapseStatus.Expired, "Out of schedule", timelapse.TimeZone);
                                
                                continue;
                            }

                            //// maybe timelapse record does not exist in dictionary so add it
                            if (!_timelapseInfos.ContainsKey(timelapse.ID))
                            {
                                //Utils.FileLog("ADDNEW: " + timelapse.ID + " (" + timelapse.Title + ")");
                                TimelapseProcessInfo tpi = new TimelapseProcessInfo();
                                tpi.ProcessId = pid;
                                tpi.IsResponding = true;
                                tpi.NextRun = Utils.SQLMinDate;
                                tpi.Interval = timelapse.SnapsInterval;
                                _timelapseInfos.Add(timelapse.ID, tpi);
                            }

                            //// if user has changed timelapse SnapsInterval then
                            //// kill current recorder and start new with updated SnapsInterval
                            TimelapseProcessInfo info = _timelapseInfos[timelapse.ID];
                            if (timelapse.SnapsInterval != info.Interval)
                            {
                                Utils.FileLog("KILLSTART: " + timelapse.ID + " (" + timelapse.Title + ")");
                                
                                // restarts for instant interval update
                                KillProcess(pid, timelapse.ID);
                                StartTimelapser(timelapse);
                            }
                        }
                        //// timelapse recording is enabled but no recorder is running currently
                        else if (pid == 0 && timelapse.IsRecording)
                        {
                            //// status is new or processing then just start new recorder
                            if (timelapse.Status <= (int)TimelapseStatus.Processing)
                            {
                                StartTimelapser(timelapse);
                            }
                            //// status is scheduled then check and start recorder if now its withing schedule
                            else if (timelapse.Status == (int)TimelapseStatus.Scheduled || 
                                timelapse.Status == (int)TimelapseStatus.Expired)
                            {
                                if (timelapse.DateAlways && timelapse.TimeAlways)
                                {
                                    string log = "RESTART: " + timelapse.ID + " (" + timelapse.Title + ")";
                                    StartTimelapser(timelapse);
                                    TimelapseDao.UpdateStatus(timelapse.Code, TimelapseStatus.Scheduled, "Recording on schedule", timelapse.TimeZone);
                                }
                                else if (timelapse.FromDT.Hour >= timelapse.ToDT.Hour)
                                {
                                    if ((DateTime.UtcNow >= from && DateTime.UtcNow < to))
                                    {
                                        string log = "RESTART: " + timelapse.ID + " (" + timelapse.Title + ")";
                                        StartTimelapser(timelapse);
                                        TimelapseDao.UpdateStatus(timelapse.Code, TimelapseStatus.Scheduled, "Recording on schedule", timelapse.TimeZone);
                                    }
                                }
                                else if ((timelapse.DateAlways && !timelapse.TimeAlways) &&
                                    DateTime.UtcNow.TimeOfDay >= timelapse.FromDT.TimeOfDay &&
                                    DateTime.UtcNow.TimeOfDay < timelapse.ToDT.TimeOfDay)
                                {
                                    string log = "RESTART: " + timelapse.ID + " (" + timelapse.Title + ")";
                                    StartTimelapser(timelapse);
                                    TimelapseDao.UpdateStatus(timelapse.Code, TimelapseStatus.Scheduled, "Recording on schedule", timelapse.TimeZone);
                                }
                                else
                                {
                                    if (DateTime.UtcNow >= timelapse.FromDT && DateTime.UtcNow < timelapse.ToDT)
                                    {
                                        string log = "RESTART: " + timelapse.ID + " (" + timelapse.Title + ")";
                                        StartTimelapser(timelapse);
                                        TimelapseDao.UpdateStatus(timelapse.Code, TimelapseStatus.Scheduled, "Recording on schedule", timelapse.TimeZone);
                                    }
                                }
                            }
                            //// status is failed so check its retry time
                            else if (timelapse.Status == (int)TimelapseStatus.Failed)
                            {
                                TimeSpan time = DateTime.UtcNow - timelapse.ModifiedDT;
                                //// Retry every 5min in 1st hour, 15min in 2nd hour, 60min afterwards
                                int wait = (time.TotalMinutes < 60 ? 5 : time.TotalMinutes < 120 ? 15 : 60);

                                if (_timelapseInfos.ContainsKey(timelapse.ID))
                                {
                                    if (DateTime.UtcNow >= _timelapseInfos[timelapse.ID].NextRun)
                                    {
                                        if (_timelapseInfos[timelapse.ID].NextRun != Utils.SQLMinDate)
                                        {
                                            if ((!timelapse.DateAlways || !timelapse.TimeAlways) &&
                                                (DateTime.UtcNow < timelapse.FromDT || DateTime.UtcNow > timelapse.ToDT))
                                            {
                                                //Utils.FileLog("New Timelapse (" + timelapse.ID + ") out of schedule (" +
                                                //    Utils.ConvertFromUtc(timelapse.FromDT, timelapse.TimeZone) + "-" + 
                                                //    Utils.ConvertFromUtc(timelapse.ToDT, timelapse.TimeZone) + ")");
                                            }
                                            else
                                            {
                                                Utils.FileLog("RETRY: " + timelapse.ID + " (" + timelapse.Title + ")");
                                                StartTimelapser(timelapse);
                                                _timelapseInfos[timelapse.ID].NextRun = DateTime.UtcNow.AddMinutes(wait);
                                            }
                                        }
                                        else if (_timelapseInfos[timelapse.ID].NextRun == Utils.SQLMinDate)
                                            _timelapseInfos[timelapse.ID].NextRun = DateTime.UtcNow.AddMinutes(wait);
                                    }
                                }
                                else
                                {
                                    StartTimelapser(timelapse);
                                }
                            }
                        }
                    }
                    catch (Exception x)
                    {
                        Utils.FileLog("Executor.CheckRequests Error (" + timelapse.ID + "): " + x.Message);
                    }
                }

                Thread.Sleep(1000 * 60 * Settings.RecheckInterval);     // RecheckInterval in minutes
            }
        }

        private void StartTimelapser(Timelapse timelapse)
        {
            try
            {
                // tests if camera details are available from Evercam before starting its recorder
                EvercamV1.Evercam evercam = new EvercamV1.Evercam(Settings.EvercamClientID, Settings.EvercamClientSecret, Settings.EvercamClientUri);
                EvercamV1.Camera camera = evercam.GetCamera(timelapse.CameraId);

                // if camera found then start its process
                if (camera.IsOnline)
                {
                    ProcessStartInfo process = new ProcessStartInfo(TimelapserExePath, timelapse.ID.ToString());
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

                    Utils.FileLog("Executor.StartTimelapser(" + timelapse.ID + ") Camera (" + camera.ID + ") started (" + timelapse.Title + ")");
                }
                else
                {
                    TimelapseDao.UpdateStatus(timelapse.Code, TimelapseStatus.Failed, "Camera went Offline", timelapse.TimeZone);
                    Utils.FileLog("Executor.StartTimelapser(" + timelapse.ID + ") Camera (" + camera.ID + ") is Offline at Evercam (" + timelapse.Title + ")");
                }
            }
            catch (Exception x)
            {
                if (x.Message.ToLower().Contains("not found")) 
                {
                    TimelapseDao.UpdateStatus(timelapse.Code, TimelapseStatus.NotFound, "Camera details Could not be retrieved from Evercam", timelapse.TimeZone);
                    Utils.FileLog("Executor.StartTimelapser(" + timelapse.ID + ") Error: Could not get camera (" + timelapse.CameraId
                        + ") details from Evercam (" + timelapse.Title + ")");
                }
                else if (x.Message.ToLower().Contains("not exist"))
                {
                    TimelapseDao.UpdateStatus(timelapse.Code, TimelapseStatus.NotFound, "Camera details Could not be retrieved from Evercam", timelapse.TimeZone);
                    Utils.FileLog("Executor.StartTimelapser(" + timelapse.ID + ") Error: Camera (" + timelapse.CameraId
                        + ") does not exist at Evercam (" + timelapse.Title + ")");
                }
                else if (x.Message.ToLower().Contains("offline"))
                {
                    TimelapseDao.UpdateStatus(timelapse.Code, TimelapseStatus.Failed, "Camera went Offline", timelapse.TimeZone);
                    Utils.FileLog("Executor.StartTimelapser(" + timelapse.ID + ") Error: Camera (" + timelapse.CameraId
                        + ") is offline at " + BLL.Common.Utils.ConvertFromUtc(timelapse.FromDT, timelapse.TimeZone) + ". (" + timelapse.Title + ")");
                }
                else
                    Utils.FileLog("Executor.StartTimelapser(" + timelapse.ID + ") (" + timelapse.Title + ") Error: " + x.Message);
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
                    if ((process.Id == _timelapseInfos[timelapse.ID].ProcessId) && process.ProcessName == TimelapserExePath)
                    {
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
                            }
                            catch (Exception x)
                            {
                                id = 0;
                                Utils.FileLog("Executor.ProcessRunning Error (" + process.Id + "): " + TimelapserExePath + " (Error): " + x.Message);
                            }
                        }
                    }
                }
                else
                {
                    Process[] processlist = Process.GetProcesses();
                    foreach (Process process in processlist)
                    {
                        if (process.ProcessName.ToLower().StartsWith("timelapser_"))
                        {
                            int tid = 0;
                            string _id = process.ProcessName.Substring(
                                process.ProcessName.IndexOf("_") + 1,
                                process.ProcessName.Length - (process.ProcessName.IndexOf("_") + 1));
                            if (int.TryParse(_id, out tid) && tid == timelapse.ID)
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
                                    //Utils.FileLog("Executor.ProcessRunning (" + process.Id + "): " + TimelapserExePath + " (Found) " + process.Responding);
                                    break;
                                }
                                else
                                {
                                    id = 0;
                                    KillProcess(process.Id, timelapse.ID);
                                    //Utils.FileLog("Executor.ProcessRunning (" + process.Id + "): " + TimelapserExePath + " (Kill) " + process.Responding);
                                    break;
                                }
                            }
                        }
                    }
                }
                return id;
            }
            catch (Exception x)
            {
                //Utils.FileLog("Executor.ProcessRunning(Timelapse timelapse) Error (" + TimelapserExePath + ") : " + x.Message);
                return 0;
            }
        }

        private bool KillProcess(int pid, int tid)
        {
            if (pid == 0) return false;
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

                //// remove this timelapse from dictionary untill its been added in next run
                //// so that the dictioanary always contains list of timelapses with currently running processes
                if (tid > 0)
                {
                    Utils.FileLog("KillFfmpeg: #" + tid);
                    KillFfmpeg(tid);
                    _timelapseInfos.Remove(tid);
                }

                return true;
            }
            catch (Exception x)
            {
                Utils.FileLog("KillProcess Error: " + x.Message);
                return false;
            }
        }

        protected void KillFfmpeg(int tid)
        {
            try
            {
                Process[] processlist = Process.GetProcesses();
                foreach (Process process in processlist)
                {
                    if (process.ProcessName.ToLower().Equals("ffmpeg_" + tid))
                    {
                        ProcessStartInfo start = new ProcessStartInfo();
                        start.FileName = "taskkill.exe";
                        start.Arguments = "/pid " + process.Id + " /F";
                        start.UseShellExecute = false;

                        Process p = new Process();
                        start.CreateNoWindow = true;
                        p.StartInfo = start;
                        p.Start();
                        p.WaitForExit(100);
                    }
                }
            }
            catch (Exception x)
            {
                Utils.FileLog("KillFfmpeg Error: " + x.Message);
            }
        }

        protected string CopyTimelapser(int id)
        {
            string ExeFile = Path.Combine(Settings.TimelapseExePath, "Timelapser.exe");
            string ConfigFile = Path.Combine(Settings.TimelapseExePath, "Timelapser.exe.config");
            string PathDest = Path.Combine(Settings.TimelapseExePath, "timelapser_" + id + ".exe");
            string ConfigDest = Path.Combine(Settings.TimelapseExePath, "timelapser_" + id + ".exe.config");
            try
            {
                // if already exists (and process is running) 
                // then kill the process and delete its exe
                if (File.Exists(PathDest) && !_timelapseInfos.ContainsKey(id))
                {
                    KillProcess(Utils.ProcessRunning("timelapser_" + id), 0);
                    try
                    {
                        File.Delete(PathDest);
                        File.Delete(ConfigDest);

                        File.Copy(ExeFile, PathDest, true);
                        File.Copy(ConfigFile, ConfigDest, true);
                    }
                    catch (Exception x)
                    {
                        Utils.FileLog("CopyTimelapser(" + id + ") Error in File.Delete/File.Copy: " + x.Message);
                    }
                }
                else if (!File.Exists(PathDest) && !_timelapseInfos.ContainsKey(id))
                {
                    File.Copy(ExeFile, PathDest, true);
                    File.Copy(ConfigFile, ConfigDest, true);
                }
            }
            catch (Exception x)
            {
                Utils.FileLog("CopyTimelapser(" + id + ") Error: " + x.Message);
                return "";
            }
            return PathDest;
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            Utils.FileLog("UnhandledException Error: " + ex.ToString());
            StopExecution();
            Environment.ExitCode = 10;
        }

        public void StopExecution()
        {
            isServiceRunning = false;
            //// also set service properties to run Shutdown.exe all 
            //// as external program in case of service failure
            Shutdown("all");
        }

        protected void Shutdown(string param)
        {
            try
            {
                Utils.FileLog("TimelapseAppService Shutdown " + param);
                ProcessStartInfo process = new ProcessStartInfo(Settings.ShutdownProcessPath, param);
                process.UseShellExecute = true;
                process.WindowStyle = ProcessWindowStyle.Hidden;

                Process currentProcess = Process.Start(process);
                currentProcess.WaitForExit();
            }
            catch (Exception x)
            {
                Utils.FileLog("TimelapseAppService Shutdown Error: " + x.Message);
            }
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
