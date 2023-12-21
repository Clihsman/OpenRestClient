﻿using OpenRestController.Enums;
using System;

namespace OpenRestController.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class RestMethod : Attribute
    {
        public string Route { get; private set; }
        public MethodType Method { get; private set; }

        public RestMethod(string route, MethodType method)
        {
            Route = route;
            Method = method;
        }

        public RestMethod(MethodType method)
        {
            Method = method;
        }
    }
}
