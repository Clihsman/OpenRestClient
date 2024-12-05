using OpenRestController.Attributes;
using OpenRestController.Enums;

namespace OpenRestClient.Attributes
{
    public class PutMapping : RestMethod
    {
        public PutMapping(string route) : base(route, MethodType.PUT) { }
        public PutMapping() : base(MethodType.PUT) { }
    }
}
