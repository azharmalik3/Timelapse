using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Shutdown
{
    class Program
    {
        static void Main(string[] args)
        {
            bool timelapses = false;
            bool ffmpeg = false;
            bool app = false;

            if (args.Contains<string>("timelapse"))
                timelapses = true;
            if (args.Contains<string>("ffmpeg"))
                ffmpeg = true;
            if (args.Contains<string>("app"))
                app = true;
            if (args.Contains<string>("all"))
                app = ffmpeg = timelapses = true;
            Process[] processlist = Process.GetProcesses();
            foreach (Process process in processlist)
            {
                if (ffmpeg && process.ProcessName.ToLower().StartsWith("ffmpeg_"))
                {
                    Console.WriteLine("kill ffmpeg: " + process.ProcessName);
                    KillProcess(process.Id);
                }
                if (timelapses && process.ProcessName.ToLower().Contains("timelapser_"))
                {
                    Console.WriteLine("kill timelapser: " + process.ProcessName);
                    KillProcess(process.Id);
                }
                if (app && process.ProcessName.ToLower().Contains("timelapscreator"))
                {
                    Console.WriteLine("kill timelapscreator: " + process.MainWindowTitle);
                    KillProcess(process.Id);
                }
            }
        }

        static void KillProcess(int pid)
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
                //process.WaitForExit(500);
            }
            catch
            {
            }
        }
    }
}
