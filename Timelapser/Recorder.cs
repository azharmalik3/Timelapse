using System;
using System.IO;
using System.Net;
using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.Threading;
using BLL.Entities;
using BLL.Dao;
using BLL.Common;

namespace Timelapser
{
    public class Recorder
    {
        public string FfmpegExePath;
        bool fileError = false;
        int timeOutCount = 0;
        int timeOutLimit = Settings.TimeoutLimit;
        int index = 0;
        Size dimension = new Size(int.Parse(Settings.VideoWidth), int.Parse(Settings.VideoHeight));

        private string log = "";
        protected Timelapse timelapse { get; set; }

        public Recorder(Timelapse timelapse)
        {
            this.timelapse = timelapse;
        }

        public void Start()
        {
            if (timelapse.ID == 0)
            {
                Console.WriteLine("Timelapse details could not be found.");
                Utils.TimelapseLog(timelapse, "Exiting... Timelapse details could not be found.");
                ExitProcess();
                return;
            }

            if (string.IsNullOrEmpty((FfmpegExePath = CopyFfmpeg())))
            {
                Console.WriteLine("Unable to create copy of FFMPEG.exe.");
                Utils.TimelapseLog(timelapse, "Exiting... Unable to create copy of FFMPEG.exe.");
                ExitProcess();
                return;
            }

            //_timer = new Timer(timer_Elapsed, null, 0, 60000);  // timelapse.SnapsInterval * 60 * 1000    // mins * seconds * millis

            //// recording images sequential
            RecordTimelapse();
        }

