using System;
using System.Linq;

namespace BLL.Entities
{
    public class Timelapse
    {
        public int ID { get; set; }
        public string UserId { get; set; }
        public string CameraId { get; set; }
        public string OauthToken { get; set; }
        public string StatusTag { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public string ServerIP { get; set; }
        public string TzId { get; set; }
        public string TimeZone { get; set; }
        public DateTime ModifiedDT { get; set; }
        public DateTime FromDT { get; set; }
        public DateTime ToDT { get; set; }
        public DateTime LastSnapDT { get; set; }
        public DateTime CreatedDT { get; set; }
        public bool EnableMD { get; set; }
        public int MDThreshold { get; set; }
        public bool ExcludeDark { get; set; }
        public int DarkThreshold { get; set; }
        public int SnapsInterval { get; set; }
        public int Status { get; set; }
        public int Privacy { get; set; }
        public bool DateAlways { get; set; }
        public bool TimeAlways { get; set; }
        public bool IsRecording { get; set; }
        public bool IsDeleted { get; set; }
        public int FPS { get; set; }
        public int SnapsCount { get; set; }
        public long FileSize { get; set; }
        public string Duration { get; set; }
        public string Resolution { get; set; }
        public string WatermarkImage { get; set; }
        public int WatermarkPosition { get; set; }
    }
}
