using Agilix.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Shared
{
    public static class HttpUtil
    {
        public static bool IsServerAvailable(string hostname, int port, int connectionTimeout)
        {
            bool result = false;
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {

                IPAddress ipAddress = IPAddress.Parse(hostname);
                IPEndPoint endPoint = new IPEndPoint(ipAddress, port);

                try
                {
                    socket.Blocking = false;
                    socket.Connect(endPoint);
                    result = true;
                }
                catch (SocketException socketException)
                {
                    if (socketException.ErrorCode == (int)SocketError.WouldBlock)
                    {
                        int timeoutMicroseconds = connectionTimeout * 1000;
                        if (socket.Poll(timeoutMicroseconds, SelectMode.SelectWrite))
                        {
                            result = true;
                        }
                    }
                }
            }

            return result;
        }
    }


    public class HttpClient
    {
        public string Host;
        public int Port;
        public int ReadWriteTimeout = Timeout.Infinite;
        public int ConntectionTimeout = Timeout.Infinite;

        private CookieContainer cookieContainer = new CookieContainer();

        static HttpClient()
        {
            ServicePointManager.DefaultConnectionLimit = 1000;
            ServicePointManager.UseNagleAlgorithm = false;
        }

        public HttpClient(string host, int port)
        {
            this.Host = host;
            this.Port = port;
        }


        public string GetTextResponse(string resource, string method = "GET", string body = null, Dictionary<string, string> headers = null, HttpStatusCode expectedStatus = HttpStatusCode.OK)
        {
            using (HttpWebResponse response = GetWebResponse(resource, method, body, headers))
            {

                string responseText = null;
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    responseText = reader.ReadToEnd();
                }

                if (expectedStatus != response.StatusCode)
                {
                    throw new HttpException(response.StatusCode, "Expected status code {0} for request to [{1}] http://{2}:{3}{4}. Got {5} - {6}.", expectedStatus, method, Host, Port, resource, response.StatusCode, responseText);
                }

                return responseText;
            }
        }


        public dynamic GetJsonResponse(string resource, string method = "GET", string body = null, Dictionary<string, string> headers = null, HttpStatusCode expectedStatus = HttpStatusCode.OK)
        {
            string responseText = GetTextResponse(resource, method, body, headers, expectedStatus);
            if (responseText.Length > 0)
            {
                if (responseText[0] == '{')
                {
                    return DynObject.Parse(responseText);
                }
                else
                {
                    return DynArray.Parse(responseText);
                }
            }
            else
            {
                return null;
            }
        }


        private HttpWebResponse GetWebResponse(string resource, string method = "GET", string body = null, Dictionary<string, string> headers = null)
        {
            Stopwatch requestTimer = Stopwatch.StartNew();
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://" + Host + ":" + Port + resource);
            request.Method = method;
            request.Accept = "*/*";
            request.ReadWriteTimeout = ReadWriteTimeout;
            request.Timeout = ConntectionTimeout;
            request.ContentType = "application/json; charset=utf-8";
            request.UserAgent = "Mozilla/5.0 (Windows NT; Windows NT 6.3; en-US) TurboTank";
            request.CookieContainer = cookieContainer;

            foreach (KeyValuePair<string, string> header in headers)
            {
                request.Headers.Add(header.Key, header.Value);
            }

            if (body != null && body.Length > 0)
            {
                using (StreamWriter writer = new StreamWriter(request.GetRequestStream(), IOUtil.Utf8Encoding))
                {
                    writer.Write(body);
                }
            }
            else
            {
                request.ContentLength = 0;
            }

            HttpWebResponse response;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException ex)
            {
                response = (HttpWebResponse)ex.Response;
                if (response == null) throw ex;
            }


            if (headers != null)
            {
                foreach (string header in response.Headers)
                {
                    headers[header] = response.Headers.Get(header);
                }
            }

            LogRequest(resource, method, response, requestTimer.ElapsedMilliseconds);

            return response;
        }


        private void LogRequest(string resource, string method, HttpWebResponse response, double responseTime)
        {

            DynObject json = new DynObject();
            json["action"] = "clientReqEnd";
            json["httpMethod"] = method;
            json["host"] = Host;
            json["port"] = Port;
            json["url"] = resource;
            json["httpStatus"] = response.StatusCode;
            json["responseTime"] = responseTime;

            Logger.Info(json);
        }


    }
}
