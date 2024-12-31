namespace OpenRestClient.ApiController.Attributes;

[AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
public class RestDebug : Attribute
{
    public RestDebug() { }
}
