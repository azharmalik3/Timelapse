using System;
using System.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;
using BLL.Common;

namespace TimelapseApp
{
    class Program
    {
        public static string FilePath = Settings.BucketUrl + Settings.BucketName;
        static void Main(string[] args)
        {
            try
            {
                Console.Title = "Timelapse Application";

                Utils.AppLog("Starting Timelapse Monitor Application");
                Console.WriteLine("Starting Timelapse Monitor Application");
                Console.WriteLine("");

                Monitor monitor = new Monitor();
                monitor.Start();

                Console.WriteLine("");
                Console.WriteLine("Press ENTER to exit...");
                Console.ReadLine();
            }
            catch (Exception x)
            {
                Utils.AppLog(x.Message, x);
            }
        }
    }
}
