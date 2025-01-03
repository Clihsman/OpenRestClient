using OpenRestController.Enums;

namespace OpenRestClient;

public sealed class HttpRequest(MethodType method, string url, string? contentType, string? content)
{
    public MethodType Method { get; private set; } = method;
    public string Url { get; private set; } = url;
    public string? ContentType { get; private set; } = contentType;
    public string? Content { get; private set; } = content;
    public bool Cancel { get; set; } = false;
}
