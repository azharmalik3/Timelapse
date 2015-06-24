using System;
using System.Dynamic;
using System.Globalization;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Threading;
using System.Linq;
using System.IO;
using System.Net;
using System.Text;
using System.Net.Http;
using System.Web.Http;
using System.Web.Security;
using System.Web.Http.Cors;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using EvercamV2;
using TimelapseApi.Models;
using BLL.Dao;
using BLL.Entities;
using BLL.Common;

namespace TimelapseApi.Controllers
{
    /// <summary>
    /// This API allows an Authorized user to play with their camera's timelapses
    /// </summary>
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class TimelapsesController : ApiController
    {
        /// <summary>
        /// Get list of public timelapses
        /// </summary>
        /// <returns>See sample response data</returns>
        [Route("v1/timelapses/public")]
        [HttpGet]
        public IQueryable<TimelapseModel> GetPublic()
        {
            return TimelapseModel.Convert(TimelapseDao.GetList(TimelapsePrivacy.Public, null)).AsQueryable();
        }

        /// <summary>
        /// Get list of timelapses with Recording mode enabled
        /// </summary>
        /// <returns>See sample response data</returns>
        [Route("v1/timelapses")]
        [HttpGet]
        public IQueryable<TimelapseModel> GetActive()
        {
            return TimelapseModel.Convert(TimelapseDao.GetList(null, null)).AsQueryable();
        }

        /// <summary>
        /// Get user's timelapse details
        /// </summary>
        /// <param name="id">Timelapse Id</param>
        /// <returns>See sample response data</returns>
        [Route("v1/timelapses/{id:int}")]
        [HttpGet]
        public TimelapseModel Get(int id)
        {
            return TimelapseModel.Convert(TimelapseDao.Get(id));
        }

        /// <summary>
        /// Get list of user's timelapses
        /// </summary>
        /// <param name="id">Timelapse Owner's Evercam Id</param>
        /// <returns>See sample response data</returns>
        [Route("v1/timelapses/users/{id}")]
        [HttpGet]
        public IQueryable<TimelapseModel> GetByUser(string id)
        {
            return TimelapseModel.Convert(TimelapseDao.GetListByEvercamId(id, null, null)).AsQueryable();
        }

        /// <summary>
        /// Get user's timelapse's snapshots count
        /// </summary>
        /// <param name="code">Timelapse Unique Code</param>
        /// <param name="id">Timelapse Owner's Id</param>
        /// <returns>See sample response data</returns>
        [Route("v1/timelapses/{code}/users/{id}/snapscount")]
        [HttpGet]
        public ResponseMessage GetSnapsCount(string code, string id)
        {
            if (TimelapseDao.Get(code).UserId != id)
                throw new HttpResponseException(HttpStatusCode.Forbidden);
            return new ResponseMessage() { message = TimelapseDao.GetSnapsCount(code).ToString() };
        }

        /// <summary>
        /// Get user's timelapse details against given 'code'
        /// </summary>
        /// <param name="code">Timelapse Unique Code</param>
        /// <param name="id">Timelapse Owner's Evercam Id</param>
        /// <returns>See sample response data</returns>
        [Route("v1/timelapses/{code}/users/{id}")]
        [HttpGet]
        public TimelapseModel GetByCode(string code, string id)
        {
            Timelapse t = TimelapseDao.Get(code);
            if (t.UserId != id)
                throw new HttpResponseException(HttpStatusCode.Forbidden);
            return TimelapseModel.Convert(TimelapseDao.Get(code));
        }

        /// <summary>
        /// Get given timelapse's placeholder image after retrieving it from Evercam
        /// and embedding logo in it
        /// </summary>
        /// <param name="code">Timelapse Unique Code</param>
        /// <returns>See sample response data</returns>
        [Route("v1/timelapses/{code}/placeholder")]
        [HttpGet]
        public DataModel GetPlaceholder(string code)
        {
            Evercam.SANDBOX = Settings.EvercamSandboxMode;
            Evercam evercam = new Evercam(Settings.EvercamClientID, Settings.EvercamClientSecret, Settings.EvercamClientUri);
            Timelapse t = TimelapseDao.Get(code);
            if (!string.IsNullOrEmpty(t.OauthToken))
                evercam = new Evercam(t.OauthToken);

            Camera c = evercam.GetCamera(t.CameraId);
            
            string cleanCameraId = BLL.Common.Utils.RemoveSymbols(c.ID);
            string filePath = Path.Combine(Settings.BucketUrl, Settings.BucketName, cleanCameraId, t.ID.ToString());
            if (!Directory.Exists(filePath))
                Directory.CreateDirectory(filePath);

            try
            {
                byte[] image = evercam.CreateSnapshot(c.ID, Settings.EvercamClientName, true).ToBytes();
                return new DataModel
                {
                    data = "data:image/jpeg;base64," + Convert.ToBase64String(
                        Utils.WatermarkImage(
                            t.ID,
                            image,
                            filePath + "\\" + t.Code + ".jpg",
                            t.WatermarkImage,
                            t.WatermarkPosition))
                };
            }
            catch (Exception x) { return new DataModel(); }
        }

        /// <summary>
        /// Create new timelapse
        /// </summary>
        /// <param name="data">See sample request data</param>
        /// <param name="id">Timelapse Owner's Evercam Id</param>
        /// <returns>Returns newly created timelapse details in case of success or HTTP 400 error in case of failure</returns>
        [Route("v1/timelapses/users/{id}")]
        [HttpPost]
        public TimelapseModel Post([FromBody]TimelapseInfoModel data, string id)
        {
            Timelapse t = TimelapseModel.Convert(data, id);
            t.ServerIP = Request.RequestUri.Host;
            t.Status = (int)TimelapseStatus.Processing;
            t.Code = Utils.GeneratePassCode(10);

            int tid = 0;
            if ((tid = TimelapseDao.Insert(t)) == 0)
                throw new HttpResponseException(HttpStatusCode.BadRequest);

            string base64 = Settings.TimelapseAPIUrl + "testimage/" + Settings.TempTimelapse;
            if (!string.IsNullOrEmpty(data.watermark_file))
            {
                //// create placeholder image at the same time
                Evercam.SANDBOX = Settings.EvercamSandboxMode;
                Evercam evercam = new Evercam(Settings.EvercamClientID, Settings.EvercamClientSecret, Settings.EvercamClientUri);
                if (!string.IsNullOrEmpty(t.OauthToken))
                    evercam = new Evercam(t.OauthToken);

                Camera c = evercam.GetCamera(t.CameraId);
                string cleanCameraId = BLL.Common.Utils.RemoveSymbols(c.ID);
                string filePath = Path.Combine(Settings.BucketUrl, Settings.BucketName, cleanCameraId, tid.ToString());
                if (!Directory.Exists(filePath))
                    Directory.CreateDirectory(filePath);

                try
                {
                    byte[] image = evercam.CreateSnapshot(c.ID, Settings.EvercamClientName, true).ToBytes();
                    base64 = "data:image/jpeg;base64," + Convert.ToBase64String(
                        Utils.WatermarkImage(
                            t.ID,
                            image,
                            filePath + "\\" + t.Code + ".jpg",
                            t.WatermarkImage,
                            t.WatermarkPosition));
                }
                catch (Exception x) { BLL.Common.Utils.FileLog(t.ID + " - " + x.ToString()); }
            }

            return TimelapseModel.Convert(TimelapseDao.Get(tid), base64);
        }

        /// <summary>
        /// Update timelapse details
        /// </summary>
        /// <param name="code">Timelapse unique code (e.g. a1s2d3f4g5)</param>
        /// <param name="data">See sample request data</param>
        /// <param name="id">Timelapse Owner's Evercam Id</param>
        /// <returns>Returns updated timelapse details in case of success or HTTP error 400/BadRequest</returns>
        [Route("v1/timelapses/{code}/users/{id}")]
        [HttpPut]
        public TimelapseModel Update(string code, [FromBody]TimelapseInfoModel data, string id)
        {
            Timelapse t = TimelapseDao.Get(code);
            if (t.UserId != id)
                throw new HttpResponseException(HttpStatusCode.Forbidden);

            t = TimelapseModel.Convert(data, t.UserId, t.ID, code, t.Status);

            if (!TimelapseDao.Update(t))
                throw new HttpResponseException(HttpStatusCode.BadRequest);

            if (t.FromDT.Hour >= t.ToDT.Hour)
            {
                // ToDT is in next day
                DateTime from = new DateTime();
                DateTime to = new DateTime();
                if (t.DateAlways)
                {
                    from = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day,
                        t.FromDT.Hour, t.FromDT.Minute, t.FromDT.Second);
                    to = new DateTime(from.Year, from.Month, from.Day,
                        t.ToDT.Hour, t.ToDT.Minute, t.ToDT.Second).AddDays(1);
                }
                else
                {
                    from = new DateTime(t.FromDT.Year, t.FromDT.Month, t.FromDT.Day,
                        t.FromDT.Hour, t.FromDT.Minute, t.FromDT.Second);
                    to = new DateTime(t.ToDT.Year, t.ToDT.Month, t.ToDT.Day,
                        t.ToDT.Hour, t.ToDT.Minute, t.ToDT.Second);
                }

                if (!t.DateAlways && t.ToDT.Date <= t.FromDT.Date)
                    TimelapseDao.UpdateStatus(t.Code, TimelapseStatus.Stopped, "Out of schedule", t.TimeZone);
                else if ((DateTime.UtcNow < from || DateTime.UtcNow > to))
                {
                    if (t.DateAlways)
                        TimelapseDao.UpdateStatus(t.Code, TimelapseStatus.Scheduled, "Recording on schedule", t.TimeZone);
                    else
                        TimelapseDao.UpdateStatus(t.Code, TimelapseStatus.Stopped, "Out of schedule", t.TimeZone);
                }
            }
            else if (t.Status > (int)TimelapseStatus.Processing) {
                if (t.DateAlways && t.TimeAlways)
                    TimelapseDao.UpdateStatus(t.Code, TimelapseStatus.Processing, "Processing...", t.TimeZone);
                else if (t.DateAlways && !t.TimeAlways)
                {
                    if (DateTime.UtcNow.TimeOfDay >= t.FromDT.TimeOfDay && DateTime.UtcNow.TimeOfDay <= t.ToDT.TimeOfDay)
                        TimelapseDao.UpdateStatus(t.Code, TimelapseStatus.Processing, "Processing...", t.TimeZone);
                }
                else if (DateTime.UtcNow >= t.FromDT && DateTime.UtcNow <= t.ToDT)
                    TimelapseDao.UpdateStatus(t.Code, TimelapseStatus.Processing, "Processing...", t.TimeZone);
            }

            string base64 = Settings.TimelapseAPIUrl + "testimage/" + Settings.TempTimelapse;
            if (!string.IsNullOrEmpty(data.watermark_file))
            {
                //// create placeholder image at the same time
                Evercam.SANDBOX = Settings.EvercamSandboxMode;
                Evercam evercam = new Evercam(Settings.EvercamClientID, Settings.EvercamClientSecret, Settings.EvercamClientUri);
                if (!string.IsNullOrEmpty(t.OauthToken))
                    evercam = new Evercam(t.OauthToken);

                Camera c = evercam.GetCamera(t.CameraId);
                string cleanCameraId = BLL.Common.Utils.RemoveSymbols(c.ID);
                string filePath = Path.Combine(Settings.BucketUrl, Settings.BucketName, cleanCameraId, t.ID.ToString());
                if (!Directory.Exists(filePath))
                    Directory.CreateDirectory(filePath);

                try
                {
                    byte[] image = evercam.CreateSnapshot(c.ID, Settings.EvercamClientName, true).ToBytes();
                    base64 = "data:image/jpeg;base64," + Convert.ToBase64String(
                            Utils.WatermarkImage(
                                t.ID,
                                image,
                                filePath + "\\" + t.Code + ".jpg",
                                t.WatermarkImage,
                                t.WatermarkPosition));
                }
                catch (Exception x) { BLL.Common.Utils.FileLog(t.ID + " - " + x.ToString()); }
            }

            return TimelapseModel.Convert(TimelapseDao.Get(code), base64);
        }