        private void RecordTimelapse()
        {
            Utils.TimelapseLog(timelapse, "Recording Timelapse...");
            while (true)
            {
                try
                {
                    Stopwatch watch = new Stopwatch();
                    watch.Start();
                    
                    //// GET THIS FROM API !!!
                    timelapse = TimelapseDao.Get(timelapse.Code);

                    if (timelapse.ID == 0)
                    {
                        Utils.TimelapseLog(timelapse, "EXIT: timelapse.ID == 0 || timelapse.IsRecording == false");
                        ExitProcess();
                    }
                    if (timelapse.IsRecording == false)
                    {
                        TimelapseDao.UpdateStatus(timelapse.Code, TimelapseStatus.Paused, "Recording Stopped", timelapse.TimeZone);
                        Utils.TimelapseLog(timelapse, "EXIT: timelapse.IsRecording == false");
                        ExitProcess();
                    }
                    if (timelapse.Status == (int)TimelapseStatus.Expired)
                    {
                        TimelapseDao.UpdateStatus(timelapse.Code, TimelapseStatus.Paused, "Recording Stopped", timelapse.TimeZone);
                        Utils.TimelapseLog(timelapse, "EXIT: timelapse.Status == Expired");
                        ExitProcess();
                    }
                    if (fileError)
                    {
                        Utils.TimelapseLog(timelapse, "Error in creating video file");
                        ExitProcess();
                    }

                    string mp4FileName = Path.Combine(Program.UpPath, timelapse.Code + ".mp4");
                    string tempMp4FileName = Path.Combine(Program.TempPath, timelapse.Code + ".mp4");
                    string tempVideoFileName = Path.Combine(Program.TempPath, "temp" + timelapse.Code + ".mp4");
                    string baseMp4FileName = Path.Combine(Program.TempPath, "base" + timelapse.Code + ".mp4");

                    string watermark = "";
                    if (!string.IsNullOrEmpty(timelapse.WatermarkImage))
                    {
                        string path = WebUtility.UrlDecode(timelapse.WatermarkImage);
                        watermark = path.Replace(Utils.SiteServer, Utils.WatermarkPrefix).Replace(@"/", @"\\");
                        if (File.Exists(watermark))
                        {
                            FileInfo info = new FileInfo(watermark);
                            Program.WatermarkFile = timelapse.ID + info.Extension;
                            File.Copy(watermark, Path.Combine(Program.TimelapseExePath, Program.WatermarkFile), true);
                        }
                    }

                    //// timelapse recorder is just initializing
                    if (!Program.Initialized)
                    {
                        DirectoryInfo d = new DirectoryInfo(Program.DownPath);
                        int fileCount = d.GetFiles("*.jpg").Length;

                        if (fileCount > 0 && File.Exists(mp4FileName))
                        {
                            if (fileCount > timelapse.SnapsCount)
                            {
                                TimelapseVideoInfo timelapseVideoInfo = CreateVideoFromImages(mp4FileName, baseMp4FileName);
                                index = ((TimelapseVideoInfo)@timelapseVideoInfo).SnapsCount;
                                Utils.TimelapseLog(timelapse, "CreateVideoFromImages(" +
                                    fileCount + " > " + timelapse.SnapsCount + ") index=" + index);
                            }
                        }
                        else if (fileCount > 0)
                        {
                            TimelapseVideoInfo timelapseVideoInfo = CreateVideoFromImages(mp4FileName, baseMp4FileName);
                            index = ((TimelapseVideoInfo)@timelapseVideoInfo).SnapsCount;
                            Utils.TimelapseLog(timelapse, "CreateVideoFromImages(" +
                                    fileCount + " > '0') index=" + index);
                        }

                        Program.Initialized = true;
                        Utils.TimelapseLog(timelapse, "Timelapser Initialized @ " + Utils.ConvertFromUtc(DateTime.UtcNow, timelapse.TimeZone) + " (" + timelapse.FromDT + "-" + timelapse.ToDT + ")");
                    }
                    
                    if (timelapse.Status == (int)TimelapseStatus.New)
                    {
                        timelapse.Status = (int)TimelapseStatus.Processing;
                        TimelapseDao.UpdateStatus(timelapse.Code, TimelapseStatus.Processing, "Recording started...", timelapse.TimeZone);
                    }
                    else if (timelapse.FromDT.Hour >= timelapse.ToDT.Hour)
                    {
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

                        if (!timelapse.DateAlways && timelapse.ToDT.Date <= timelapse.FromDT.Date)
                        {
                            Utils.TimelapseLog(timelapse, "Invalid ext. schedule (" + timelapse.FromDT + "-" + timelapse.ToDT + ")");
                            TimelapseDao.UpdateStatus(timelapse.Code, TimelapseStatus.Expired, "Out of schedule", timelapse.TimeZone);
                            ExitProcess();
                        }
                        if (DateTime.UtcNow < from || DateTime.UtcNow > to)
                        {
                            Utils.TimelapseLog(timelapse, "Out of ext. schedule (" + timelapse.FromDT + "-" + timelapse.ToDT + ")");
                            if (timelapse.DateAlways)
                                TimelapseDao.UpdateStatus(timelapse.Code, TimelapseStatus.Scheduled, "Recording on schedule", timelapse.TimeZone);
                            else
                                TimelapseDao.UpdateStatus(timelapse.Code, TimelapseStatus.Expired, "Out of schedule", timelapse.TimeZone);
                            ExitProcess();
                        }
                    }
                    else if ((timelapse.DateAlways && !timelapse.TimeAlways))
                    {
                        if (DateTime.UtcNow.TimeOfDay < timelapse.FromDT.TimeOfDay || 
                            DateTime.UtcNow.TimeOfDay > timelapse.ToDT.TimeOfDay)
                        {
                            Utils.TimelapseLog(timelapse, "Out of schedule (" +
                                timelapse.FromDT + "-" + timelapse.ToDT + ")");
                            TimelapseDao.UpdateStatus(timelapse.Code, TimelapseStatus.Scheduled, "Recording on schedule", timelapse.TimeZone);
                            ExitProcess();
                        }
                    }
                    else if (DateTime.UtcNow < timelapse.FromDT || DateTime.UtcNow > timelapse.ToDT)
                    {
                        string log = "Out of schedule (" + timelapse.FromDT + "-" + timelapse.ToDT + ")";
                        Utils.TimelapseLog(timelapse, log);
                        TimelapseDao.UpdateStatus(timelapse.Code, TimelapseStatus.Expired, "Out of schedule", timelapse.TimeZone);
                        ExitProcess();
                    }

                    string imageFile = DownloadSnapshot();
                    
                    long retry = 1000 * Settings.RetryInterval;
                    long elapsed = watch.ElapsedMilliseconds;
                    long total = 1000 * 60 * timelapse.SnapsInterval;
                    long left = (total - elapsed > 0 ? total - elapsed : retry);
                    long wait = (string.IsNullOrEmpty(imageFile) ? retry / 1000 : left / 1000);

                    if (!string.IsNullOrEmpty(imageFile))
                    {
                        //// generates video source avi file and updates timelapse status to Processing
                        if (!File.Exists(mp4FileName))
                            GenerateVideoSingleImage(mp4FileName, baseMp4FileName, imageFile);
                        else
                            ConcatenateVideoSingleImage(mp4FileName, tempMp4FileName, baseMp4FileName, tempVideoFileName, imageFile);
                        wait = left / 1000;
                    }
                    else
                    {
                        //// could not get an image from camera so retry afte 10 seconds
                        wait = retry / 1000;
                        if (timeOutCount >= timeOutLimit)
                        {
                            string log = "Camera not reachable (tried " + timeOutCount + " times) ";
                            Utils.TimelapseLog(timelapse, log);
                            TimelapseDao.UpdateStatus(timelapse.Code, TimelapseStatus.Failed, log, timelapse.TimeZone);
                            ExitProcess();
                        }
                    }

                    Console.WriteLine("Waiting " + wait + " seconds");
                    Thread.Sleep(TimeSpan.FromMilliseconds(wait * 1000));
                }
                catch (Exception x)
                {
                    Utils.TimelapseLog(timelapse, x);
                    TimelapseDao.UpdateStatus(timelapse.Code, TimelapseStatus.Failed, "Failed with error - " + x.Message, timelapse.TimeZone);
                    Console.WriteLine("RecordTimelapse Error: " + x.Message);
                }
            }
        }

