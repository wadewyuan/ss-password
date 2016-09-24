using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;

namespace ss_password
{
    public class HttpHelper
    {
        /// <summary>  
        /// 创建GET方式的HTTP请求  
        /// </summary>  
        public static HttpWebResponse CreateGetHttpResponse(string url, int timeout, string userAgent, CookieCollection cookies)
        {
            HttpWebRequest request = null;
            if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                //对服务端证书进行有效性校验（非第三方权威机构颁发的证书，如自己生成的，不进行验证，这里返回true）
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                request = WebRequest.Create(url) as HttpWebRequest;
                request.ProtocolVersion = HttpVersion.Version10;    //http版本，默认是1.1,这里设置为1.0
            }
            else
            {
                request = WebRequest.Create(url) as HttpWebRequest;
            }
            request.Method = "GET";

            //设置代理UserAgent和超时
            //request.UserAgent = userAgent;
            //request.Timeout = timeout;
            if (cookies != null)
            {
                request.CookieContainer = new CookieContainer();
                request.CookieContainer.Add(cookies);
            }
            return request.GetResponse() as HttpWebResponse;
        }

        /// <summary>  
        /// 创建POST方式的HTTP请求  
        /// </summary>  
        public static HttpWebResponse CreatePostHttpResponse(string url, IDictionary<string, string> parameters, int timeout, string userAgent, CookieCollection cookies)
        {
            HttpWebRequest request = null;
            //如果是发送HTTPS请求  
            if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                //ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                request = WebRequest.Create(url) as HttpWebRequest;
                //request.ProtocolVersion = HttpVersion.Version10;
            }
            else
            {
                request = WebRequest.Create(url) as HttpWebRequest;
            }
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";

            //设置代理UserAgent和超时
            //request.UserAgent = userAgent;
            //request.Timeout = timeout; 

