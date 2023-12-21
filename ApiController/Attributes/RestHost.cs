using System;

namespace OpenRestController.Attributes
{
    [AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
    public class RestHost : Attribute
    {
        public string Host { get; set; }

        public RestHost(string host) {
            Host = host;
        }
    }
}
