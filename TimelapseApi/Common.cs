using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Xml;
using System.IO;
using System.Xml.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using Microsoft.Web.Services3;
using Microsoft.Web.Services3.Security.Tokens;
using Newtonsoft.Json;
using TimelapseApi.Models;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Data.SqlClient;
using BLL.Dao;
using BLL.Entities;
using BLL.Common;

namespace TimelapseApi
{
    public class Common
    {
        public const string CAMBA_AUTH_KEY = "X-CAMBA-AUTH";
        public const string CAMBA_TEST_IMAGES = "http://testimage.camba.tv/";
        public const string CAMBA_API_URL = "http://webapi.camba.tv/";
        public const string API_TEST_IMAGES = @"C:/Camba/timelapseapi/testimage/";
        public const string TIMELAPSE_TEMP_IMAGE = @"timelapse.jpg";
        public const string UNAUTHORIZED = "Authorization has been denied for this request.";
        public const string USER_ID_KEY = "USER_ID";
        public const string USER_ROLE_KEY = "USER_ROLE";
        public const string TRANSFER_PROTOCOL = "http://";
        public const string DEFAULT_TIMEZONE = "GMT Standard Time";

        public static class Utility
        {
            public static string GetTimezone(string tzId)
            {
                string timeZone = "";
                try
                {
                    //// for using latest online xml
                    //XElement xelement = XElement.Load("http://unicode.org/repos/cldr/trunk/common/supplemental/windowsZones.xml");
                    XElement xelement = XElement.Load(HttpContext.Current.Server.MapPath("~/bin/TimeZones.xml"));
                    IEnumerable<XElement> zones = from z in xelement.Descendants("mapZone") 
                                                    where (string)z.Attribute("type") == tzId
                                                    select z;
                    timeZone = zones.FirstOrDefault().Attribute("other").Value;
                }
                catch (Exception x) {
                    //throw x;
                }
                return timeZone;
            }

            public static string GetTimelapseResourceUrl(Timelapse timelapse)
            {
                return Common.TRANSFER_PROTOCOL + timelapse.ServerIP + "/" + 
                    Settings.BucketName + "/" + 
                    Utils.RemoveSymbols(timelapse.CameraId) + "/" + 
                    timelapse.ID + "/";
            }

            public static JObject GetJsonResult(string message)
            {
                JObject jobj = new JObject();
                jobj.Add("message", message);
                return jobj;
            }

            public static JObject GetJsonResult(string message, bool success)
            {
                JObject jobj = new JObject();
                jobj.Add("message", message);
                jobj.Add("code", (success? "SUCCESS" : "ERROR"));
                return jobj;
            }

            public static HttpResponseException GetResponseException(string message, HttpStatusCode status)
            {
                var content = new StringContent(Common.Utility.GetJsonResult(message).ToString());
                var msg = new HttpResponseMessage(status) { Content = content };
                return new HttpResponseException(msg);
            }

            public static HttpResponseMessage GetResponseMessage(string message, HttpStatusCode status)
            {
                var content = new StringContent(Common.Utility.GetJsonResult(message).ToString());
                var msg = new HttpResponseMessage(status) { Content = content };
                return msg;
            }
        }

        public class PasswordDigestBehavior : IEndpointBehavior
        {
            public string Username { get; set; }
            public string Password { get; set; }

            public PasswordDigestBehavior(string username, string password)
            {
                this.Username = username;
                this.Password = password;
            }

            #region IEndpointBehavior Members

            public void AddBindingParameters(ServiceEndpoint endpoint, System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
            {
                //  throw new NotImplementedException();
            }

            public void ApplyClientBehavior(ServiceEndpoint endpoint, System.ServiceModel.Dispatcher.ClientRuntime clientRuntime)
            {
                clientRuntime.MessageInspectors.Add(new PasswordDigestMessageInspector(this.Username, this.Password));
            }

            public void ApplyDispatchBehavior(ServiceEndpoint endpoint, System.ServiceModel.Dispatcher.EndpointDispatcher endpointDispatcher)
            {
                //     throw new NotImplementedException();
            }

            public void Validate(ServiceEndpoint endpoint)
            {
                //  throw new NotImplementedException();
            }

            #endregion
        }
        public class PasswordDigestMessageInspector : IClientMessageInspector
        {
            public string Username { get; set; }
            public string Password { get; set; }

            public static string Xmlstring { get; private set; }

            public PasswordDigestMessageInspector(string username, string password)
            {
                this.Username = username;
                this.Password = password;
            }

            public void AfterReceiveReply(ref Message reply, object correlationState)
            {
                Xmlstring = reply.ToString();
            }

            public string AfterReceiveResponse(ref Message request, IClientChannel channel, InstanceContext instanceContext)
            {
                return "";
            }

            public object BeforeSendRequest(ref Message request, IClientChannel channel)
            {
                // Use the WSE 3.0 security token class
                UsernameToken token = new UsernameToken(this.Username, this.Password, PasswordOption.SendHashed);

                // Serialize the token to XML
                XmlElement securityToken = token.GetXml(new XmlDocument());

                //
                MessageHeader securityHeader = MessageHeader.CreateHeader("Security", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd", securityToken, false);
                request.Headers.Add(securityHeader);

                // complete
                return Convert.DBNull;
            }
        }
    }
}