            if (cookies != null)
            {
                request.CookieContainer = new CookieContainer();
                request.CookieContainer.Add(cookies);
            }
            //发送POST数据  
            if (!(parameters == null || parameters.Count == 0))
            {
                StringBuilder buffer = new StringBuilder();
                int i = 0;
                foreach (string key in parameters.Keys)
                {
                    if (i > 0)
                    {
                        buffer.AppendFormat("&{0}={1}", key, parameters[key]);
                    }
                    else
                    {
                        buffer.AppendFormat("{0}={1}", key, parameters[key]);
                        i++;
                    }
                }
                byte[] data = Encoding.ASCII.GetBytes(buffer.ToString());
                using (Stream stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }
            }
            string[] values = request.Headers.GetValues("Content-Type");
            return request.GetResponse() as HttpWebResponse;
        }

        /// <summary>
        /// 获取请求的数据
        /// </summary>
        public static string GetResponseString(HttpWebResponse webresponse)
        {
            using (Stream s = webresponse.GetResponseStream())
            {
                StreamReader reader = new StreamReader(s, Encoding.UTF8);
                return reader.ReadToEnd();

            }
        }

        /// <summary>
        /// 验证证书
        /// </summary>
        private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            if (errors == SslPolicyErrors.None)
                return true;
            return false;
        }

    }

    class ServerInfo
    {
        private string host;
        private string port;
        private string method;
        private string password;

        public string Host
        {
            get { return host; }
            set { host = value;  }
        }
        public string Port
        {
            get { return port; }
            set { port = value; }
        }
        public string Method
        {
            get { return method; }
            set { method = value; }
        }
        public string Password
        {
            get { return password; }
            set { password = value; }
        }
    }

    public class PasswordGetter
    {
        public PasswordGetter()
        {
        }

        static void Main()
        {
            string issUrl = "http://www.ishadowsocks.org/";
            string mockUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.103 Safari/537.36";
            Debugger.Log(0, null, "Sending request");

            HttpWebResponse response = HttpHelper.CreateGetHttpResponse(issUrl, 0, mockUserAgent, null);
            StreamReader reader = new StreamReader(response.GetResponseStream());
            string htmlDoc = reader.ReadToEnd();
            int serverAIdx = htmlDoc.IndexOf("A服务器地址");
            int serverBIdx = htmlDoc.IndexOf("B服务器地址");
            int serverCIdx = htmlDoc.IndexOf("C服务器地址");

            string serverAInfo = htmlDoc.Substring(serverAIdx, serverBIdx - serverAIdx);
            string serverBInfo = htmlDoc.Substring(serverBIdx, serverCIdx - serverBIdx);

            List<ServerInfo> serverList = new List<ServerInfo>();
            serverList.Add(GetServerInfoObject(serverAInfo));
            serverList.Add(GetServerInfoObject(serverBInfo));

            string ssDir = @"E:\tools\shadowsocks\";
            bool updated = UpdateSSServerInfo(ssDir + "gui-config.json", serverList);

            if(updated)
            {
                StartupShadowSocks(ssDir, "Shadowsocks.exe");
            }
        }

        static ServerInfo GetServerInfoObject(string serverInfoString)
        {
            ServerInfo serverInfo = null;
            if(serverInfoString != null && serverInfoString.Length > 0)
            {
                serverInfo = new ServerInfo();
                string[] infoArray = Regex.Split(serverInfoString, "\\s+<h4>");
                foreach (string s in infoArray)
                {
                    int beginIdx = 0;
                    if (s.Contains("服务器地址:"))
                    {
                        beginIdx = s.IndexOf("服务器地址:") + 6;
                        serverInfo.Host = s.Substring(beginIdx, s.IndexOf("</h4>") - beginIdx);
                    }
                    else if(s.Contains("端口:"))
                    {
                        beginIdx = s.IndexOf("端口:") + 3;
                        serverInfo.Port = s.Substring(beginIdx, s.IndexOf("</h4>") - beginIdx);
                    }
                    else if (s.Contains("密码:"))
                    {
                        beginIdx = s.IndexOf("密码:") + 3;
                        serverInfo.Password = s.Substring(beginIdx, s.IndexOf("</h4>") - beginIdx);
                    }
                    else if (s.Contains("加密方式:"))
                    {
                        beginIdx = s.IndexOf("加密方式:") + 5;
                        serverInfo.Method = s.Substring(beginIdx, s.IndexOf("</h4>") - beginIdx);
                    }
                }
            }

            return serverInfo;
        }

        static bool UpdateSSServerInfo(string path, List<ServerInfo> serverList)
        {
            if(File.Exists(path))
            {
                FileStream fs = File.OpenRead(path);

                //判断文件是文本文件还二进制文件。该方法似乎不科学
                byte b;
                for (long i = 0; i < fs.Length; i++)
                {
                    b = (byte)fs.ReadByte();
                    if (b == 0)
                    {
                        return false;//有此字节则表示改文件不是文本文件。就不用替换了
                    }
                }
                //判断文本文件编码规则。
                byte[] bytes = new byte[2];
                Encoding coding = Encoding.Default;
                if (fs.Read(bytes, 0, 2) > 2)
                {
                    if (bytes == new byte[2] { 0xFF, 0xFE }) coding = Encoding.Unicode;
                    if (bytes == new byte[2] { 0xFE, 0xFF }) coding = Encoding.BigEndianUnicode;
                    if (bytes == new byte[2] { 0xEF, 0xBB }) coding = Encoding.UTF8;
                }
                fs.Close();

                string text = File.ReadAllText(path);
                bool needUpdate = false;
                foreach (ServerInfo si in serverList)
                {
                    int hostIdx = text.IndexOf(si.Host);
                    string serverInfoSection = text.Substring(hostIdx, text.IndexOf("}", hostIdx) - hostIdx);
                    string updatedServerInfoSection = Regex.Replace(serverInfoSection, "\"password\": \".*\",", "\"password\": \"" + si.Password + "\",");
                    if(!serverInfoSection.Equals(updatedServerInfoSection))
                    {
                        needUpdate = true;
                    }

                    text = text.Replace(serverInfoSection, updatedServerInfoSection);
                }

                if(needUpdate)
                {
                    File.Copy(path, path + ".backup", true); // backup
                    File.WriteAllText(path, text, coding);
                    return true;
                }
                else
                {
                    return false;
                }
                
            }
            else
            {
                return false;
            }
        }

        static void StartupShadowSocks(string directory, string filename)
        {

            System.Diagnostics.Process[] process = System.Diagnostics.Process.GetProcessesByName("Shadowsocks");
            foreach (System.Diagnostics.Process p in process)
            {
                p.Kill();
            }

            //设置启动程序的信息
            System.Diagnostics.ProcessStartInfo Info = new System.Diagnostics.ProcessStartInfo();
            //设置外部程序名  
            Info.FileName = filename;
            //设置外部程序工作目录为   C:\\ 
            Info.WorkingDirectory = directory;

            //声明一个程序类  
            System.Diagnostics.Process Proc;
            try
            {
                Proc = System.Diagnostics.Process.Start(Info);
                System.Threading.Thread.Sleep(500);
            }
            catch (System.ComponentModel.Win32Exception)
            {
                return;
            }
        }
    }
}
