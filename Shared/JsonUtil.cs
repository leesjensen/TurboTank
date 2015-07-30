using Agilix.Shared;
using System.Net;

namespace Shared
{
    public static class JsonUtil
    {
        public static dynamic Get(dynamic json, string name)
        {
            dynamic result = Get(json, name, null);
            if (result == null)
            {
                throw new HttpException(HttpStatusCode.BadRequest, "Missing required parameter {0}", name);
            }

            return result;
        }

        public static dynamic Get(dynamic json, string name, dynamic defaultValue)
        {
            dynamic result = null;
            if (json is DynObject)
            {
                result = json[name];
            }

            if (result == null)
            {
                result = defaultValue;
            }

            return result;
        }

    }
}