        protected string DownloadSnapshot()
        {
            //// /1/images/0.jpg
            string tempfile = Path.Combine(Program.DownPath, index + ".jpg");
            try
            {
                var data = Program.Evercam.GetLiveImage(Program.Camera.ID);
                if (Storage.SaveFile(tempfile, data.ToBytes()))
                {
                    //// should calculate original image ratio and give to ResizeImage function
                    //// will resize the image and rename as source file. e.g. code.jpg
                    tempfile = CopyResizeImage(tempfile, dimension);
                    timeOutCount = 0;
                    index++;

                    //if (timelapse.Status != (int)TimelapseStatus.Processing)
                    TimelapseDao.UpdateStatus(timelapse.Code, TimelapseStatus.Processing, "Now recording...", timelapse.TimeZone);

                    TimelapseDao.UpdateLastSnapshot(timelapse.Code, DateTime.UtcNow);
                }
                else
                {
                    throw new Exception("No Live Image");
                }
            }
            catch (Exception x)
            {
                //// retry - fallback
                EvercamV1.Camera camera = Program.Evercam.GetCamera(Program.Camera.ID);
                if (camera != null && camera.External != null && camera.External.Host != null && camera.IsOnline)
                {
                    SnapshotData snap = Utils.DoDownload(camera.External.Http.Jpg, camera.CameraUsername, camera.CameraPassword, true);
                    if (Storage.SaveFile(tempfile, snap.Data))
                    {
                        //// should calculate original image ratio and give to ResizeImage function
                        //// will resize the image and rename as source file. e.g. code.jpg
                        tempfile = CopyResizeImage(tempfile, dimension);
                        timeOutCount = 0;
                        index++;

                        //if (timelapse.Status != (int)TimelapseStatus.Processing)
                        TimelapseDao.UpdateStatus(timelapse.Code, TimelapseStatus.Processing, "Now recording...", timelapse.TimeZone);

                        TimelapseDao.UpdateLastSnapshot(timelapse.Code, DateTime.UtcNow);
                    }
                    else
                    {
                        tempfile = "";
                        timeOutCount++;
                        Utils.TimelapseLog(timelapse, "DownloadSnapshot Error: " + x.ToString());
                    }
                }
            }
            return tempfile;
        }

