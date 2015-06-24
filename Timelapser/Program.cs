using System;
using System.Diagnostics;
using System.Configuration;
using System.Threading;
using System.IO;
using System.Net;
using BLL.Dao;
using BLL.Entities;
using BLL.Common;
using EvercamV2;

namespace Timelapser
{
    class Program
    {
        public static string FilePath = Settings.BucketUrl + Settings.BucketName;
        public static string TimelapseExePath = Settings.TimelapseExePath;  // source copy
        public static string FfmpegExePath = Settings.FfmpegExePath;        // source copy
        public static string FfmpegCopyPath = Settings.FfmpegCopyPath;      // multiple copies
        public static string TempTimelapse = Settings.TempTimelapse;        // temporary timalapse image
        public static string WatermarkFile = "";
        public static string WatermarkFileName = "logo.png";
        public static string TempFile = "temp.jpg";
        public static string FtpUser = Settings.FtpUser;
        public static string FtpPassword = Settings.FtpPassword;
        public static Evercam Evercam = new Evercam();
        public static Camera Camera = new Camera();
        public static string UpPath;
        public static string DownPath;
        public static string TempPath;
        public static bool Initialized;
        static Timelapse tl = new Timelapse();
        static int RETRY_INTERVAL = int.Parse(ConfigurationSettings.AppSettings["RetryInterval"]);
        static int TRY_COUNT = int.Parse(ConfigurationSettings.AppSettings["TryCount"]);

