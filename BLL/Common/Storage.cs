using System;
using System.IO;
using System.Net;

namespace BLL.Common
{
    public class Storage
    {
        public static void CreateDirecotory(string ftpServer, string ftpPath, NetworkCredential credentials)
        {
            string lastdir = "";
            var dirNames = ftpPath.Substring(0, ftpPath.LastIndexOf("/", StringComparison.Ordinal));
            string[] dirs = dirNames.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var dir in dirs)
            {
                string url = ftpServer + lastdir + dir;
                WebRequest request = WebRequest.Create(url);
                request.Method = WebRequestMethods.Ftp.MakeDirectory;
                request.Credentials = credentials;
                using (var resp = (FtpWebResponse)request.GetResponse())
                {
                    resp.StatusCode.ToString();
                }
                lastdir += dir + "/";
            }
        }

        public static void UploadFile(string ftpPathAndFileName, string localFileName, NetworkCredential credentials)
        {
            using (var client = new WebClient())
            {
                client.Credentials = credentials;
                client.UploadFile(ftpPathAndFileName, "STOR", localFileName);
                client.Dispose();
            }
        }

        public static bool DownloadFile(string url, string localFileName)
        {
            var wc = new WebClient();
            try
            {
                wc.DownloadFile(url, localFileName);
                return true;
            }
            catch (Exception x)
            {
                return false;
            }
        }

        public static bool SaveFile(string fileName, byte[] data)
        {
            if (data == null)
                return false;
            try
            {
                string path = fileName.Substring(0, fileName.LastIndexOf(@"\"));

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                FileStream stream = new FileStream(fileName, FileMode.Create);
                stream.Write(data, 0, data.Length);
                stream.Close();
                stream.Dispose();
                return true;
            }
            catch (Exception x)
            {
                return false;
            }
        }
    }
}