        /// <summary>
        /// Update timelapse status only
        /// </summary>
        /// <param name="code">Timelapse Code</param>
        /// <param name="status">Timelapse Status (0: New, 1: Processing, 2: Failed, 3: Cancelled, 4: Completed)</param>
        /// <param name="id">Timelapse Owner's Evercam Id</param>
        /// <returns>Returns updated timelapse details in case of success or HTTP error 400/BadRequest</returns>
        [Route("v1/timelapses/{code}/status/{status:int}/users/{id}")]
        [HttpPost]
        public TimelapseModel UpdateStatus(string code, int status, string id)
        {
            Timelapse t = TimelapseDao.Get(code);
            if (t.UserId != id)
                throw new HttpResponseException(HttpStatusCode.Forbidden);
            if (!TimelapseDao.UpdateStatus(code, (TimelapseStatus)status, "Status set to " + ((TimelapseStatus)status).ToString(), t.TimeZone))
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            return TimelapseModel.Convert(TimelapseDao.Get(code));
        }

        /// <summary>
        /// Update timelapse snapshots count only
        /// </summary>
        /// <param name="code">Timelapse Code</param>
        /// <param name="count">Snapshots Count</param>
        /// <param name="id">Timelapse Owner's Evercam Id</param>
        /// <returns>Returns updated timelapse details in case of success or HTTP error 400/BadRequest</returns>
        [Route("v1/timelapses/{code}/snaps/{count:int}/users/{id}")]
        [HttpPost]
        public TimelapseModel UpdateSnapsCount(string code, int count, string id)
        {
            if (TimelapseDao.Get(code).UserId != id)
                throw new HttpResponseException(HttpStatusCode.Forbidden);
            if (TimelapseDao.UpdateSnapsCount(code, count) < count)
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            return TimelapseModel.Convert(TimelapseDao.Get(code));
        }