        protected TimelapseVideoInfo CreateVideoFromImages(string output, string baseOutput)
        {
            Utils.TimelapseLog(timelapse, ">>> CreateVideoFromImages(" + output + ")");
            RunProcess("-r " + timelapse.FPS + " -i " + Program.DownPath + @"\%00000d.jpg -c:v libx264 -r " + timelapse.FPS + " -pix_fmt yuv420p -y " + output, 1000);

            File.Copy(output, baseOutput, true);
            WatermarkVideo(baseOutput, output);
            
            TimelapseVideoInfo info = UpdateVideoInfo(output);
            Utils.TimelapseLog(timelapse, "<<< CreateVideoFromImages(" + output + ")");
            return info;
        }

        protected void GenerateVideoSingleImage(string output, string baseOutput, string imageFile)
        {
            Utils.TimelapseLog(timelapse, ">>> GenerateVideoSingleImage(" + output + ")");
            RunProcess("-r " + timelapse.FPS + " -i " + imageFile + " -c:v libx264 -r " + timelapse.FPS + " -pix_fmt yuv420p " + output, 500);

            File.Copy(output, baseOutput, true);
            WatermarkVideo(baseOutput, output);
            UpdateVideoInfo(output);
            Utils.TimelapseLog(timelapse, "<<< GenerateVideoSingleImage(" + output + ")");
        }

        protected void ConcatenateVideoSingleImage(string mp4FileName, string tempMp4FileName, string baseMp4FileName, string tempVideoFileName, string imageFile)
        {
            /*Recovered*/
            Utils.TimelapseLog(timelapse, ">>> ConcatenateVideoSingleImage(" + mp4FileName + ")");
            string str = Path.Combine(Program.TempPath, this.timelapse.Code + ".txt");
            File.Delete(tempMp4FileName);
            File.Delete(tempVideoFileName);
            File.Delete(str);
            
            // create video file with single new snapshot
            RunProcess("-r " + timelapse.FPS + " -i " + imageFile + " -c:v libx264 -r " + timelapse.FPS + " -y -pix_fmt yuv420p " + tempVideoFileName, 500);
            
            // create text file that describes the files to be concatenated
            CreateConfigFile(baseMp4FileName, tempVideoFileName, str);
            
            // create a concatenated video file
            RunProcess("-f concat -i " + str + " -c copy " + tempMp4FileName, 1000);

            // saving a copy of original video as base
            File.Copy(tempMp4FileName, baseMp4FileName, true);

            WatermarkVideo(baseMp4FileName, mp4FileName);
            UpdateVideoInfo(mp4FileName);
            Utils.TimelapseLog(timelapse, "<<< ConcatenateVideoSingleImage(" + mp4FileName + ")");
        }

        private void WatermarkVideo(string input, string output)
        {
            string param = "";
            try
            {
                if (string.IsNullOrEmpty(Program.WatermarkFile))
                {
                    //// just copy input file as it is to output location
                    File.Copy(input, output, true);
                    return;
                }
                switch (timelapse.WatermarkPosition)
                {
                    case (int)WatermarkPosition.TopLeft:
                        param = "-i " + input + " -y -vf \"movie=" + Program.WatermarkFile + " [watermark]; [in][watermark] overlay=10:10 [out]\" " + output;                        
                        break;
                    case (int)WatermarkPosition.TopRight:
                        param = "-i " + input + " -y -vf \"movie=" + Program.WatermarkFile + " [watermark]; [in][watermark] overlay=main_w-overlay_w-10:10 [out]\" " + output;
                        break;
                    case (int)WatermarkPosition.BottomLeft:
                        param = "-i " + input + " -y -vf \"movie=" + Program.WatermarkFile + " [watermark]; [in][watermark] overlay=10:main_h-overlay_h-10 [out]\" " + output;
                        break;
                    case (int)WatermarkPosition.BottomRight:
                        param = "-i " + input + " -y -vf \"movie=" + Program.WatermarkFile + " [watermark]; [in][watermark] overlay=main_w-overlay_w-10:main_h-overlay_h-10 [out]\" " + output;
                        break;
                }
                RunProcess(param, 500);
            }
            catch (Exception x)
            {
                Utils.TimelapseLog(timelapse, "Watermark Video Error: Process=" + param + Environment.NewLine + "Error: " + x.ToString());
            }
        }

