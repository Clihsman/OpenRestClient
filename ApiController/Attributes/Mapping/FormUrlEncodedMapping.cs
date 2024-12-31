using OpenRestController.Enums;

namespace OpenRestClient.Attributes;

public class FormUrlEncodedMapping : RestMethod
{
    public FormUrlEncodedMapping(string route) : base(route, MethodType.POST) { }

    public FormUrlEncodedMapping() : base(MethodType.POST) { }
}
