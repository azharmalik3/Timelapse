using System;
using System.Linq;
using System.Configuration;

namespace BLL.Common
{
    public class Settings
    {
        public static string EvercamClientName
        {
            get { return ConfigurationSettings.AppSettings["EvercamClientName"]; }
        }

        public static string EvercamClientID
        {
            get { return ConfigurationSettings.AppSettings["EvercamClientID"]; }
        }

        public static string EvercamClientSecret
        {
            get { return ConfigurationSettings.AppSettings["EvercamClientSecret"]; }
        }

        public static string EvercamClientUri
        {
            get { return ConfigurationSettings.AppSettings["EvercamClientUri"]; }
        }

        public static string TimelapseAPIUrl
        {
            get { return ConfigurationSettings.AppSettings["TimelapseAPIUrl"]; }
        }

        public static string ConnectionString
        {
            get { return ConfigurationSettings.AppSettings["ConnectionString"]; }
        }

        public static string BucketUrl
        {
            get { return ConfigurationSettings.AppSettings["BucketUrl"]; }
        }

        public static string BucketName
        {
            get { return ConfigurationSettings.AppSettings["BucketName"]; }
        }

        public static string TimelapseExePath
        {
            get { return ConfigurationSettings.AppSettings["TimelapseExePath"]; }
        }

        public static string ShutdownProcessPath
        {
            get { return ConfigurationSettings.AppSettings["ShutdownProcessPath"]; }
        }

        public static string TimelapserProcessName
        {
            get { return ConfigurationSettings.AppSettings["TimelapserProcessName"]; }
        }

        public static string TempPath
        {
            get { return ConfigurationSettings.AppSettings["TempPath"]; }
        }

        public static string VideoWidth
        {
            get { return ConfigurationSettings.AppSettings["VideoWidth"]; }
        }

        public static string VideoHeight
        {
            get { return ConfigurationSettings.AppSettings["VideoHeight"]; }
        }

        public static string TempTimelapse
        {
            get { return ConfigurationSettings.AppSettings["TempTimelapse"]; }
        }

        public static string TempLogos
        {
            get { return ConfigurationSettings.AppSettings["TempLogos"]; }
        }

        public static string FfprobeExePath
        {
            get { return ConfigurationSettings.AppSettings["FfprobeExePath"]; }
        }

        public static string FfmpegExePath
        {
            get { return ConfigurationSettings.AppSettings["FfmpegExePath"]; }
        }

        public static string FfmpegCopyPath
        {
            get { return ConfigurationSettings.AppSettings["FfmpegCopyPath"]; }
        }

        public static string TimelapserCopyPath
        {
            get { return ConfigurationSettings.AppSettings["TimelapserCopyPath"]; }
        }

        public static string FtpServer
        {
            get { return ConfigurationSettings.AppSettings["FtpServer"]; }
        }

        public static string FtpUser
        {
            get { return ConfigurationSettings.AppSettings["FtpUser"]; }
        }

        public static string FtpPassword
        {
            get { return ConfigurationSettings.AppSettings["FtpPassword"]; }
        }

        public static string SiteServer
        {
            get { return ConfigurationSettings.AppSettings["SiteServer"]; }
        }

        public static string TimelapseServer
        {
            get { return ConfigurationSettings.AppSettings["TimelapseServer"]; }
        }

        public static string DevelopersList
        {
            get { return ConfigurationSettings.AppSettings["DevelopersList"]; }
        }

        public static string EmailSource
        {
            get { return ConfigurationSettings.AppSettings["EmailSource"]; }
        }

        public static string SmtpEmail
        {
            get { return ConfigurationSettings.AppSettings["SmtpEmail"]; }
        }

        public static string SmtpPassword
        {
            get { return ConfigurationSettings.AppSettings["SmtpPassword"]; }
        }

        public static string SmtpServer
        {
            get { return ConfigurationSettings.AppSettings["SmtpServer"]; }
        }

        public static string SmtpServerPort
        {
            get { return ConfigurationSettings.AppSettings["SmtpServerPort"]; }
        }

        public static string ExceptionFromEmail
        {
            get { return ConfigurationSettings.AppSettings["ExceptionFromEmail"]; }
        }

        public static int TimeoutLimit
        {
            get { return int.Parse(ConfigurationSettings.AppSettings["TimeoutLimit"]); }
        }

        public static int RetryInterval
        {
            get { return int.Parse(ConfigurationSettings.AppSettings["RetryInterval"]); }
        }

        public static int RecheckInterval
        {
            get { return int.Parse(ConfigurationSettings.AppSettings["RecheckInterval"]); }
        }
    }
}
