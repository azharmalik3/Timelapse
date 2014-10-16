using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BLL.Common;
using BLL.Entities;
using System.Web.Http;
using System.Net;
using Newtonsoft.Json;

namespace TimelapseApi.Models
{
    public class TimelapseModel
    {
        public int id { get; set; }
        /// <example>evercam-camera-id</example>
        public string camera_id { get; set; }
        /// <example>evercam-user-id</example>
        public string user_id { get; set; }
        /// <example>a1s2d3f4g5</example>
        public string code { get; set; }
        /// <example>05/10/2013 00:00:10</example>
        public string from_date { get; set; }
        /// <example>05/10/2013 00:00:10</example>
        public string to_date { get; set; }
        /// <example>my test timelapse</example>
        public string title { get; set; }
        /// <example>http://timelapse.camba.tv/1/a1s2d3f4g5.mp4</example>
        public string mp4_url { get; set; }
        /// <example>http://timelapse.camba.tv/1/a1s2d3f4g5.jpg</example>
        public string jpg_url { get; set; }
        /// <example>GMT</example>
        public string time_zone { get; set; }
        public long snaps_count { get; set; }
        public string file_size { get; set; }
        public string duration { get; set; }
        public string resolution { get; set; }
        /// <example>5</example>
        public int interval { get; set; }
        public bool enable_md { get; set; }
        /// <example>2</example>
        public int md_thrushold { get; set; }
        public bool exclude_dark { get; set; }
        /// <example>1</example>
        public int darkness_thrushold { get; set; }
        /// <example>0</example>
        public int status { get; set; }
        public string status_tag { get; set; }
        /// <example>0</example>
        public int privacy { get; set; }
        public int fps { get; set; }
        public bool is_date_always { get; set; }
        public bool is_time_always { get; set; }
        public bool is_recording { get; set; }
        /// <example>0</example>
        public int watermark_position { get; set; }
        public string watermark_file { get; set; }
        /// <example>05/10/2013 00:00:10</example>
        public string last_snap_date { get; set; }
        /// <example>05/10/2013 00:00:10</example>
        public string modified_date { get; set; }
        /// <example>05/10/2013 00:00:10</example>
        public string created_date { get; set; }

        public static Timelapse Convert(TimelapseInfoModel model, string evercamId)
        {
            Timelapse timelapse = new Timelapse();

            timelapse.UserId = evercamId;
            timelapse.CameraId = model.camera_eid;
            timelapse.OauthToken = model.access_token;
            timelapse.Title = model.title;
            timelapse.LastSnapDT = Utils.SQLMinDate;
            
            timelapse.Privacy = model.privacy;
            timelapse.DateAlways = model.is_date_always;
            timelapse.TimeAlways = model.is_time_always;

            DateTime from = new DateTime(Utils.SQLMinDate.Year, Utils.SQLMinDate.Month, Utils.SQLMinDate.Day, 0, 0, 0, 0);
            DateTime to = new DateTime(Utils.SQLMaxDate.Year, Utils.SQLMaxDate.Month, Utils.SQLMaxDate.Day, 23, 59, 59, 000);
            DateTime f = from;
            DateTime t = to;

            if (!model.is_date_always)
            {
                f = DateTime.Parse(model.from_date);
                t = DateTime.Parse(model.to_date);
                from = new DateTime(f.Year, f.Month, f.Day, 0, 0, 0, 0);
                to = new DateTime(t.Year, t.Month, t.Day, 23, 59, 59, 000);
            }
            if (!model.is_time_always)
            {
                f = DateTime.Parse(model.from_time);
                t = DateTime.Parse(model.to_time);
                from = new DateTime(from.Year, from.Month, from.Day, f.Hour, f.Minute, f.Second, f.Millisecond);
                to = new DateTime(to.Year, to.Month, to.Day, t.Hour, t.Minute, t.Second, t.Millisecond);
            }

            timelapse.TzId = model.time_zone;
            timelapse.TimeZone = Common.Utility.GetTimezone(model.time_zone);
            if (string.IsNullOrEmpty(timelapse.TimeZone))
                timelapse.TimeZone = Common.DEFAULT_TIMEZONE;
            
            timelapse.FromDT = Utils.ConvertToUtc(from, timelapse.TimeZone);
            timelapse.ToDT = Utils.ConvertToUtc(to, timelapse.TimeZone);
            timelapse.EnableMD = model.enable_md;
            timelapse.MDThreshold = model.md_thrushold;
            timelapse.ExcludeDark = model.exclude_dark;
            timelapse.DarkThreshold = model.darkness_thrushold;
            timelapse.SnapsInterval = model.interval;
            timelapse.IsRecording = model.is_recording;
            timelapse.FPS = model.fps;
            timelapse.WatermarkImage = model.watermark_file;
            timelapse.WatermarkPosition = model.watermark_position;

            return timelapse;
        }

