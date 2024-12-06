using OpenRestController.Enums;

namespace OpenRestClient.Attributes
{
    public class DeleteMapping : RestMethod
    {
        public DeleteMapping(string route) : base(route, MethodType.DELETE) { }
        public DeleteMapping() : base(MethodType.DELETE) { }
    }
}