        static void Main(string[] args)
        {
            //Utils.UpdateTimelapsesOnAzure();
            //Utils.CopyTimelapsesToAzure();

            AppDomain.CurrentDomain.UnhandledException += CurrentDomainUnhandledException;
            int tId = Convert.ToInt32(args[0]);
            
            // testing any timelapse
            //int tId = 398;
            
            Evercam.SANDBOX = Settings.EvercamSandboxMode;
            Evercam = new Evercam(Settings.EvercamClientID, Settings.EvercamClientSecret, Settings.EvercamClientUri);
            Timelapse timelapse = new Timelapse();
            try
            {
                tl = timelapse = TimelapseDao.Get(tId);
                string cleanCameraId = BLL.Common.Utils.RemoveSymbols(timelapse.CameraId);

                if (timelapse.ID == 0)
                {
                    TimelapseDao.UpdateStatus(timelapse.Code, TimelapseStatus.NotFound, "Timelapse details not found", timelapse.TimeZone);
                    ExitProcess("Timelapse not found. ID = " + tId);
                }
                if (!timelapse.IsRecording)
                {
                    TimelapseDao.UpdateStatus(timelapse.Code, TimelapseStatus.Stopped, "Recording stopped", timelapse.TimeZone);
                    ExitProcess("Timelapse stopped. ID = " + tId);
                }
                if (string.IsNullOrEmpty(cleanCameraId)) 
                {
                    TimelapseDao.UpdateStatus(timelapse.Code, TimelapseStatus.NotFound, "Camera details could not be retreived from Evercam", timelapse.TimeZone);
                    ExitProcess("Invalid Camera ID. Timelapse ID = " + tId + ", Camera ID = " + timelapse.CameraId);
                }

                //// User AuthToken is Unauthorized to access certain cameras, e.g. wayra_office
                //// may be shared cameras ?
                if (!string.IsNullOrEmpty(timelapse.OauthToken))
                    Evercam = new Evercam(timelapse.OauthToken);

                for (int i = 1; i <= TRY_COUNT; i++)
                {
                    //// tests x times if camera is available instantly otherwise exits
                    try
                    {
                        var data = Evercam.GetLiveImage(timelapse.CameraId);
                        break;
                    }
                    catch(Exception x) {
                        Utils.TimelapseLog(timelapse, "Main Error (try#" + i + "): " + x.ToString());
                        if (i < TRY_COUNT)
                            Thread.Sleep(RETRY_INTERVAL * 1000);    // 7 seconds
                        else
                        {
                            Snapshot snap = Evercam.GetLatestSnapshot(timelapse.CameraId, true);
                            byte[] data = snap.ToBytes();
                            if (data != null && data.Length > 0)
                                break;
                            else
                            {
                                BLL.Common.Utils.AppLog("Main Error in Timelapse#" + tId + ". Camera recording not found.");
                                TimelapseDao.UpdateStatus(timelapse.Code, TimelapseStatus.Failed, "Camera not accessible", timelapse.TimeZone);
                                ExitProcess("Camera not accessible");
                            }
                        }
                    }
                }

                Camera = Evercam.GetCamera(timelapse.CameraId);
                
                if (Camera == null || string.IsNullOrEmpty(Camera.ID))
                {
                    TimelapseDao.UpdateStatus(timelapse.Code, TimelapseStatus.NotFound, "Camera details could not be retreived from Evercam", timelapse.TimeZone);
                    ExitProcess("Camera not found. ID = " + timelapse.CameraId);
                }
                if (!Camera.IsOnline)
                {
                    TimelapseDao.UpdateStatus(timelapse.Code, TimelapseStatus.Failed, "Camera went offline", timelapse.TimeZone);
                    ExitProcess("Camera is offline. ID = " + timelapse.CameraId);
                }
                
                Console.Title = "Timelapse (#" + tId + ") - Camera (#" + cleanCameraId + ")";
                Console.WriteLine("Running Timelapse (#" + tId + ") - Camera (#" + cleanCameraId + ")");

                UpPath = Path.Combine(FilePath, cleanCameraId, timelapse.ID.ToString());
                DownPath = Path.Combine(FilePath, cleanCameraId, timelapse.ID.ToString(), "images");
                TempPath = Path.Combine(FilePath, cleanCameraId, timelapse.ID.ToString(), "temp");

                if (!Directory.Exists(FfmpegCopyPath))
                    Directory.CreateDirectory(FfmpegCopyPath);
                if (!Directory.Exists(FilePath))
                    Directory.CreateDirectory(FilePath);
                if (!Directory.Exists(UpPath))
                    Directory.CreateDirectory(UpPath);
                if (!Directory.Exists(DownPath))
                    Directory.CreateDirectory(DownPath);
                if (!Directory.Exists(TempPath))
                    Directory.CreateDirectory(TempPath);

                Recorder recorder = new Recorder(timelapse);
                recorder.Start();
            }
            catch (Exception x)
            {
                if (x.Message.ToLower().Contains("not found"))
                    TimelapseDao.UpdateStatus(timelapse.Code, TimelapseStatus.NotFound, "Camera details could not be retreived from Evercam", timelapse.TimeZone);
                else if (x.Message.ToLower().Contains("not exist"))
                    TimelapseDao.UpdateStatus(timelapse.Code, TimelapseStatus.NotFound, "Camera details could not be retreived from Evercam", timelapse.TimeZone);
                else
                    TimelapseDao.UpdateStatus(timelapse.Code, TimelapseStatus.Failed, "Camera not accessible", timelapse.TimeZone);

                BLL.Common.Utils.AppLog("Main Error in Timelapse#" + tId + ". ERR: " + x.Message);
                ExitProcess(x.Message);
            }
        }

        public static void ExitProcess(string msg)
        {
            try
            {
                BLL.Common.Utils.TimelapseLog(tl,"EXIT Recorder @ " + msg);
                Console.WriteLine("EXIT @ " + msg);
                Environment.Exit(0);
            }
            catch (Exception x)
            {
                BLL.Common.Utils.TimelapseLog(tl, "EXIT Recorder Error: " + x.ToString(), x);
                Console.WriteLine(DateTime.UtcNow + " " + x.Message);
                Environment.Exit(0);
            }
        }

        static void CurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            Environment.ExitCode = 10;
        }
    }
}
