﻿namespace OpenRestClient.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public sealed class RestController : Attribute
    {
        public string? Route { get; private set; }

        public RestController(string route)
        {
            Route = route;
        }

        public RestController()
        {
            Route = null;
        }
    }
}
