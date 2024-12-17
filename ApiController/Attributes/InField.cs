namespace OpenRestClient.Attributes;

[AttributeUsage(AttributeTargets.Parameter)]
public class InField : Attribute
{
    public string? Name { get; set; }

    public InField(string name)
    {
        Name = name;
    }

    public InField() { }
}
