using OpenRestClient.Attributes;
using OpenRestController.Enums;

namespace OpenRestClient;

public class PatchMapping : RestMethod
{
    public PatchMapping(string route) : base(route, MethodType.PATCH) { }

    public PatchMapping() : base(MethodType.PATCH) { }
}
