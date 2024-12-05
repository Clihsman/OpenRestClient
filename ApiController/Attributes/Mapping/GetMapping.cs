using OpenRestController.Attributes;
using OpenRestController.Enums;

namespace OpenRestClient.Attributes
{
    public class GetMapping : RestMethod
    {
        public GetMapping(string route) : base(route, MethodType.GET) { }
        public GetMapping() : base(MethodType.GET) { }
    }
}