        public static Timelapse Convert(TimelapseInfoModel model, string evercamId, int id, string code, int status)
        {
            Timelapse timelapse = new Timelapse();

            timelapse.ID = id;
            if (!string.IsNullOrEmpty(code))
                timelapse.Code = code;
            timelapse.Status = status;

            timelapse.UserId = evercamId;
            timelapse.CameraId = model.camera_eid;
            timelapse.OauthToken = model.access_token;
            timelapse.Title = model.title;
            timelapse.LastSnapDT = Utils.SQLMinDate;

            timelapse.Privacy = model.privacy;
            timelapse.DateAlways = model.is_date_always;
            timelapse.TimeAlways = model.is_time_always;

            DateTime from = new DateTime(Utils.SQLMinDate.Year, Utils.SQLMinDate.Month, Utils.SQLMinDate.Day, 0, 0, 0, 0);
            DateTime to = new DateTime(Utils.SQLMaxDate.Year, Utils.SQLMaxDate.Month, Utils.SQLMaxDate.Day, 23, 59, 59, 000);
            DateTime f = from;
            DateTime t = to;

            if (!model.is_date_always)
            {
                f = DateTime.Parse(model.from_date);
                t = DateTime.Parse(model.to_date);
                from = new DateTime(f.Year, f.Month, f.Day, 0, 0, 0, 0);
                to = new DateTime(t.Year, t.Month, t.Day, 23, 59, 59, 000);
            }
            if (!model.is_time_always)
            {
                f = DateTime.Parse(model.from_time);
                t = DateTime.Parse(model.to_time);
                from = new DateTime(from.Year, from.Month, from.Day, f.Hour, f.Minute, f.Second, f.Millisecond);
                to = new DateTime(to.Year, to.Month, to.Day, t.Hour, t.Minute, t.Second, t.Millisecond);
            }

            timelapse.TzId = model.time_zone;
            timelapse.TimeZone = Common.Utility.GetTimezone(model.time_zone);
            if (string.IsNullOrEmpty(timelapse.TimeZone))
                timelapse.TimeZone = Common.DEFAULT_TIMEZONE;

            timelapse.FromDT = Utils.ConvertToUtc(from, timelapse.TimeZone);
            timelapse.ToDT = Utils.ConvertToUtc(to, timelapse.TimeZone);
            timelapse.EnableMD = model.enable_md;
            timelapse.MDThreshold = model.md_thrushold;
            timelapse.ExcludeDark = model.exclude_dark;
            timelapse.DarkThreshold = model.darkness_thrushold;
            timelapse.SnapsInterval = model.interval;
            timelapse.IsRecording = model.is_recording;
            timelapse.FPS = model.fps;
            timelapse.WatermarkImage = model.watermark_file;
            timelapse.WatermarkPosition = model.watermark_position;

            return timelapse;
        }

        public static Timelapse Convert(TimelapseModel model, string userId)
        {
            Timelapse timelapse = new Timelapse();

            timelapse.ID = model.id;
            timelapse.UserId = userId;
            timelapse.CameraId = model.camera_id;
            timelapse.Code = model.code;
            timelapse.Title = model.title;
            timelapse.Status = model.status;
            timelapse.Privacy = model.privacy;
            timelapse.DateAlways = model.is_date_always;
            timelapse.TimeAlways = model.is_time_always;
            timelapse.FromDT = DateTime.Parse(model.from_date);
            timelapse.ToDT = DateTime.Parse(model.to_date);
            timelapse.EnableMD = model.enable_md;
            timelapse.MDThreshold = model.md_thrushold;
            timelapse.ExcludeDark = model.exclude_dark;
            timelapse.DarkThreshold = model.darkness_thrushold;
            timelapse.IsRecording = model.is_recording;
            timelapse.SnapsInterval = model.interval;
            timelapse.TimeZone = model.time_zone;
            timelapse.CreatedDT = DateTime.Parse(model.created_date);
            timelapse.FPS = model.fps;
            timelapse.WatermarkImage = model.watermark_file;
            timelapse.WatermarkPosition = model.watermark_position;

            return timelapse;
        }

