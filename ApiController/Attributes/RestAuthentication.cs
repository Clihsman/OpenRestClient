namespace OpenRestClient.Attributes
{
    public abstract class RestAuthentication : Attribute
    {
        public AuthenticationType AuthenticationType { get; private set; }
        public AuthenticationMode AuthenticationMode { get; private set; }
        public string ObjectPath { get; private set; }

        public RestAuthentication(AuthenticationType type, AuthenticationMode mode, string objectPath) {
            AuthenticationType = type;
            AuthenticationMode = mode;
            ObjectPath = objectPath;
        }
    }
}
