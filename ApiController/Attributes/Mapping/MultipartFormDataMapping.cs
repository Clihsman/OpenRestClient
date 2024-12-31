using OpenRestController.Enums;

namespace OpenRestClient.Attributes;

public class MultipartFormDataMapping : RestMethod
{
    public MultipartFormDataMapping() : base(MethodType.POST) { }
    public MultipartFormDataMapping(string route) : base(route, MethodType.POST) { }
}
