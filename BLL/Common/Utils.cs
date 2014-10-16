using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Configuration;
using System.Threading.Tasks;
using BLL.Entities;
using BLL.Dao;

namespace BLL.Common
{
    public class Utils
    {
        public static string SiteServer = ConfigurationSettings.AppSettings["SiteServer"];
        public static string WatermarkPrefix = ConfigurationSettings.AppSettings["WatermarkPrefix"];

        public static DateTime SQLMinDate = new DateTime(1900, 1, 1, 12, 0, 0);     // changed from 1753    to fix utc
        public static DateTime SQLMaxDate = new DateTime(8888, 12, 31, 23, 59, 59); // changed from 9999    conversion errors
        private const string Dictionary = "abcdefghiklmonpqrstuxzwy";
        private const string Dictionary2 = "abcdefghiklmonpqrstuxzwy1234567890";

        public static string RemoveSymbols(string str)
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                foreach (char c in str)
                {
                    if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '-' || c == '_')
                        sb.Append(c);
                }
            }
            catch { }
            return sb.ToString();
        }

        public static string GenerateRandomString(int length)
        {
            string s = "";
            Random r = new Random();
            for (int i = 0; i < length; i++)
            {
                int idx = r.Next(Dictionary.Length);
                s += Dictionary[idx];
            }
            return s;
        }

        public static string GeneratePassCode(int length)
        {
            string s = "";
            Random r = new Random();
            for (int i = 0; i < length; i++)
            {
                int idx = r.Next(Dictionary2.Length);
                s += Dictionary2[idx];
            }
            return s;
        }

        public static bool IsTimeBetween(DateTime time, DateTime? fromTime, DateTime? toTime)
        {
            if (fromTime == null || toTime == null)
                return true;

            double sec = (time - new DateTime(time.Year, time.Month, time.Day)).TotalMilliseconds;
            double fromSec =
                (fromTime.Value - new DateTime(fromTime.Value.Year, fromTime.Value.Month, fromTime.Value.Day)).
                    TotalMilliseconds;
            double toSec =
                (toTime.Value - new DateTime(toTime.Value.Year, toTime.Value.Month, toTime.Value.Day)).TotalMilliseconds;

            if (fromSec == toSec && toSec == 0)
                return true;

            if (toSec > fromSec)
                return sec >= fromSec && sec <= toSec;
            return sec >= toSec || sec <= fromSec; // if range is 23.00 - 5.00
        }

        public static TimeZoneInfo GetTimeZoneInfo(string tz)
        {
            TimeZoneInfo tzi = String.IsNullOrEmpty(tz) ? TimeZoneInfo.Utc : TimeZoneInfo.FindSystemTimeZoneById(tz);
            return tzi;
        }

        public static DateTime ConvertToUtc(DateTime dt, string timezone, bool useTryCatch = false)
        {
            TimeZoneInfo tzi = GetTimeZoneInfo(timezone);
            if (useTryCatch)
            {
                try
                {
                    return TimeZoneInfo.ConvertTimeToUtc(dt, tzi);
                }
                catch (Exception)
                {
                    // Possibly we get this error: The supplied DateTime represents an invalid time.  
                    // For example, when the clock is adjusted forward, any time in the period that is skipped is invalid.
                    // so we move time to one hour earlier
                    return TimeZoneInfo.ConvertTimeToUtc(dt.AddHours(-1), tzi);
                }
            }
            return TimeZoneInfo.ConvertTimeToUtc(dt, tzi);
        }

        public static int ConvertHourToUtc(int hour, string timezone)
        {
            TimeZoneInfo tzi = GetTimeZoneInfo(timezone);
            return TimeZoneInfo.ConvertTimeToUtc(new DateTime(2011, 01, 01, hour, 0, 0), tzi).Hour;
        }

        public static DateTime ConvertFromUtc(DateTime dt, string timezone)
        {
            TimeZoneInfo tzi = GetTimeZoneInfo(timezone);
            return TimeZoneInfo.ConvertTimeFromUtc(dt, tzi);
        }

        public static double GetBaseUtcOffsetMilliseconds(string timezone)
        {
            if (string.IsNullOrEmpty(timezone))
                return 0;
            TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById(timezone);
            return tz.BaseUtcOffset.TotalMilliseconds;
        }

        public static double GetTimeZoneOffSetMilliseconds(string timezone)
        {
            if (string.IsNullOrEmpty(timezone))
                return 0;
            var now = DateTime.UtcNow;
            var dt = ConvertFromUtc(now, timezone);
            return (dt - now).TotalMilliseconds;
        }

        public static double GetTimeZoneOffSetHours(string timezone)
        {
            if (string.IsNullOrEmpty(timezone))
                return 0;
            var now = DateTime.UtcNow;
            var dt = ConvertFromUtc(now, timezone);
            return (dt - now).TotalHours;
        }

        public static string GetCameraDayEndTimeInUtc(DateTime time, string timeZone)
        {
            string endTime = "";
            DateTime utcTime = ConvertToUtc(time, timeZone);
            if (time.Hour == 0 && time.Minute == 0 && time.Second == 0 && time.Millisecond == 0)
                endTime = utcTime.AddSeconds(86399).ToString("yyyyMMddHHmmssfff");
            //there are total 86400 seconds in a day, 86399 is used to keep the day same
            return endTime;
        }

        public static string GetNiceTime(DateTime dt)
        {
            TimeSpan ts = DateTime.UtcNow - dt;
            if (ts.TotalMinutes < 60)
                return "" + (int)ts.TotalMinutes + " minutes";
            if (ts.TotalHours < 24)
                return "" + (int)ts.TotalHours + " hours";
            if (ts.TotalDays < 30)
                return "" + (int)ts.TotalDays + " days";
            if (ts.TotalDays < 365)
                return "" + (int)ts.TotalDays / 30 + " months";
            if (ts.TotalDays > 365)
                return "" + (int)ts.TotalDays / 365 + " years";
            return "";
        }

        public static string GetSpace(long size)
        {
            if (size < 1024 * 1024)
                return "" + ((double)size / 1024).ToString("f1") + "Kb";
            if (size < 1024 * 1024 * 1024)
                return "" + ((double)size / (1024 * 1024)).ToString("f1") + "Mb";
            if (size > 1024 * 1024 * 1024)
            {
                double gbs = (double)size / (1024 * 1024 * 1024);
                if (gbs > 1024)
                {
                    double tbs = gbs / 1024;
                    return "" + tbs.ToString("f1") + "Tb";
                }
                return "" + gbs.ToString("f1") + "Gb";
            }
            return size + "B";
        }

        public static void FileLog(string msg)
        {
            try
            {
                System.IO.Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
                string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");

                if (!Directory.Exists(logPath))
                    Directory.CreateDirectory(logPath);

                string logFile = Path.Combine(logPath, DateTime.UtcNow.ToString("yyyy-MM-dd-HH") + ".txt");

                StreamWriter file = new StreamWriter(logFile, true);
                file.WriteLine(DateTime.UtcNow + "\t" + msg + "\n");
                file.Close();
            }
            catch (Exception) { }
        }

        public static void AppLog(string message)
        {
            try
            {
                // temporarily loggin in file as well
                FileLog(message);
                LogDao.Insert(new Log() { Message = message, Type = (int)TimelapseLogType.AppLog });
            }
            catch { }
        }

        public static void AppLog(string message, Exception x)
        {
            try
            {
                FileLog(message + ": " + x.ToString());
                // temporarily loggin in file as well
                LogDao.Insert(new Log() { Message = message, Details = x.InnerException.ToString(), Type = (int)TimelapseLogType.AppError });
            }
            catch { }
        }

        public static void TimelapseLog(Timelapse timelapse, string message)
        {
            try
            {
                System.IO.Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
                string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");

                if (!Directory.Exists(logPath))
                    Directory.CreateDirectory(logPath);

                string logFolder = Path.Combine(logPath, timelapse.ID.ToString());

                if (!Directory.Exists(logFolder))
                    Directory.CreateDirectory(logFolder);

                string logFile = Path.Combine(logFolder, DateTime.UtcNow.ToString("yyyy-MM-dd-HH") + ".txt");

                StreamWriter file = new StreamWriter(logFile, true);
                file.WriteLine(DateTime.UtcNow + "\t" + message + "\n");
                file.Close();

                //// temporarily logging in file as well
                //FileLog("Timelapse#" + timelapse.ID + ": " + message);
                //LogDao.Insert(new Log() { TimelapseId = timelapse.ID, CameraId = timelapse.CameraId, UserId = timelapse.UserId, Type = (int)TimelapseLogType.RecorderLog, Message = message });
            }
            catch { }
        }

        public static void TimelapseLog(Timelapse timelapse, Exception x)
        {
            TimelapseLog(timelapse, x.ToString());
            try
            {
                //// temporarily loggin in file as well
                //FileLog("Timelapse#" + timelapse.ID + ": " + x.ToString());
                //LogDao.Insert(new Log() { TimelapseId = timelapse.ID, CameraId = timelapse.CameraId, UserId = timelapse.UserId, Type = (int)TimelapseLogType.RecorderError, Message = x.Message, Details = x.InnerException.ToString() });
            }
            catch { }
        }

        public static void TimelapseLog(Timelapse timelapse, string message, Exception x)
        {
            TimelapseLog(timelapse, message + Environment.NewLine + x.ToString());
            try
            {
                //// temporarily loggin in file as well
                //FileLog("Timelapse#" + timelapse.ID + ": " + message + Environment.NewLine + x.ToString());
                //LogDao.Insert(new Log() { TimelapseId = timelapse.ID, CameraId = timelapse.CameraId, UserId = timelapse.UserId, Type = (int)TimelapseLogType.RecorderError, Message = message, Details = x.InnerException.ToString() });
            }
            catch { }
        }

        public static SnapshotData DoDownload(string url, string username, string password, bool useCredentials)
        {
            SnapshotData snapshot = new SnapshotData();
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.KeepAlive = false;

            if (useCredentials)
                request.Credentials = new NetworkCredential(username, password);

            using (var response = (HttpWebResponse)request.GetResponse())
            {
                using (Stream stream = response.GetResponseStream())
                {
                    using (MemoryStream ms = new MemoryStream(60000))
                    {
                        if (response.ContentType.Contains("image") && stream != null)
                        {
                            stream.CopyTo(ms);
                            snapshot.Data = ms.ToArray();
                            snapshot.ContentType = response.Headers["Content-Type"];
                        }
                    }
                }
            }

            return snapshot;
        }

        public static string ProcessRunning(int processId)
        {
            try
            {
                Process[] processlist = Process.GetProcesses();
                foreach (Process process in processlist)
                    if (process.Id == processId)
                        return process.ProcessName;
                return "";
            }
            catch (Exception x)
            {
                return "";
            }
        }

        public static int ProcessRunning(string processName)
        {
            try
            {
                Process[] processlist = Process.GetProcesses();
                foreach (Process process in processlist)
                    if (process.ProcessName.ToLower().Equals(processName.ToLower()))
                        return process.Id;
                return 0;
            }
            catch (Exception x)
            {
                return 0;
            }
        }
    }

    public class SnapshotData
    {
        public byte[] Data { get; set; }
        public string ContentType { get; set; }
    }
}
