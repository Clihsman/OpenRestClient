using System.Net;

namespace OpenRestClient
{
    public class RestResponse<T>
    {
        public T? Value { get; private set; }
        public HttpResponseMessage HttpResponse { get; private set; }
        public HttpStatusCode HttpStatusCode => HttpResponse.StatusCode;

        public RestResponse(T? value, HttpResponseMessage httpResponse) {
            Value = value;
            HttpResponse = httpResponse;
        }

        public RestResponse(HttpResponseMessage httpResponse)
        {
            HttpResponse = httpResponse;
        }
    }
}