        public static TimelapseModel Convert(Timelapse timelapse)
        {
            TimelapseModel model = new TimelapseModel();

            model.id = timelapse.ID;
            model.camera_id = timelapse.CameraId;
            model.user_id = timelapse.UserId;
            model.code = timelapse.Code;
            model.title = timelapse.Title;
            model.status_tag = timelapse.StatusTag;
            model.jpg_url = Common.Utility.GetTimelapseResourceUrl(timelapse) + timelapse.Code + ".jpg";
            model.mp4_url = Common.Utility.GetTimelapseResourceUrl(timelapse) + timelapse.Code + ".mp4";
            model.status = timelapse.Status;
            model.time_zone = timelapse.TimeZone;
            model.snaps_count = timelapse.SnapsCount;
            model.file_size = Utils.GetSpace(timelapse.FileSize);
            model.duration = timelapse.Duration;
            model.resolution = timelapse.Resolution;
            model.privacy = timelapse.Privacy;
            model.from_date = Utils.ConvertFromUtc(timelapse.FromDT, timelapse.TimeZone).ToString();
            model.to_date = Utils.ConvertFromUtc(timelapse.ToDT, timelapse.TimeZone).ToString();
            model.is_date_always = timelapse.DateAlways;
            model.is_time_always = timelapse.TimeAlways;
            model.enable_md = timelapse.EnableMD;
            model.md_thrushold = timelapse.MDThreshold;
            model.exclude_dark = timelapse.ExcludeDark;
            model.darkness_thrushold = timelapse.DarkThreshold;
            model.is_recording = timelapse.IsRecording;
            model.interval = timelapse.SnapsInterval;
            model.last_snap_date = (timelapse.LastSnapDT == Utils.SQLMinDate ? "" : Utils.ConvertFromUtc(timelapse.LastSnapDT, timelapse.TimeZone).ToString("f"));
            model.modified_date = timelapse.ModifiedDT.ToString("f");
            model.created_date = timelapse.CreatedDT.ToString("f");
            model.fps = timelapse.FPS;
            model.watermark_file = timelapse.WatermarkImage;
            model.watermark_position = (int)timelapse.WatermarkPosition;

            return model;
        }

        public static List<TimelapseModel> Convert(List<Timelapse> timelapses)
        {
            List<TimelapseModel> list = new List<TimelapseModel>();

            foreach (Timelapse timelapse in timelapses)
            {
                TimelapseModel model = new TimelapseModel();

                model.id = timelapse.ID;
                model.camera_id = timelapse.CameraId;
                model.user_id = timelapse.UserId;
                model.code = timelapse.Code;
                model.title = timelapse.Title;
                model.status_tag = timelapse.StatusTag;
                model.jpg_url = Common.Utility.GetTimelapseResourceUrl(timelapse) + timelapse.Code + ".jpg";
                model.mp4_url = Common.Utility.GetTimelapseResourceUrl(timelapse) + timelapse.Code + ".mp4";
                model.status = timelapse.Status;
                model.time_zone = timelapse.TimeZone;
                model.snaps_count = timelapse.SnapsCount;
                model.file_size = Utils.GetSpace(timelapse.FileSize);
                model.duration = timelapse.Duration;
                model.resolution = timelapse.Resolution;
                model.privacy = timelapse.Privacy;
                model.from_date = Utils.ConvertFromUtc(timelapse.FromDT, timelapse.TimeZone).ToString();
                model.to_date = Utils.ConvertFromUtc(timelapse.ToDT, timelapse.TimeZone).ToString();
                model.is_date_always = timelapse.DateAlways;
                model.is_time_always = timelapse.TimeAlways;
                model.enable_md = timelapse.EnableMD;
                model.md_thrushold = timelapse.MDThreshold;
                model.exclude_dark = timelapse.ExcludeDark;
                model.darkness_thrushold = timelapse.DarkThreshold;
                model.is_recording = timelapse.IsRecording;
                model.interval = timelapse.SnapsInterval;
                model.last_snap_date = (timelapse.LastSnapDT == Utils.SQLMinDate ? "" : Utils.ConvertFromUtc(timelapse.LastSnapDT, timelapse.TimeZone).ToString("f"));
                model.modified_date = timelapse.ModifiedDT.ToString("f");
                model.created_date = timelapse.CreatedDT.ToString("f");
                model.fps = timelapse.FPS;
                model.watermark_file = timelapse.WatermarkImage;
                model.watermark_position = (int)timelapse.WatermarkPosition;

                list.Add(model);
            }
            return list;
        }

