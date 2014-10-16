using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using BLL.Dao;
using BLL.Entities;
using BLL.Common;
using EvercamV1;

namespace Timelapser
{
    class Program
    {
        public static string FilePath = Settings.BucketUrl + Settings.BucketName;
        public static string TimelapseExePath = Settings.TimelapseExePath;  // source copy
        public static string FfmpegExePath = Settings.FfmpegExePath;    // source copy
        public static string FfmpegCopyPath = Settings.FfmpegCopyPath;  // multiple copies
        public static string TempTimelapse = Settings.TempTimelapse;    // temporary timalapse image
        public static string WatermarkFile = "";
        public static string FtpUser = Settings.FtpUser;
        public static string FtpPassword = Settings.FtpPassword;
        public static Evercam Evercam = new Evercam();
        public static Camera Camera = new Camera();
        public static string UpPath;
        public static string DownPath;
        public static string TempPath;
        public static string LogFile;
        public static bool Initialized;
        static Timelapse tl = new Timelapse();

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainUnhandledException;
            int tId = Convert.ToInt32(args[0]);
            //int tId = 220;
            Evercam = new Evercam(Settings.EvercamClientID, Settings.EvercamClientSecret, Settings.EvercamClientUri);
            Timelapse timelapse = new Timelapse();
            try
            {
                tl = timelapse = TimelapseDao.Get(tId);

                string cleanCameraId = BLL.Common.Utils.RemoveSymbols(timelapse.CameraId);
                if (timelapse.ID == 0)
                {
                    TimelapseDao.UpdateStatus(timelapse.Code, TimelapseStatus.Failed, "Timelapse details not found", timelapse.TimeZone);
                    ExitProcess("Timelapse not found. ID = " + tId);
                }
                if (timelapse.IsRecording == false)
                {
                    TimelapseDao.UpdateStatus(timelapse.Code, TimelapseStatus.Failed, "Recording stopped", timelapse.TimeZone);
                    ExitProcess("Timelapse stopped. ID = " + tId);
                }
                if (string.IsNullOrEmpty(cleanCameraId)) 
                {
                    TimelapseDao.UpdateStatus(timelapse.Code, TimelapseStatus.NotFound, "Camera details could not be retreived from Evercam", timelapse.TimeZone);
                    ExitProcess("Invalid Camera ID. Timelapse ID = " + tId + ", Camera ID = " + timelapse.CameraId);
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
                if (Camera.External == null || string.IsNullOrEmpty(Camera.External.Host) || string.IsNullOrEmpty(Camera.External.Http.Jpg) || Camera.External.Http.Port == 0)
                {
                    TimelapseDao.UpdateStatus(timelapse.Code, TimelapseStatus.Failed, "Invalid camera snapshot URL", timelapse.TimeZone);
                    ExitProcess("Invalid camera snapshot URL. ID = " + timelapse.CameraId);
                }
                //// for testing only
                //var data = Program.Camera.GetLiveImage();
                
                Console.Title = "Timelapse (#" + tId + ") - Camera (#" + cleanCameraId + ")";
                Console.WriteLine("Running Timelapse (#" + tId + ") - Camera (#" + cleanCameraId + ")");
                //BLL.Common.Utils.FileLog("Starting Timelapse (#" + tId + ") - Camera (#" + cleanCameraId + ")");

                UpPath = Path.Combine(FilePath, cleanCameraId, timelapse.ID.ToString());
                DownPath = Path.Combine(FilePath, cleanCameraId, timelapse.ID.ToString(), "images");
                TempPath = Path.Combine(FilePath, cleanCameraId, timelapse.ID.ToString(), "temp");
                //LogFile = Path.Combine(UpPath, "logs.txt");

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
                //if (!File.Exists(LogFile))
                //    File.Create(LogFile);

                Recorder recorder = new Recorder(timelapse);
                recorder.Start();
            }
            catch (Exception x)
            {
                if (x.Message.ToLower().Contains("not found"))
                    TimelapseDao.UpdateStatus(timelapse.Code, TimelapseStatus.NotFound,
                        "Camera details could not be retreived from Evercam", timelapse.TimeZone);
                else if (x.Message.ToLower().Contains("not exist"))
                    TimelapseDao.UpdateStatus(timelapse.Code, TimelapseStatus.NotFound,
                        "Camera details could not be retreived from Evercam", timelapse.TimeZone);
                else if (x.Message.ToLower().Contains("offline"))
                    TimelapseDao.UpdateStatus(timelapse.Code, TimelapseStatus.Failed,
                        "Camera went offline", timelapse.TimeZone);

                BLL.Common.Utils.AppLog("MAIN Recorder Error in Timelapse#" + tId, x);
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
