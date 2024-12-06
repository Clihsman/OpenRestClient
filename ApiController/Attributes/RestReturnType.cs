namespace OpenRestClient.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RestReturnType : Attribute
    {
        public Type ReturnType { get; private set; }
        public bool IsArray { get; private set; }

        public RestReturnType(Type returnType) {
          ReturnType = returnType;
        }

        public RestReturnType(Type returnType, bool isArray)
        {
            ReturnType = returnType;
            IsArray = isArray;
        }
    }
}
