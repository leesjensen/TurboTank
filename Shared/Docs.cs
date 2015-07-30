using Agilix.Shared;

namespace Shared
{
    [System.AttributeUsage(System.AttributeTargets.Method |
                           System.AttributeTargets.Struct,
                           AllowMultiple = true)
    ]
    public class Docs : System.Attribute
    {
        public string name;
        public string description;
        public string method = "GET";
        public string requestPattern;
        public string requestExample;
        public string requestExampleBody;
        public string responseExample;
        public bool isInternal;
        public bool isSecure = true;

        public DynObject Serialize()
        {
            DynObject serialization = new DynObject();
            AddProperty("name", name, serialization);
            AddProperty("description", description, serialization);
            AddProperty("method", method, serialization);
            AddProperty("requestPattern", requestPattern, serialization);
            AddProperty("requestExample", requestExample, serialization);
            AddObject("requestExampleBody", requestExampleBody, serialization);
            AddObject("responseExample", responseExample, serialization);

            return serialization;
        }

        private void AddProperty(string name, string value, DynObject serialization)
        {
            if (value != null && value.Length > 0)
            {
                serialization[name] = value;
            }
        }

        private void AddObject(string name, string value, DynObject serialization)
        {
            if (value != null && value.Length > 0)
            {
                if (value[0] == '{')
                {
                    serialization[name] = DynObject.Parse(value);
                }
                else
                {
                    serialization[name] = value;
                }
            }
        }

    }



}
