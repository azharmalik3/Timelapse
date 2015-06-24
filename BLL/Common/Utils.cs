using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
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
        public static string SiteServer = Settings.SiteServer;
        public static string WatermarkPrefix = Settings.WatermarkPrefix;
        public static int WatermarkMargin = Settings.WatermarkMargin;

        public static DateTime SQLMinDate = new DateTime(1900, 1, 1, 12, 0, 0);         // changed from 1753    to fix utc
        public static DateTime SQLMaxDate = new DateTime(8888, 12, 31, 23, 59, 59);     // changed from 9999    conversion errors
        private const string Dictionary = "abcdefghiklmonpqrstuxzwy";
        private const string Dictionary2 = "abcdefghiklmonpqrstuxzwy1234567890";

        public static void CopyTimelapsesToAzure()
        {
            SqlConnection AzureConnection = new SqlConnection(ConfigurationSettings.AppSettings["ConnectionStringAzure"]);
            AzureConnection.Open();
            List<Timelapse> timelapses = TimelapseDao.GetList(null, null);
            foreach (Timelapse t in timelapses)
            {
                string query = @"SET IDENTITY_INSERT Timelapses ON INSERT INTO [dbo].[Timelapses] " +
                               "([Id],[UserId],[CameraId],[OauthToken],[Code],[Title],[Status],[Privacy],[FromDT],[ToDT],[DateAlways],[TimeAlways],[ServerIP],[TzId],[TimeZone],[SnapsInterval],[ModifiedDT],[EnableMD],[MDThreshold],[ExcludeDark],[DarkThreshold],[FPS],[IsRecording],[IsDeleted],[CreatedDT],[WatermarkImage],[WatermarkPosition]) " +
                               "VALUES " +
                               "(@Id,@UserId,@CameraId,@OauthToken,@Code,@Title,@Status,@Privacy,@FromDT,@ToDT,@DateAlways,@TimeAlways,@ServerIP,@TzId,@TimeZone,@SnapsInterval,@ModifiedDT,@EnableMD,@MDThreshold,@ExcludeDark,@DarkThreshold, @FPS,@IsRecording,@IsDeleted,@CreatedDT,@WatermarkImage,@WatermarkPosition) " +
                               "SELECT CAST(scope_identity() AS int) SET IDENTITY_INSERT Timelapses OFF";
                try
                {
                    var p0 = new SqlParameter("@Id", t.ID);
                    var p1 = new SqlParameter("@CameraId", t.CameraId);
                    var p2 = new SqlParameter("@UserId", t.UserId);
                    var p3 = new SqlParameter("@Code", t.Code);
                    var p4 = new SqlParameter("@Title", t.Title);
                    var p5 = new SqlParameter("@Status", t.Status);
                    var p6 = new SqlParameter("@Privacy", t.Privacy);
                    var p7 = new SqlParameter("@FromDT", (t.FromDT == null ? Utils.SQLMinDate : t.FromDT));
                    var p8 = new SqlParameter("@ToDT", (t.ToDT == null ? Utils.SQLMaxDate : t.ToDT));
                    var p9 = new SqlParameter("@ServerIP", t.ServerIP);
                    var p10 = new SqlParameter("@EnableMD", t.EnableMD);
                    var p11 = new SqlParameter("@MDThreshold", t.MDThreshold);
                    var p12 = new SqlParameter("@ExcludeDark", t.ExcludeDark);
                    var p13 = new SqlParameter("@DarkThreshold", t.DarkThreshold);
                    var p14 = new SqlParameter("@IsRecording", t.IsRecording);
                    var p15 = new SqlParameter("@IsDeleted", t.IsDeleted);
                    var p16 = new SqlParameter("@ModifiedDT", Utils.ConvertFromUtc(DateTime.UtcNow, t.TimeZone));
                    var p17 = new SqlParameter("@SnapsInterval", t.SnapsInterval);
                    var p18 = new SqlParameter("@TimeZone", t.TimeZone);
                    var p19 = new SqlParameter("@DateAlways", t.DateAlways);
                    var p20 = new SqlParameter("@TimeAlways", t.TimeAlways);
                    var p21 = new SqlParameter("@CreatedDT", Utils.ConvertFromUtc(DateTime.UtcNow, t.TimeZone));
                    var p22 = new SqlParameter("@TzId", t.TzId);
                    var p23 = new SqlParameter("@FPS", t.FPS);
                    var p24 = new SqlParameter("@OauthToken", t.OauthToken);
                    var p25 = new SqlParameter("@WatermarkImage", (string.IsNullOrEmpty(t.WatermarkImage) || t.WatermarkImage.Equals("-") ? "" : t.WatermarkImage));
                    var p26 = new SqlParameter("@WatermarkPosition", t.WatermarkPosition);

                    var list = new[] { p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15, p16, p17, p18, p19, p20, p21, p22, p23, p24, p25, p26 };
                    var cmd = new SqlCommand { CommandText = query, CommandType = CommandType.Text };
                    cmd.Parameters.AddRange(list);
                    
                    cmd.Connection = AzureConnection;
                    int result = (int)cmd.ExecuteScalar();
                    
                    cmd.Dispose();
                }
                catch (Exception ex)
                {

                }
            }
            AzureConnection.Close();
        }

        public static void UpdateTimelapsesOnAzure()
        {
            SqlConnection AzureConnection = new SqlConnection(ConfigurationSettings.AppSettings["ConnectionStringAzure"]);
            AzureConnection.Open();
            List<Timelapse> timelapses = TimelapseDao.GetList(null, null);
            foreach (Timelapse t in timelapses)
            {
                string query = @"UPDATE [dbo].[Timelapses] " +
                    "SET [SnapsCount] = @SnapsCount, [FileSize] = @FileSize, [Resolution] = @Resolution, [Duration] = @Duration, [LastSnapDT] = @LastSnapDT " +
                    "WHERE (Id = '" + t.ID + "')";
                try
                {
                    var p27 = new SqlParameter("@SnapsCount", t.SnapsCount);
                    var p28 = new SqlParameter("@Duration", t.Duration);
                    var p29 = new SqlParameter("@Resolution", t.Resolution);
                    var p30 = new SqlParameter("@FileSize", t.FileSize);
                    var p31 = new SqlParameter("@LastSnapDT", t.LastSnapDT);

                    var list = new[] { p27, p28, p29, p30, p31 };
                    var cmd = new SqlCommand { CommandText = query, CommandType = CommandType.Text };
                    cmd.Parameters.AddRange(list);

                    cmd.Connection = AzureConnection;
                    int result = cmd.ExecuteNonQuery();

                    cmd.Dispose();
                }
                catch (Exception ex)
                {

                }
            }
            AzureConnection.Close();
        }

        /// <summary>
        /// Checks if given timelapse needs to be started
        /// </summary>
        /// <param name="timelapse"></param>
        /// <returns></returns>
        public static bool StartTimelapse(Timelapse timelapse)
        {
            if (timelapse.Status == (int)TimelapseStatus.Failed)
            { 
                timelapse.StatusTag = "Camera not accessible";
                FileLog("Utils.StartTimelapse#" + timelapse.ID + " - Start Failed");
                return true;
            }

            if (timelapse.Status == (int)TimelapseStatus.Stopped && timelapse.IsRecording)
            { 
                timelapse.Status = (int)TimelapseStatus.Processing; timelapse.StatusTag = "Now recording...";
                FileLog("Utils.StartTimelapse#" + timelapse.ID + " - Start Stopped");
                return true;
            }

            // otherwise if new, processing, scheduled or expired
            if (timelapse.DateAlways && timelapse.TimeAlways)
            {
                timelapse.Status = (int)TimelapseStatus.Processing; timelapse.StatusTag = "Now recording...";
                FileLog("Utils.StartTimelapse#" + timelapse.ID + " - Start Recording Always");
                return true;
            } 
            else if (timelapse.DateAlways && !timelapse.TimeAlways)
            {
                if (timelapse.FromDT.Hour >= timelapse.ToDT.Hour)
                {
                    DateTime nextDay = DateTime.UtcNow.AddDays(1);
                    if (DateTime.UtcNow >= new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, timelapse.FromDT.Hour, timelapse.FromDT.Minute, 0) &&
                        DateTime.UtcNow < new DateTime(nextDay.Year, nextDay.Month, nextDay.Day, timelapse.ToDT.Hour, timelapse.ToDT.Minute, 59))
                    { 
                        timelapse.Status = (int)TimelapseStatus.Scheduled; timelapse.StatusTag = "Recording on schedule...";
                        FileLog("Utils.StartTimelapse#" + timelapse.ID + " - Start Recording Everyday Next");
                        return true; 
                    }
                }
                else if (DateTime.UtcNow >= new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, timelapse.FromDT.Hour, timelapse.FromDT.Minute, 0) &&
                    DateTime.UtcNow < new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, timelapse.ToDT.Hour, timelapse.ToDT.Minute, 59))
                {
                    timelapse.Status = (int)TimelapseStatus.Scheduled; timelapse.StatusTag = "Recording on schedule...";
                    FileLog("Utils.StartTimelapse#" + timelapse.ID + " - Start Recording Everyday");
                    return true; 
                }
            }
            else if (!timelapse.DateAlways && timelapse.TimeAlways)
            {
                if (DateTime.UtcNow >= new DateTime(timelapse.FromDT.Year, timelapse.FromDT.Month, timelapse.FromDT.Day, 0, 0, 0) &&
                    DateTime.UtcNow < new DateTime(timelapse.ToDT.Year, timelapse.ToDT.Month, timelapse.ToDT.Day, 23, 59, 59))
                { 
                    timelapse.Status = (int)TimelapseStatus.Scheduled; timelapse.StatusTag = "Recording on schedule...";
                    FileLog("Utils.StartTimelapse#" + timelapse.ID + " - Start Recording Anytime");
                    return true; 
                }
            }
            else if (!timelapse.DateAlways && !timelapse.TimeAlways)
            {
                if (DateTime.UtcNow >= new DateTime(timelapse.FromDT.Year, timelapse.FromDT.Month, timelapse.FromDT.Day, timelapse.FromDT.Hour, timelapse.FromDT.Minute, 0) &&
                    DateTime.UtcNow < new DateTime(timelapse.ToDT.Year, timelapse.ToDT.Month, timelapse.ToDT.Day, timelapse.ToDT.Hour, timelapse.ToDT.Minute, 59))
                { 
                    timelapse.Status = (int)TimelapseStatus.Scheduled; timelapse.StatusTag = "Recording on schedule...";
                    FileLog("Utils.StartTimelapse#" + timelapse.ID + " - Start Recording Range");
                    return true; 
                }
            }

            FileLog("Utils.StartTimelapse#" + timelapse.ID + " - NoStart Recording");
            return false;
        }

        /// <summary>
        /// Checks if given timelapse needs to be stopped
        /// </summary>
        /// <param name="timelapse"></param>
        /// <returns></returns>
        public static bool StopTimelapse(Timelapse timelapse)
        {
            if (timelapse.Status == (int)TimelapseStatus.NotFound)
            { 
                timelapse.StatusTag = "Camera details not found";
                FileLog("Utils.StopTimelapse#" + timelapse.ID + " - Stop Recording Not Found");
                return true; 
            }

            //if (timelapse.Status == (int)TimelapseStatus.Failed)
            //{ 
            //    timelapse.StatusTag = "Camera not accessible";
            //    FileLog("Utils.StopTimelapse#" + timelapse.ID + " - Stop Recording Failed");
            //    return true; 
            //}

            if (timelapse.Status == (int)TimelapseStatus.Stopped)
            { 
                timelapse.StatusTag = "Recording stopped";
                FileLog("Utils.StopTimelapse#" + timelapse.ID + " - Stop Recording Stopped");
                return true; 
            }

            //if (timelapse.Status == (int)TimelapseStatus.Expired)
            //{ 
            //    timelapse.StatusTag = "Out of schedule";
            //    FileLog("Utils.StopTimelapse#" + timelapse.ID + " - Stop Recording Expired");
            //    return true; 
            //}

            // otherwise if new, processing or scheduled
            if (timelapse.DateAlways && !timelapse.TimeAlways)
            {
                if (timelapse.FromDT.Hour >= timelapse.ToDT.Hour)
                {
                    DateTime nextDay = DateTime.UtcNow.AddDays(1);
                    if (DateTime.UtcNow >= new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, timelapse.ToDT.Hour, timelapse.ToDT.Minute, 59) &&
                        DateTime.UtcNow < new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, timelapse.FromDT.Hour, timelapse.FromDT.Minute, 00))
                    { 
                        timelapse.Status = (int)TimelapseStatus.Scheduled; timelapse.StatusTag = "Recording on schedule";
                        FileLog("Utils.StopTimelapse#" + timelapse.ID + " - NoStop Recording Everyday Next");
                        return false; 
                    }
                }
                else if (DateTime.UtcNow < new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, timelapse.FromDT.Hour, timelapse.FromDT.Minute, 0) ||
                    DateTime.UtcNow > new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, timelapse.ToDT.Hour, timelapse.ToDT.Minute, 59))
                { 
                    timelapse.Status = (int)TimelapseStatus.Scheduled; timelapse.StatusTag = "Recording on schedule";
                    FileLog("Utils.StopTimelapse#" + timelapse.ID + " - NoStop Recording Everyday");
                    return false; 
                }
            }
            else if (!timelapse.DateAlways && timelapse.TimeAlways)
            {
                if (DateTime.UtcNow < new DateTime(timelapse.FromDT.Year, timelapse.FromDT.Month, timelapse.FromDT.Day, 0, 0, 0) ||
                    DateTime.UtcNow >= new DateTime(timelapse.ToDT.Year, timelapse.ToDT.Month, timelapse.ToDT.Day, 23, 59, 59))
                { 
                    timelapse.Status = (int)TimelapseStatus.Expired; timelapse.StatusTag = "Out of schedule";
                    FileLog("Utils.StopTimelapse#" + timelapse.ID + " - Stop Recording Anytime - Expired");
                    return true; 
                }
            }
            else if (!timelapse.DateAlways && !timelapse.TimeAlways)
            {
                if (DateTime.UtcNow.Date >= timelapse.FromDT.Date && DateTime.UtcNow.Date <= timelapse.ToDT.Date)
                { 
                    timelapse.Status = (int)TimelapseStatus.Scheduled; timelapse.StatusTag = "Recording on schedule";
                    FileLog("Utils.StopTimelapse#" + timelapse.ID + " - NoStop Recording Range");
                    return false; 
                }
                else
                { 
                    timelapse.Status = (int)TimelapseStatus.Expired; timelapse.StatusTag = "Out of schedule";
                    FileLog("Utils.StopTimelapse#" + timelapse.ID + " - Stop Recording Range - Expired");
                    return true; 
                }
            }

            FileLog("Utils.StopTimelapse#" + timelapse.ID + " - NoStop Recording");
            return false;
        }

        public static byte[] WatermarkImage(string input, string output, string logofile, int logoposition)
        {
            byte[] bytes = new byte[] { };
            try
            {
                Image image = Image.FromFile(input);
                Graphics g = System.Drawing.Graphics.FromImage(image);
                MemoryStream stream = new MemoryStream();

                if (!string.IsNullOrEmpty(logofile))
                    InsertLogo(g, logofile, logoposition, image.Width, image.Height);

                using (MemoryStream ms = new MemoryStream())
                {
                    image.Save(ms, ImageFormat.Jpeg);
                    bytes = ms.ToArray();
                }

                g.Dispose();
                image.Dispose();
                stream.Close();
                stream.Dispose();

                if (!string.IsNullOrEmpty(output))
                {
                    Storage.SaveFile(output, bytes);
                }
                return bytes;
            }
            catch (Exception x)
            {
                FileLog("Utils.WatermarkImage: " + x.Message + Environment.NewLine + x.InnerException.ToString());
                return bytes;
            }
        }

        public static byte[] WatermarkImage(int timelapseId, byte[] imagedata, string output, string logofile, int logoposition)
        {
            byte[] bytes = new byte[] { };
            try
            {
                MemoryStream stream = new MemoryStream(imagedata);
                Image image = System.Drawing.Image.FromStream(stream);
                Graphics g = System.Drawing.Graphics.FromImage(image);

                if (!string.IsNullOrEmpty(logofile))
                    InsertLogo(timelapseId, g, logofile, logoposition, image.Width, image.Height);

                using (MemoryStream ms = new MemoryStream())
                {
                    image.Save(ms, ImageFormat.Jpeg);
                    bytes = ms.ToArray();
                }

                g.Dispose();
                image.Dispose();
                stream.Close();
                stream.Dispose();

                if (!string.IsNullOrEmpty(output))
                {
                    Storage.SaveFile(output, bytes);
                }

                return bytes;
            }
            catch (Exception x)
            {
                FileLog("Utils.WatermarkImage: " + x.Message + Environment.NewLine + x.InnerException.ToString());
                return bytes;
            }
        }

        private static void InsertLogo(Graphics g, string watermark, int position, int width, int height)
        {
            try
            {
                using (Image logo = Image.FromFile(watermark))
                {
                    Bitmap TransparentLogo = new Bitmap(logo.Width, logo.Height);
                    Graphics TGraphics = Graphics.FromImage(TransparentLogo);
                    ColorMatrix ColorMatrix = new ColorMatrix();
                    ColorMatrix.Matrix33 = 0.50F;
                    ImageAttributes ImgAttributes = new ImageAttributes();
                    ImgAttributes.SetColorMatrix(ColorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                    TGraphics.DrawImage(logo, new Rectangle(0, 0, TransparentLogo.Width, TransparentLogo.Height), 0, 0, TransparentLogo.Width, TransparentLogo.Height, GraphicsUnit.Pixel, ImgAttributes);
                    TGraphics.Dispose();

                    switch (position)
                    {
                        case (int)WatermarkPosition.TopLeft:
                            g.DrawImage(TransparentLogo, Utils.WatermarkMargin, Utils.WatermarkMargin);
                            break;
                        case (int)WatermarkPosition.TopRight:
                            g.DrawImage(TransparentLogo, width - (TransparentLogo.Width + Utils.WatermarkMargin), Utils.WatermarkMargin);
                            break;
                        case (int)WatermarkPosition.BottomLeft:
                            g.DrawImage(TransparentLogo, Utils.WatermarkMargin, height - (TransparentLogo.Height + Utils.WatermarkMargin));
                            break;
                        case (int)WatermarkPosition.BottomRight:
                            g.DrawImage(TransparentLogo, width - (TransparentLogo.Width + Utils.WatermarkMargin), height - (TransparentLogo.Height + Utils.WatermarkMargin));
                            break;
                    }
                }
            }
            catch (Exception x) { FileLog("Utils.AddLogo: " + x.Message + Environment.NewLine + x.ToString()); }
        }

        private static void InsertLogo(int timelapseId, Graphics g, string watermark, int position, int width, int height)
        {
            try
            {
                string file = "";
                if (!string.IsNullOrEmpty(watermark))
                    file = Utils.DoDownload(watermark, timelapseId + ".png");   // creates .png file on root of bll.dll

                ////// ideally should not download it, instead get from disk location
                ////// but that was erroring 'out of memory' due to corrupt image format
                //string nPath = Path.Combine(Settings.TempLogos, Path.GetFileName(watermark));
                //Storage.DownloadFile(watermark, nPath);
                //string path = WebUtility.UrlDecode(watermark);
                //path = path.Replace(Utils.SiteServer, Utils.WatermarkPrefix).Replace(@"/", @"\\");
                //if (!File.Exists(path))
                //{
                //    FileLog("Utils.AddLogo: File Not Found: " + nPath);
                //    return;
                //}

                using (Image logo = Image.FromFile(file))
                {
                    Bitmap TransparentLogo = new Bitmap(logo.Width, logo.Height);
                    Graphics TGraphics = Graphics.FromImage(TransparentLogo);
                    ColorMatrix ColorMatrix = new ColorMatrix();
                    ColorMatrix.Matrix33 = 0.50F;
                    ImageAttributes ImgAttributes = new ImageAttributes();
                    ImgAttributes.SetColorMatrix(ColorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                    TGraphics.DrawImage(logo, new Rectangle(0, 0, TransparentLogo.Width, TransparentLogo.Height), 0, 0, TransparentLogo.Width, TransparentLogo.Height, GraphicsUnit.Pixel, ImgAttributes);
                    TGraphics.Dispose();

                    switch (position)
                    {
                        case (int)WatermarkPosition.TopLeft:
                            g.DrawImage(TransparentLogo, Utils.WatermarkMargin, Utils.WatermarkMargin);
                            break;
                        case (int)WatermarkPosition.TopRight:
                            g.DrawImage(TransparentLogo, width - (TransparentLogo.Width + Utils.WatermarkMargin), Utils.WatermarkMargin);
                            break;
                        case (int)WatermarkPosition.BottomLeft:
                            g.DrawImage(TransparentLogo, Utils.WatermarkMargin, height - (TransparentLogo.Height + Utils.WatermarkMargin));
                            break;
                        case (int)WatermarkPosition.BottomRight:
                            g.DrawImage(TransparentLogo, width - (TransparentLogo.Width + Utils.WatermarkMargin), height - (TransparentLogo.Height + Utils.WatermarkMargin));
                            break;
                    }
                    FileLog("Logo added: " + file);
                }
            }
            catch (Exception x) { FileLog("Utils.AddLogo: " + x.Message + Environment.NewLine + x.ToString()); }
        }

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

        public static void TimelapseLog(int timelapseId, string message)
        {
            try
            {
                System.IO.Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
                string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");

                if (!Directory.Exists(logPath))
                    Directory.CreateDirectory(logPath);

                string logFolder = Path.Combine(logPath, timelapseId.ToString());

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

        public static void TimelapseLog(Timelapse timelapse, string message)
        {
            TimelapseLog(timelapse.ID, message);
        }

        public static void TimelapseLog(Timelapse timelapse, Exception x)
        {
            TimelapseLog(timelapse, x.ToString());
        }

        public static void TimelapseLog(Timelapse timelapse, string message, Exception x)
        {
            TimelapseLog(timelapse, message + Environment.NewLine + x.ToString());
        }

        public static string DoDownload(string url, string path)
        {
            try
            {
                byte[] data;
                using (WebClient client = new WebClient())
                {
                    data = client.DownloadData(url);
                    using (MemoryStream mem = new MemoryStream(data)) 
                    {
                        var yourImage = Image.FromStream(mem);
                        yourImage.Save(path, ImageFormat.Png);
                    }
                }
                return path;
            }
            catch {
                return "";
            }
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

        public static int TimelapseRunning(int timelapseId)
        {
            int id = 0;
            try
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
                        if (int.TryParse(_id, out tid) && tid == timelapseId)
                        {
                            if (process.Responding)
                            {
                                id = process.Id;
                                break;
                            }
                            else
                            {
                                id = process.Id * -1;
                                break;
                            }
                        }
                    }
                }
                return id;
            }
            catch (Exception x)
            {
                return id;
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

        public static bool KillProcess(int pid, int tid)
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

                //// remove this timelapse from dictionary untill its been added in next run
                //// so that the dictioanary always contains list of timelapses with currently running processes
                if (tid > 0)
                {
                    Utils.KillProcess("ffmpeg_" + tid);
                }

                return true;
            }
            catch (Exception x)
            {
                Utils.FileLog("KillProcess Error: " + x.Message);
                return false;
            }
        }

        public static void KillProcess(string processName)
        {
            try
            {
                Process[] processlist = Process.GetProcesses();
                foreach (Process process in processlist)
                {
                    if (process.ProcessName.ToLower().Equals(processName))
                    {
                        ProcessStartInfo start = new ProcessStartInfo();
                        start.FileName = "taskkill.exe";
                        start.Arguments = "/pid " + process.Id + " /F";
                        start.UseShellExecute = false;

                        Process p = new Process();
                        start.CreateNoWindow = true;
                        p.StartInfo = start;
                        p.Start();
                    }
                }
            }
            catch (Exception x)
            {
                Utils.FileLog("KillFfmpeg Error: " + x.Message);
            }
        }
    }

    public class SnapshotData
    {
        public byte[] Data { get; set; }
        public string ContentType { get; set; }
    }
}