        public TimelapseVideoInfo UpdateVideoInfo(string movieName)
        {
            Utils.TimelapseLog(timelapse, ">>> UpdateVideoInfo(" + movieName + ")");
            string result = "";
            try
            {
                var p = new Process();
                string fileargs = " -threads 1 -i " + movieName + " -f null /dev/null ";
                //string fileargs = " -i 'c://ffmpeg//u5sc526oq1.mp4' -show_frames | find /c 'pict_type'";     // for ffprobe

                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

                p.StartInfo.FileName = FfmpegExePath;
                p.StartInfo.Arguments = fileargs;

                p.Start();
                p.WaitForExit(500);
                result = p.StandardError.ReadToEnd();

                KillProcess(p.Id);
                
                TimelapseVideoInfo info = new TimelapseVideoInfo();

                int index1 = result.IndexOf("Duration: ", StringComparison.Ordinal);
                int index2 = index1 + 8;
                if (index1 >= 0 && index2 >= 0)
                    info.Duration = result.Substring(index1 + ("Duration: ").Length, index2 - index1);

                if (result.Contains("SAR"))
                {
                    index2 = result.IndexOf("SAR", StringComparison.Ordinal) - 1;
                    index1 = index2 - 10;
                    info.Resolution = result.Substring(index1, index2 - index1).Trim();
                }
                else if (result.Contains("yuv420p"))
                {
                    index1 = result.IndexOf("yuv420p, ", StringComparison.Ordinal) + ("yuv420p, ").Length;
                    index2 = result.IndexOf(", ", index1);
                    info.Resolution = result.Substring(index1, index2 - index1).Trim();
                }

                info.Resolution = info.Resolution.Replace(",", "");
                info.Resolution = info.Resolution.Replace(" ", "");

                index1 = result.LastIndexOf("frame=", StringComparison.Ordinal) + ("frame= ").Length;
                index2 = result.IndexOf("fps", index1, StringComparison.Ordinal) - 1;
                if (index1 >= 0 && index2 >= 0)
                    info.SnapsCount = int.Parse(result.Substring(index1, index2 - index1).Trim());

                FileInfo fi = new FileInfo(movieName);
                info.FileSize = fi.Length;

                TimelapseDao.UpdateFileInfo(timelapse.Code, info);

                Utils.TimelapseLog(timelapse, "<<< UpdateVideoInfo(" + movieName + ")");
                return info;
            }
            catch (Exception ex)
            {
                Utils.TimelapseLog(timelapse, "<<< UpdateVideoInfo(" + movieName + ") Error: " + ex.ToString());
                // file is un-readable may be causing error like 'Invalid data found when processing input'
                // so move this bad copy of to /temp/ folder for backup and clean the space for new file
                string errVideoFileName = Path.Combine(Program.TempPath, "err" + timelapse.Code + ".mp4");
                if (File.Exists(errVideoFileName))
                    File.Delete(errVideoFileName);
                //File.Copy(movieName, errVideoFileName, true);
                //File.Delete(movieName);

                Utils.TimelapseLog(timelapse, ex + Environment.NewLine + "Output: " + result);

                return new TimelapseVideoInfo();
            }
        }

        protected string RunProcess(string parameters, int wait)
        {
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = FfmpegExePath;
            start.Arguments = " -threads 1 " + parameters;
            start.UseShellExecute = false;
            start.RedirectStandardError = true;

            Process process = new Process();
            start.CreateNoWindow = true;
            process.StartInfo = start;
            process.Start();
            process.WaitForExit(wait);
            string output = process.StandardError.ReadToEnd();

            //// enable for debugging
            //if (parameters.Contains("[watermark]"))
            //    Utils.TimelapseLog(timelapse, output);
            
            try
            {
                KillProcess(process.Id);
                return "";
            }
            catch (Exception x)
            {
                Utils.TimelapseLog(timelapse, x);
                return x.Message;
            }
        }

