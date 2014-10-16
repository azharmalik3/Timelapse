using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using BLL.Common;
using BLL.Entities;
using BLL.Dao;
using RestSharp;

namespace BLL.Dao
{
    class TimelapseApiProxy
    {
        protected string APIUrl = Settings.TimelapseAPIUrl;

        public static List<Timelapse> GetTimelapses()
        {
            var client = new RestClient(Settings.TimelapseAPIUrl);
            var request = new RestRequest("v1/timelapses", Method.GET);
            var timelapsedata = client.Execute<List<Timelapse>>(request);
            List<Timelapse> timelapses = timelapsedata.Data;

            if (timelapsedata == null || timelapsedata.Data == null)
                return new List<Timelapse>();

            return timelapses;
        }

        public static Timelapse GetTimelapse(int timelapseId)
        {
            var client = new RestClient(Settings.TimelapseAPIUrl);
            var request = new RestRequest("v1/timelapses/" + timelapseId, Method.GET);
            var timelapsedata = client.Execute<Timelapse>(request);
            Timelapse timelapse = timelapsedata.Data;

            if (timelapsedata == null || timelapsedata.Data == null)
                return new Timelapse();

            return timelapse;
        }

        public static Timelapse GetTimelapse(string timelapseCode, int userId)
        {
            var client = new RestClient(Settings.TimelapseAPIUrl);
            var request = new RestRequest("v1/timelapses/" + timelapseCode + "/users/" + userId, Method.GET);
            var timelapsedata = client.Execute<Timelapse>(request);
            Timelapse timelapse = timelapsedata.Data;

            if (timelapsedata == null || timelapsedata.Data == null)
                return new Timelapse();

            return timelapse;
        }

        public static Timelapse UpdateStatus(string code, int userId, TimelapseStatus status)
        {
            var client = new RestClient(Settings.TimelapseAPIUrl);
            var request = new RestRequest("v1/timelapses/" + code + "/status/" + (int)status + "/users/" + userId, Method.POST);
            var timelapsedata = client.Execute<Timelapse>(request);
            Timelapse timelapse = timelapsedata.Data;

            if (timelapsedata == null || timelapsedata.Data == null)
                return new Timelapse();

            return timelapse;
        }

        public static Timelapse UpdateSnapsCount(string code, int userId, int count)
        {
            var client = new RestClient(Settings.TimelapseAPIUrl);
            var request = new RestRequest("v1/timelapses/" + code + "/snaps/" + count + "/users/" + userId, Method.POST);
            var timelapsedata = client.Execute<Timelapse>(request);
            Timelapse timelapse = timelapsedata.Data;

            if (timelapsedata == null || timelapsedata.Data == null)
                return new Timelapse();

            return timelapse;
        }

        public static int GetSnapsCount(string code, int userId)
        {
            var client = new RestClient(Settings.TimelapseAPIUrl);
            var request = new RestRequest("v1/timelapses/" + code + "/users/" + userId + "/snapscount", Method.GET);
            var msgdata = client.Execute<ResponseMessage>(request);
            ResponseMessage msg = msgdata.Data;

            if (msgdata == null || msgdata.Data == null)
                return 0;

            return int.Parse(msg.message);
        }
    }
}
