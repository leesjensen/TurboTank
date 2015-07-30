using System;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Shared
{
    public delegate void WebServerDelegate(WebServerRequest ws);

    public class RequestHandler
    {
        public Regex MethodRegex;
        public Regex ResourceRegex;
        public WebServerDelegate Callback;
        public Docs Documentation;

        public RequestHandler(WebServerDelegate callback)
            : this(((Docs[])callback.Method.GetCustomAttributes(typeof(Docs), false))[0], callback)
        {
        }

        public RequestHandler(Docs documentation, WebServerDelegate callback)
        {
            this.Callback = callback;
            this.Documentation = documentation;
            try
            {
                this.MethodRegex = new Regex(Documentation.method, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                this.ResourceRegex = new Regex(parsePatternText(Documentation.requestPattern), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            }
            catch (Exception ex)
            {
                throw new HttpException(HttpStatusCode.InternalServerError, "Unable to parse the docs for {0}. {1}", Documentation.name, ex.Message);
            }
        }



        protected static String parsePatternText(String patternText)
        {
            StringBuilder resultingPattern = new StringBuilder("^");

            int startPos;
            int searchStart = 0;
            while ((startPos = patternText.IndexOf('{', searchStart)) != -1)
            {
                string prefix = patternText.Substring(searchStart, startPos - searchStart);
                resultingPattern.Append(prefix);

                int endPos = patternText.IndexOf('}', startPos);
                if (endPos == -1) throw new Exception("Invalid resource pattern. Missing closing brace '}'.");

                String paramName = patternText.Substring(startPos + 1, endPos - startPos - 1);
                String regexText = "[^/]+";
                int regExpPos = paramName.IndexOf(':');
                if (regExpPos != -1)
                {
                    regexText = paramName.Substring(regExpPos + 1);
                    paramName = paramName.Substring(0, regExpPos);
                }

                resultingPattern.AppendFormat("(?<{0}>{1})", paramName, regexText);

                searchStart = endPos + 1;
            }

            resultingPattern.Append(patternText.Substring(searchStart));
            resultingPattern.Append("$");

            return resultingPattern.ToString();
        }

        public override string ToString()
        {
            return Documentation.requestPattern;
        }
    }
}
