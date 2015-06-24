using System;
using System.IO;
using System.Net;
using System.Collections;
using System.Drawing;
using System.Configuration;
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
        string MAX_RES = ConfigurationSettings.AppSettings["MaxResCameras"];

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
            //// recording images sequential
            RecordTimelapse();
        }

        private void RecordTimelapse()
        {
            int processId = Utils.TimelapseRunning(timelapse.ID);
            Utils.TimelapseLog(timelapse, "Recording Timelapse...");

            Process p = Process.GetProcessById(processId);
            if (p.Id > 0 && p.Id != Process.GetCurrentProcess().Id && !p.HasExited && !p.Responding)
            {
                Utils.TimelapseLog(timelapse, "Killing previous halted process#" + p.Id);
                Utils.KillProcess(p.Id, timelapse.ID);
            }
            else if (p.Id > 0 && p.Id != Process.GetCurrentProcess().Id && !p.HasExited && p.Responding)
            {
                Utils.TimelapseLog(timelapse, "EXIT: Timelapse recorder already running process#" + p.Id);
                ExitProcess();
            }

            while (true)
            {
                Stopwatch watch = Stopwatch.StartNew();
                DateTime utcBefore = DateTime.UtcNow;
                try
                {
                    //// GET THIS FROM API !!!
                    timelapse = TimelapseDao.Get(timelapse.Code);

                    if (timelapse.ID == 0)
                    {
                        Utils.TimelapseLog(timelapse, "EXIT: Timelapse.ID == 0");
                        ExitProcess();
                    }
                    if (timelapse.Status == (int)TimelapseStatus.Stopped && !timelapse.IsRecording)
                    {
                        Utils.TimelapseLog(timelapse, "EXIT: Timelapse.Status == Stopped");
                        ExitProcess();
                    }
                    if (fileError)
                    {
                        Utils.TimelapseLog(timelapse, "EXIT: Error in creating video file");
                        ExitProcess();
                    }

                    Program.WatermarkFile = timelapse.ID + ".png";
                    string mp4IdFileName = Path.Combine(Program.UpPath, timelapse.ID + ".mp4");
                    string mp4CodeFileName = Path.Combine(Program.UpPath, timelapse.Code + ".mp4");
                    string tempMp4FileName = Path.Combine(Program.TempPath, timelapse.Code + ".mp4");
                    string tempVideoFileName = Path.Combine(Program.TempPath, "temp" + timelapse.Code + ".mp4");
                    string baseMp4FileName = Path.Combine(Program.TempPath, "base" + timelapse.Code + ".mp4");

                    if (!string.IsNullOrEmpty(timelapse.WatermarkImage))
                    {
                        Utils.DoDownload(timelapse.WatermarkImage, Path.Combine(Program.TimelapseExePath, Program.WatermarkFile));
                        if (File.Exists(Path.Combine(Program.TimelapseExePath, Program.WatermarkFile)))
                            File.Copy(Path.Combine(Program.TimelapseExePath, Program.WatermarkFile), Path.Combine(Program.UpPath, Program.WatermarkFileName), true);
                    }

                    //// timelapse recorder is just initializing
                    if (!Program.Initialized)
                    {
                        DirectoryInfo d = new DirectoryInfo(Program.DownPath);
                        int fileCount = d.GetFiles("*.jpg").Length;
                        index = fileCount;
                        if (fileCount > 0 && fileCount != timelapse.SnapsCount)
                        {
                            Utils.TimelapseLog(timelapse, ">>> CreateVideoFromImages(" +
                                fileCount + " != " + timelapse.SnapsCount + ") index=" + index);
                            string lastImage = Path.Combine(Program.DownPath, index + ".jpg");
                            if (fileCount == (timelapse.SnapsCount-1) && File.Exists(lastImage)) {
                                ConcatenateVideoSingleImage(mp4IdFileName, tempMp4FileName, baseMp4FileName, tempVideoFileName, lastImage);
                                Utils.TimelapseLog(timelapse, "<<< AddedLastImageToVideo");
                            }
                            else {
                                CreateVideoFromImages(mp4IdFileName, baseMp4FileName);
                                Utils.TimelapseLog(timelapse, "<<< CreatedVideoFromImages");
                            }
                            File.Copy(mp4IdFileName, mp4CodeFileName, true);
                        }

                        Program.Initialized = true;
                        Utils.TimelapseLog(timelapse, "Timelapser Initialized @ " + Utils.ConvertFromUtc(DateTime.UtcNow, timelapse.TimeZone) + " (" + timelapse.FromDT + "-" + timelapse.ToDT + ")");
                    }

                    string imageFile = DownloadSnapshot();

                    if (Utils.StopTimelapse(timelapse))
                    {
                        TimelapseDao.UpdateStatus(timelapse.Code, (TimelapseStatus)timelapse.Status, timelapse.StatusTag, timelapse.TimeZone);
                        ExitProcess();
                    }
                    
                    if (!string.IsNullOrEmpty(imageFile))
                    {
                        //// generates video source file and updates timelapse status to Processing
                        if (!File.Exists(mp4IdFileName))
                            GenerateVideoSingleImage(mp4IdFileName, baseMp4FileName, imageFile);
                        else
                            ConcatenateVideoSingleImage(mp4IdFileName, tempMp4FileName, baseMp4FileName, tempVideoFileName, imageFile);
                        
                        File.Copy(mp4IdFileName, mp4CodeFileName, true);
                    }
                    else
                    {
                        //// could not get an image from camera so retry after 15 seconds
                        if (timeOutCount >= timeOutLimit)
                        {
                            string log = "Camera not accessible (tried " + timeOutCount + " times) ";
                            Utils.TimelapseLog(timelapse, log);
                            TimelapseDao.UpdateStatus(timelapse.Code, TimelapseStatus.Failed, log, timelapse.TimeZone);
                            ExitProcess();
                        }
                        // wait for x seconds before next retry
                        Utils.TimelapseLog(timelapse, "Retry after " + Settings.RetryInterval + " seconds");
                        Thread.Sleep(TimeSpan.FromMilliseconds(Settings.RetryInterval * 1000));
                    }

                    DateTime utcAfter = utcBefore.AddMilliseconds(watch.ElapsedMilliseconds);
                    if (timelapse.SnapsInterval == 1)
                    {
                        if (utcAfter.Hour == utcBefore.Hour && utcAfter.Minute == utcBefore.Minute)
                        {
                            int wait = 60 - utcAfter.Second;
                            Utils.TimelapseLog(timelapse, "Wait for " + wait + " seconds");
                            Thread.Sleep(TimeSpan.FromMilliseconds(wait * 1000));
                        }
                    }
                    else
                    {
                        TimeSpan span = utcAfter.AddMinutes(timelapse.SnapsInterval).Subtract(utcAfter);
                        Utils.TimelapseLog(timelapse, "Wait for " + span.TotalMinutes + " minutes");
                        Thread.Sleep(TimeSpan.FromMilliseconds(span.TotalMinutes * 60 * 1000));
                    }
                }
                catch (Exception x)
                {
                    Utils.TimelapseLog(timelapse, "ERR: RecordTimelapse(): " + x);
                    TimelapseDao.UpdateStatus(timelapse.Code, TimelapseStatus.Failed, "Failed with error - " + x.Message, timelapse.TimeZone);
                    Console.WriteLine("RecordTimelapse Error: " + x.Message);
                }
            }
        }

        protected string DownloadSnapshot()
        {
            try
            {
                Program.Camera = Program.Evercam.GetCamera(timelapse.CameraId);
            }
            catch (Exception x)
            {
                Utils.TimelapseLog(timelapse, "ERR: DownloadSnapshot: " + x.ToString());
                if (x.Message.ToLower().Contains("not found"))
                    TimelapseDao.UpdateStatus(timelapse.Code, TimelapseStatus.NotFound, "Camera details could not be retreived from Evercam", timelapse.TimeZone);
                else if (x.Message.ToLower().Contains("not exist"))
                    TimelapseDao.UpdateStatus(timelapse.Code, TimelapseStatus.NotFound, "Camera details could not be retreived from Evercam", timelapse.TimeZone);
                else if (x.Message.ToLower().Contains("offline"))
                    TimelapseDao.UpdateStatus(timelapse.Code, TimelapseStatus.Failed, "Camera not accessible", timelapse.TimeZone);

                ExitProcess();
            }
            
            //// /1/images/0.jpg
            string tempfile = Path.Combine(Program.DownPath, index + ".jpg");
            byte[] data = null;
            try
            {
                // instead of trying for X times, just try once other wise fetch from recording
                // store and returns live snapshot on evercam
                data = Program.Evercam.CreateSnapshot(timelapse.CameraId, Settings.EvercamClientName, true).ToBytes();
                Utils.TimelapseLog(timelapse, "Image data retrieved from Camera");
            }
            catch (Exception x)
            {
                    EvercamV2.Snapshot snap = Program.Evercam.GetLatestSnapshot(timelapse.CameraId, true);
                    data = snap.ToBytes();
                    if (data != null && data.Length > 0)
                    { }
                    else
                    {
                        timeOutCount++;
                        data = null;
                        Utils.TimelapseLog(timelapse, "Image count not be retrieved from Camera");
                    }
            }

            if (data != null)
            {
                try
                {
                    if (Storage.SaveFile(tempfile, data))
                    {
                        //// should calculate original image ratio and give to ResizeImage function
                        //// will resize the image and rename as source file. e.g. code.jpg
                        
                        //// No more resizing... only create <CODE>.jpg file for poster from given file and logo
                        MakePoster(tempfile);
                        timeOutCount = 0;
                        index++;

                        if (timelapse.DateAlways && timelapse.TimeAlways) 
                        {
                            timelapse.Status = (int)TimelapseStatus.Processing;
                            TimelapseDao.UpdateStatus(timelapse.Code, TimelapseStatus.Processing, "Now recording...", timelapse.TimeZone);
                        }
                        else
                        {
                            timelapse.Status = (int)TimelapseStatus.Scheduled;
                            TimelapseDao.UpdateStatus(timelapse.Code, TimelapseStatus.Scheduled, "Recording on schedule...", timelapse.TimeZone);
                        }

                        TimelapseDao.UpdateLastSnapshot(timelapse.Code, DateTime.UtcNow);

                        Utils.TimelapseLog(timelapse, "DownloadSnapshot - Image saved " + tempfile);
                    }
                    else
                    {
                        tempfile = "";
                        timeOutCount++;
                        Utils.TimelapseLog(timelapse, "DownloadSnapshot - Image not retrieved");
                        if (timeOutCount >= timeOutLimit)
                        {
                            string log = "Camera not accessible (tried " + timeOutCount + " times) ";
                            TimelapseDao.UpdateStatus(timelapse.Code, TimelapseStatus.Failed, log, timelapse.TimeZone);
                            ExitProcess();
                        }
                    }
                }
                catch (Exception x)
                {
                    tempfile = "";
                    timeOutCount++;
                    Utils.TimelapseLog(timelapse, "Image could not be not saved from Camera - Error: " + x.ToString());
                    if (timeOutCount >= timeOutLimit)
                    {
                        string log = "Camera not accessible (tried " + timeOutCount + " times) ";
                        TimelapseDao.UpdateStatus(timelapse.Code, TimelapseStatus.Failed, log, timelapse.TimeZone);
                        ExitProcess();
                    }
                }
            }
            else
            {
                tempfile = "";
                timeOutCount++;
                Utils.TimelapseLog(timelapse, "Image could not be retrieved from Camera");
                if (timeOutCount >= timeOutLimit)
                {
                    string log = "Camera not accessible (tried " + timeOutCount + " times) ";
                    TimelapseDao.UpdateStatus(timelapse.Code, TimelapseStatus.Failed, log, timelapse.TimeZone);
                    ExitProcess();
                }
            }
            return tempfile;
        }

        protected TimelapseVideoInfo CreateVideoFromImages(string output, string baseOutput)
        {
            Utils.TimelapseLog(timelapse, ">>> CreateVideoFromImages(" + output + ")");
            string[] maxres = MAX_RES.Split(new char[] {','});
            if (Array.IndexOf(maxres, timelapse.CameraId.ToLower()) > 0)
                RunProcess("-r " + timelapse.FPS + " -i " + Program.DownPath + @"\%00000d.jpg -c:v libx264 -r " + timelapse.FPS + " -profile:v main -pix_fmt yuv420p -y " + output);
            else
                RunProcess("-r " + timelapse.FPS + " -i " + Program.DownPath + @"\%00000d.jpg -c:v libx264 -r " + timelapse.FPS + " -profile:v main -preset slow -b:v 1000k -maxrate 1000k -bufsize 1000k -vf scale=-1:720 -pix_fmt yuv420p -y " + output);

            File.Copy(output, baseOutput, true);
            WatermarkVideo(baseOutput, output);

            TimelapseVideoInfo info = UpdateVideoInfo(output);
            Utils.TimelapseLog(timelapse, "<<< CreateVideoFromImages(" + output + ")");
            return info;
        }

        protected void GenerateVideoSingleImage(string output, string baseOutput, string imageFile)
        {
            Utils.TimelapseLog(timelapse, ">>> GenerateVideoSingleImage(" + output + ")");
            string[] maxres = MAX_RES.Split(new char[] { ',' });
            if (Array.IndexOf(maxres, timelapse.CameraId.ToLower()) > 0)
                RunProcess("-r " + timelapse.FPS + " -i " + imageFile + " -c:v libx264 -r " + timelapse.FPS + " -y -profile:v main -pix_fmt yuv420p " + output);
            else
                RunProcess("-r " + timelapse.FPS + " -i " + imageFile + " -c:v libx264 -r " + timelapse.FPS + " -y -profile:v main -preset slow -b:v 1000k -maxrate 1000k -bufsize 1000k -vf scale=-1:720 -pix_fmt yuv420p " + output);
            File.Copy(output, baseOutput, true);
            WatermarkVideo(baseOutput, output);

            UpdateVideoInfo(output);
            Utils.TimelapseLog(timelapse, "<<< GenerateVideoSingleImage(" + output + ")");
        }

        protected void ConcatenateVideoSingleImage(string mp4FileName, string tempMp4FileName, string baseMp4FileName, string tempVideoFileName, string imageFile)
        {
            try
            {
                /*Recovered*/
                Utils.TimelapseLog(timelapse, ">>> ConcatenateVideoSingleImage(" + mp4FileName + ")");
                string str = Path.Combine(Program.TempPath, this.timelapse.Code + ".txt");
                File.Delete(tempMp4FileName);
                File.Delete(tempVideoFileName);
                File.Delete(str);

                // create video file with single new snapshot
                string[] maxres = MAX_RES.Split(new char[] { ',' });
                if (Array.IndexOf(maxres, timelapse.CameraId.ToLower()) > 0)
                    RunProcess("-r " + timelapse.FPS + " -i " + imageFile + " -c:v libx264 -r " + timelapse.FPS + " -y -profile:v main -pix_fmt yuv420p " + tempVideoFileName);
                else
                    RunProcess("-r " + timelapse.FPS + " -i " + imageFile + " -c:v libx264 -r " + timelapse.FPS + " -y -profile:v main -preset slow -b:v 1000k -maxrate 1000k -bufsize 1000k -vf scale=-1:720 -pix_fmt yuv420p " + tempVideoFileName);

                // create text file that describes the files to be concatenated
                CreateConfigFile(baseMp4FileName, tempVideoFileName, str);

                // create a concatenated video file
                RunProcess("-f concat -i " + str + " -c copy " + tempMp4FileName);

                // saving a copy of original video as base
                File.Copy(tempMp4FileName, baseMp4FileName, true);

                WatermarkVideo(baseMp4FileName, mp4FileName);
                UpdateVideoInfo(mp4FileName);
                Utils.TimelapseLog(timelapse, "<<< ConcatenateVideoSingleImage(" + mp4FileName + ")");
            }
            catch (Exception x)
            {
                Utils.TimelapseLog(timelapse, "ERR: ConcatenateVideoSingleImage: " + x.ToString());
            }
        }

        private void WatermarkVideo(string input, string output)
        {
            string param = "";
            try
            {
                if (string.IsNullOrEmpty(timelapse.WatermarkImage))
                {
                    // just copy input file as it is to output location
                    File.Copy(input, output, true);
                    return;
                }
                else if (!File.Exists(Path.Combine(Program.TimelapseExePath, Program.WatermarkFile)))
                {
                    Utils.DoDownload(timelapse.WatermarkImage, Path.Combine(Program.TimelapseExePath, Program.WatermarkFile));
                    if (File.Exists(Path.Combine(Program.TimelapseExePath, Program.WatermarkFile)))
                        File.Copy(Path.Combine(Program.TimelapseExePath, Program.WatermarkFile), Path.Combine(Program.UpPath, Program.WatermarkFileName), true);
                }
                switch (timelapse.WatermarkPosition)
                {
                    case (int)WatermarkPosition.TopLeft:
                        param = "-i " + input + " -i " + Program.WatermarkFile + " -y -filter_complex \"overlay=10:10\" " + output;
                        break;
                    case (int)WatermarkPosition.TopRight:
                        param = "-i " + input + " -i " + Program.WatermarkFile + " -y -filter_complex \"overlay=(main_w-overlay_w)-10:10\" " + output;
                        break;
                    case (int)WatermarkPosition.BottomRight:
                        param = "-i " + input + " -i " + Program.WatermarkFile + " -y -filter_complex \"overlay=(main_w-overlay_w)-10:(main_h-overlay_h)-10\" " + output;
                        break;
                    case (int)WatermarkPosition.BottomLeft:
                        param = "-i " + input + " -i " + Program.WatermarkFile + " -y -filter_complex \"overlay=10:(main_h-overlay_h)-10\" " + output;
                        break;
                }
                RunProcess(param);
            }
            catch (Exception x)
            {
                Utils.TimelapseLog(timelapse, "ERR: Watermark Video: Process=" + param + Environment.NewLine + "Error: " + x.ToString());
            }
        }

        public TimelapseVideoInfo UpdateVideoInfo(string movieName)
        {
            string result = "";
            try
            {
                var p = new Process();
                string fileargs = " -threads 1 -i " + movieName + " -f null /dev/null ";

                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

                p.StartInfo.FileName = FfmpegExePath;
                p.StartInfo.Arguments = fileargs;

                p.Start();
                result = p.StandardError.ReadToEnd();

                Utils.KillProcess(p.Id, 0);
                
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

                //index1 = result.LastIndexOf("frame=", StringComparison.Ordinal) + ("frame= ").Length;
                //index2 = result.IndexOf("fps", index1, StringComparison.Ordinal) - 1;
                //if (index1 >= 0 && index2 >= 0)
                //    info.SnapsCount = int.Parse(result.Substring(index1, index2 - index1).Trim());

                // directly setting frames count equals to images count in directory
                DirectoryInfo d = new DirectoryInfo(Program.DownPath);
                info.SnapsCount = d.GetFiles("*.jpg").Length;

                FileInfo fi = new FileInfo(movieName);
                info.FileSize = fi.Length;

                TimelapseDao.UpdateFileInfo(timelapse.Code, info);

                return info;
            }
            catch (Exception ex)
            {
                Utils.TimelapseLog(timelapse, "ERR: UpdateVideoInfo(" + movieName + "): " + ex.ToString());
                // file is un-readable may be causing error like 'Invalid data found when processing input'
                // so move this bad copy of to /temp/ folder for backup and clean the space for new file
                string errVideoFileName = Path.Combine(Program.TempPath, "err" + timelapse.Code + ".mp4");
                if (File.Exists(errVideoFileName))
                    File.Delete(errVideoFileName);

                Utils.TimelapseLog(timelapse, "ERR: UpdateVideoInfo(" + movieName + "): " + Environment.NewLine + "Output: " + result);

                return new TimelapseVideoInfo();
            }
        }

        protected string RunProcess(string parameters)
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

            process.PriorityClass = ProcessPriorityClass.Idle;
            process.Refresh();

            string output = process.StandardError.ReadToEnd();
            
            try
            {
                if (!process.HasExited && process.Responding)
                    Utils.KillProcess(process.Id, 0);
                return "";
            }
            catch (Exception x)
            {
                Utils.TimelapseLog(timelapse, "ERR: RunProcess(" + parameters + ") " + x);
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
                Utils.TimelapseLog(timelapse, "ERR: CreateConfigFile(): " + x.Message);
            }
        }

        private void MakePoster(string filename)
        {
            try
            {
                string poster = Path.Combine(Program.UpPath, timelapse.Code + ".jpg");
                File.Copy(filename, poster, true);
                if (File.Exists(Path.Combine(Program.UpPath, Program.WatermarkFileName)))
                    Utils.WatermarkImage(filename, poster, Path.Combine(Program.UpPath, Program.WatermarkFileName), timelapse.WatermarkPosition);
                else
                    Utils.TimelapseLog(timelapse, "Watermark not found at " + Path.Combine(Program.UpPath, Program.WatermarkFile));
            }
            catch (Exception x)
            {
                Utils.TimelapseLog(timelapse, "ERR: MakePoster(" + filename + "): " + x);
            }
        }

        protected string CopyFfmpeg()
        {
            string PathDest = Path.Combine(Program.FfmpegCopyPath, "ffmpeg_" + timelapse.ID + ".exe");
            try
            {
                if (!File.Exists(PathDest))
                    File.Copy(Program.FfmpegExePath, PathDest, true);
            }
            catch (Exception x)
            {
                Utils.TimelapseLog(timelapse, "ERR: CopyFfmpeg(): " + x.Message);
                return "";
            }
            return PathDest;
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
                Utils.TimelapseLog(timelapse, "KillProcess on Exit: ffmpeg_" + timelapse.ID);
                Utils.KillProcess("ffmpeg_" + timelapse.ID);

                Environment.Exit(0);
            }
            catch (Exception x)
            {
                Utils.TimelapseLog(timelapse, "ERR: Recorder.ExitProcess: ", x);
                Console.WriteLine(DateTime.Now + " " + x.Message);
                Environment.Exit(0);
            }
        }
    }
}