        public static TimelapseModel Convert(Timelapse timelapse, string tempImage)
        {
            TimelapseModel model = new TimelapseModel();

            model.id = timelapse.ID;
            model.camera_id = timelapse.CameraId;
            model.user_id = timelapse.UserId;
            model.code = timelapse.Code;
            model.title = timelapse.Title;
            model.jpg_url = tempImage;
            model.status_tag = timelapse.StatusTag;
            model.mp4_url = Common.Utility.GetTimelapseResourceUrl(timelapse) + timelapse.Code + ".mp4";
            model.status = timelapse.Status;
            model.time_zone = timelapse.TimeZone;
            model.snaps_count = timelapse.SnapsCount;
            model.file_size = Utils.GetSpace(timelapse.FileSize);
            model.duration = timelapse.Duration;
            model.resolution = timelapse.Resolution;
            model.privacy = timelapse.Privacy;
            model.from_date = Utils.ConvertFromUtc(timelapse.FromDT, timelapse.TimeZone).ToString();
            model.to_date = Utils.ConvertFromUtc(timelapse.ToDT, timelapse.TimeZone).ToString();
            model.is_date_always = timelapse.DateAlways;
            model.is_time_always = timelapse.TimeAlways;
            model.enable_md = timelapse.EnableMD;
            model.md_thrushold = timelapse.MDThreshold;
            model.exclude_dark = timelapse.ExcludeDark;
            model.darkness_thrushold = timelapse.DarkThreshold;
            model.is_recording = timelapse.IsRecording;
            model.interval = timelapse.SnapsInterval;
            model.last_snap_date = (timelapse.LastSnapDT == Utils.SQLMinDate ? "" : Utils.ConvertFromUtc(timelapse.LastSnapDT, timelapse.TimeZone).ToString("f"));
            model.modified_date = timelapse.ModifiedDT.ToString("f");
            model.created_date = timelapse.CreatedDT.ToString("f");
            model.fps = timelapse.FPS;
            model.watermark_file = timelapse.WatermarkImage;
            model.watermark_position = (int)timelapse.WatermarkPosition;

            return model;
        }
    }

    public class TimelapseInfoModel
    {
        /// <example>evercam-camera-id</example>
        public string camera_eid { get; set; }
        /// <example>evercam-access-token</example>
        public string access_token { get; set; }
        /// <example>00:00:10</example>
        public string from_time { get; set; }
        /// <example>00:00:10</example>
        public string to_time { get; set; }
        /// <example>05/10/2013</example>
        public string from_date { get; set; }
        /// <example>05/10/2013</example>
        public string to_date { get; set; }
        /// <example>my test timelapse</example>
        public string title { get; set; }
        /// <example>America/Los_Angeles</example>
        public string time_zone { get; set; }
        public bool enable_md { get; set; }
        /// <example>2</example>
        public int md_thrushold { get; set; }
        public bool exclude_dark { get; set; }
        /// <example>1</example>
        public int darkness_thrushold { get; set; }
        /// <example>0</example>
        public int privacy { get; set; }
        public bool is_recording { get; set; }
        public bool is_date_always { get; set; }
        public bool is_time_always { get; set; }
        /// <example>5</example>
        public int interval { get; set; }
        /// <example>2</example>
        public int fps { get; set; }
        /// <example>0</example>
        public int watermark_position { get; set; }
        [JsonProperty("watermark_file", NullValueHandling = NullValueHandling.Ignore)]
        public string watermark_file { get; set; }
    }
}