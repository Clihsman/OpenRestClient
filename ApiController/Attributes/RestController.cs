using System;

namespace OpenRestController.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class RestController : Attribute
    {
        public string Route { get; private set; }

        public RestController(string route)
        {
            Route = route;
        }
    }
}
