using System.Net;

namespace OpenRestClient.ApiController.Exceptions
{
    public class RestException : Exception
    {
        public HttpStatusCode HttpStatusCode;

        public RestException(HttpStatusCode httpStatusCode, string message) : base(message) {
            HttpStatusCode = httpStatusCode;
        }
    }
}