        protected void CreateConfigFile(string mp4File, string newMp4File, string txtFileName)
        {
            try
            {
                if (!File.Exists(txtFileName))
                {
                    using (FileStream f = File.Create(txtFileName))
                    {
                        f.Close();
                    }
                }
                using (StreamWriter logs = new StreamWriter(txtFileName, true))
                {
                    logs.WriteLine("# this is a comment");
                    logs.WriteLine("file '" + mp4File + "'");
                    logs.WriteLine("file '" + newMp4File + "'");
                }
            }
            catch (Exception x)
            {
                Utils.TimelapseLog(timelapse, "CreateConfigFile() Error: " + x.Message);
            }
        }

        private string CopyResizeImage(string filename, Size size)
        {
            try
            {
                string first = Path.Combine(Program.UpPath, timelapse.Code + ".jpg");
                string final = Path.Combine(Program.UpPath, timelapse.ID + ".jpg");
                
                File.Copy(filename, final, true);
                
                // saves first image as CODE.jpg
                if (!File.Exists(first))
                    File.Copy(filename, first, true);

                return final;
            }
            catch (Exception x)
            {
                Utils.TimelapseLog(timelapse, x);
                Console.WriteLine("ResizeImage Error: " + x.ToString());
                return filename;
            }
        }

        protected string CopyFfmpeg()
        {
            string PathDest = Path.Combine(Program.FfmpegCopyPath, "ffmpeg_" + timelapse.ID + ".exe");
            try
            {
                if (!File.Exists(PathDest))
                    File.Copy(Program.FfmpegExePath, PathDest);
            }
            catch (Exception x)
            {
                Utils.TimelapseLog(timelapse, "CopyFfmpeg() Error: " + x.Message);
                return "";
            }
            return PathDest;
        }

        protected void KillProcess(int id)
        {
            try
            {
                ProcessStartInfo start = new ProcessStartInfo();
                start.FileName = "taskkill.exe";
                start.Arguments = "/pid " + id + " /F";
                start.UseShellExecute = false;

                Process process = new Process();
                start.CreateNoWindow = true;
                process.StartInfo = start;
                process.Start();
                process.WaitForExit(100);

                // retry killing if still exists
                string p = Utils.ProcessRunning(id);
                if (!string.IsNullOrEmpty(p))
                {
                    Utils.TimelapseLog(timelapse, "KillProcess failed: " + p);
                    process.Start();
                    process.WaitForExit(100);
                }
            }
            catch (Exception x)
            {
                Utils.FileLog("KillProcess Error: " + x.Message);
            }
        }

        protected void ExitProcess()
        {
            try
            {
                string PathDest = Path.Combine(Program.FfmpegCopyPath, "ffmpeg_" + timelapse.ID + ".exe");
                try
                {
                    //// Deletes timelapser copy of ffmpeg_id.exe
                    if (File.Exists(PathDest)) File.Delete(PathDest);

                    Utils.TimelapseLog(timelapse, "STOPPED @ (" + Utils.ConvertFromUtc(DateTime.UtcNow, timelapse.TimeZone) + ")");
                    Console.WriteLine("STOPPED @ (" + Utils.ConvertFromUtc(DateTime.UtcNow, timelapse.TimeZone) + ")");
                }
                catch (Exception x)
                {
                    Utils.TimelapseLog(timelapse, "Recorder.ExitProcess() Error", x);
                }
                // try killing if still exists
                int id = Utils.ProcessRunning("ffmpeg_" + timelapse.ID);
                if (id > 0)
                {
                    Utils.TimelapseLog(timelapse, "KillProcess on Exit: ffmpeg_" + timelapse.ID);
                    KillProcess(id);
                }
                Environment.Exit(0);
            }
            catch (Exception x)
            {
                Utils.TimelapseLog(timelapse, "Recorder.ExitProcess Error", x);
                Console.WriteLine(DateTime.Now + " " + x.Message);
                Environment.Exit(0);
            }
        }
    }
}
