using Newtonsoft.Json;
using System.Net;

namespace OpenRestClient.ApiController.Exceptions
{
    public class RestException(HttpStatusCode httpStatusCode, string message) : Exception(message)
    {
        public HttpStatusCode HttpStatusCode = httpStatusCode;

        public Dictionary<string, object>? ToDictionary() {
            return JsonConvert.DeserializeObject<Dictionary<string, object>>(Message);
        }

        public T? ToObject<T>()
        {
            return JsonConvert.DeserializeObject<T>(Message);
        }
    }
}
