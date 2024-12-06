using OpenRestController.Enums;

namespace OpenRestClient.Attributes
{
    public class PostMapping : RestMethod
    {
        public PostMapping(string route) : base(route, MethodType.POST) { }
        public PostMapping() : base(MethodType.POST) { }
    }
}
