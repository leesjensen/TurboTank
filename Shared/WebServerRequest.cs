using Agilix.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace Shared
{
    public interface IParameters
    {
        string GetParam(string name);
        string GetParam(string name, string defaultValue);
    }

    public class WebServerRequest : IParameters
    {
        static private Dictionary<string, string> contentTypeMap = new Dictionary<string, string>()
        {
            { ".html", "text/html" },
            { ".css", "text/css" },
            { ".png", "image/png" },
            { ".js", "application/javascript"}
        };

        private const string DEFAULT_CONTENT_TYPE = "application/json";

        public string ContentType = DEFAULT_CONTENT_TYPE;
        public string ResponseText;
        public HttpStatusCode StatusCode = HttpStatusCode.OK;

        private string userId = "unknown";
        private string responseFilename;

        private Dictionary<string, string> pathParams;
        private HttpListenerRequest baseRequest;
        private HttpListenerResponse baseResponse;

        private DynObject body;

        public WebServerRequest(Dictionary<string, string> pathParams, HttpListenerRequest baseRequest, HttpListenerResponse baseResponse, DynObject logEntry)
        {
            this.pathParams = pathParams;
            this.baseRequest = baseRequest;
            this.baseResponse = baseResponse;
        }

        public string UserId { get { return userId; } }



        public void SetJsonResponse(string jsonText, bool withSingleQuoteReplace = true)
        {
            if (withSingleQuoteReplace)
            {
                jsonText = jsonText.Replace('\'', '"');
            }

            ResponseText = jsonText;
        }


        public string ResponseFilename
        {
            get { return responseFilename; }
            set
            {
                if (value.Contains("..")) throw new HttpException(HttpStatusCode.Forbidden, "Illegal path requested");

                if (ContentType == DEFAULT_CONTENT_TYPE)
                {
                    string extension = Path.GetExtension(value);
                    ContentType = contentTypeMap.TryGetValue(extension, out ContentType) ? ContentType : "text/html";
                }

                responseFilename = value;
            }
        }

        public string GetParam(string name)
        {
            dynamic result = GetParam(name, null);
            if (result == null)
            {
                throw new HttpException(HttpStatusCode.BadRequest, "Missing required parameter {0}", name);
            }

            return result;
        }

        public string GetParam(string name, string defaultValue)
        {
            string result;
            if (!pathParams.TryGetValue(name, out result))
            {
                result = baseRequest.QueryString[name];
                if (result == null)
                {
                    result = defaultValue;
                }
            }

            return result;
        }

        public DynObject GetBody()
        {
            if (body == null)
            {
                using (StreamReader reader = new StreamReader(baseRequest.InputStream, IOUtil.Utf8Encoding))
                {
                    string bodyText = reader.ReadToEnd();
                    if (bodyText == null || bodyText.Length == 0)
                    {
                        bodyText = "{}";
                    }

                    body = DynObject.Parse(bodyText);
                }
            }

            return body;
        }

        private string GetSessionCookie()
        {
            return baseRequest.Cookies["session"] != null ? Base64Decode(baseRequest.Cookies["session"].Value) : "{'userId':'unknown','token':'invalid'}";
        }

        private static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        private static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }
}
