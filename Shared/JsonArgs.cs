using Agilix.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public class JsonArgs
    {
        public DynObject Options = new DynObject();

        public JsonArgs(string[] args)
        {
            foreach (String arg in args)
            {
                if ((arg[0] == '-') || (arg[0] == '/'))
                {
                    string name = arg.Substring(1);
                    string data = "true";

                    int delimPos = arg.IndexOf("=");
                    if (delimPos != -1)
                    {
                        name = arg.Substring(1, delimPos - 1);
                        data = arg.Substring(delimPos + 1);
                    }

                    Options[name] = data;
                }
            }
        }

        public bool HasParam(string param)
        {
            return (Options[param] != null);
        }

        public string GetParam(string param)
        {
            string result = GetParam(param, null);
            if (result == null)
            {
                throw new Exception("Required parameter was not provided: " + param);
            }

            return result;
        }

        public string GetParam(string param, string defaultValue)
        {
            string result;
            if ((result = (string)Options[param]) == null)
            {
                result = defaultValue;
            }

            return result;
        }
    }



}
