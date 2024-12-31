namespace OpenRestClient.Attributes;

public class InFormStream : InField
{
    public int? BufferSize { get; private set; }

    public InFormStream() { }

    public InFormStream(string name) {
        Name = name;
    }

    public InFormStream(string name, int bufferSize)
    {
        Name = name;
        BufferSize = bufferSize;
    }
}
