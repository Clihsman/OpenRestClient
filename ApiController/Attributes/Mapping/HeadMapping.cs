using OpenRestClient.Attributes;
using OpenRestController.Enums;

namespace OpenRestClient;

public class HeadMapping : RestMethod
{
    public HeadMapping(string route) : base(route, MethodType.HEAD) { }

    public HeadMapping() : base(MethodType.HEAD) { }
}