        /// <summary>
        /// Delete a timelapse against a given 'code'
        /// </summary>
        /// <param name="code"></param>
        /// <param name="id">Timelapse Owner's Evercam Id</param>
        /// <returns>Returns HTTP status 200/OK in case of success or HTTP error 400/BadRequest</returns>
        [Route("v1/timelapses/{code}/users/{id}")]
        [HttpDelete]
        public HttpResponseMessage Delete(string code, string id)
        {
            Timelapse timelapse = TimelapseDao.Get(code);
            if (timelapse.UserId != id)
                throw new HttpResponseException(HttpStatusCode.Forbidden);
            if (!TimelapseDao.Delete(code))
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            
            string message = "";
            string path = Path.Combine(
                    Settings.BucketUrl + Settings.BucketName,
                    BLL.Common.Utils.RemoveSymbols(timelapse.CameraId),
                    timelapse.ID.ToString());
            try
            {
                new DirectoryInfo(path).Delete(true);
            }
            catch (Exception x)
            {
                message = "Error: " + x.Message + " Deleting: " + path;
            }

            return Common.Utility.GetResponseMessage(message, HttpStatusCode.OK);
        }

        #region Utils

        /// <summary>
        /// Get Evercam token details from given token endpoint data
        /// </summary>
        /// <param name="data">See sample request data below</param>
        /// <returns>See sample response data below</returns>
        [Route("v1/tokeninfo")]
        [HttpPost]
        public TokenUserModel GetTokenUser([FromBody]TokenUrlModel data)
        {
            try
            {
                data.token_endpoint = data.token_endpoint.Replace("dashboard", "api");
                string result;
                WebRequest r = WebRequest.Create(data.token_endpoint);
                r.Method = "GET";
                using (var response = (HttpWebResponse)r.GetResponse())
                {
                    result = new StreamReader(response.GetResponseStream()).ReadToEnd();
                    response.Close();
                }
                TokenUserModel token = JsonConvert.DeserializeObject<TokenUserModel>(result);

                ////// UPDATES USER ACCESS TOKEN AGAINST ALL SNAPMAILS ////////
                TimelapseDao.UpdateUserToken(token.userid, token.access_token);
                ///////////////////////////////////////////////////////////////
                return token;
            }
            catch (Exception x) { throw new HttpResponseException(HttpStatusCode.InternalServerError); }
        }

        #endregion
    }
}