using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenRestClient.ApiController.Attributes;
using OpenRestClient.ApiController.Exceptions;
using OpenRestClient.Attributes;
using OpenRestController.Enums;
using System.Collections.Specialized;
using System.Diagnostics.SymbolStore;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace OpenRestClient
{
    public class RestApp
    {
        private string? Host { get; set; }
        private Dictionary<string, Dictionary<int, MethodArgs>> Methods { get; set; }
        private Dictionary<string, string> Headers { get; set; }

        internal static Dictionary<string, string> Env = [];

        private HttpClient HttpClient { get; set; }

        private List<Middleware> Middlewares = [];

        protected RestApp(string host, Type type)
        {
            Host = host;
            HttpClient = new HttpClient();
            Headers = [];
            Methods = [];
            AddUrls(this, GetRoute(type), type);
            LoadVariables();
        }

        protected RestApp(string host, Type type, HttpClient httpClient)
        {
            Host = host;
            HttpClient = httpClient;
            Headers = [];
            Methods = [];
            AddUrls(this, GetRoute(type), type);
            LoadVariables();
        }

        protected RestApp(Type type, HttpClient httpClient)
        {
            Host = GetHost(type);
            HttpClient = httpClient;
            Headers = [];
            Methods = [];
            AddUrls(this, GetRoute(type), type);
            LoadVariables();
        }

        protected RestApp(Type type)
        {
            Host = GetHost(type);
            HttpClient = new HttpClient();
            Headers = [];
            Methods = [];
            AddUrls(this, GetRoute(type), type);
            LoadVariables();
        }

        private void LoadVariables()
        {
            string? host = Environment.GetEnvironmentVariable("opendev.openrestclient.host");
            if (host is not null)
                Host = host;

            string? token = Env.GetValueOrDefault("opendev.openrestclient.jwt.bearer.token");
            if (token is not null)
                AddHeader("Authorization", $"Bearer {token}");
        }

        protected async Task<string?> CallString(string method, params object[] args)
        {
            (MethodArgs methodArgs, string url) = GetUrl(method, args);
            using HttpClient client = new();

            foreach (var header in Headers)
                client.DefaultRequestHeaders.Add(header.Key, header.Value);

            if (methodArgs.MethodType == MethodType.GET)
            {
                HttpResponseMessage response = await client.GetAsync(url);
                string dataResponse = await response.Content.ReadAsStringAsync();

                var httpRequest = new HttpRequest(methodArgs.MethodType, url, null, null);
                CallMiddlewares(response, httpRequest);

                if (!response.IsSuccessStatusCode)
                    throw new RestException(response.StatusCode, dataResponse);

                return dataResponse;
            }

            if (methodArgs.MethodType == MethodType.POST)
            {
                string dataRequest = JsonConvert.SerializeObject(args[methodArgs.BodyIndex]);

                var httpContent = new StringContent(dataRequest, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(url, httpContent);
                string dataResponse = await response.Content.ReadAsStringAsync();

                var httpRequest = new HttpRequest(methodArgs.MethodType, url, "application/json", dataRequest);
                CallMiddlewares(response, httpRequest);

                if (!response.IsSuccessStatusCode)
                    throw new RestException(response.StatusCode, dataResponse);

                return dataResponse;
            }

            if (methodArgs.MethodType == MethodType.PATCH)
            {
                string dataRequest = JsonConvert.SerializeObject(args[methodArgs.BodyIndex]);

                var httpContent = new StringContent(dataRequest, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PatchAsync(url, httpContent);
                string dataResponse = await response.Content.ReadAsStringAsync();

                var httpRequest = new HttpRequest(methodArgs.MethodType, url, "application/json", dataRequest);
                CallMiddlewares(response, httpRequest);

                if (!response.IsSuccessStatusCode)
                    throw new RestException(response.StatusCode, dataResponse);

                return dataResponse;
            }

            if (methodArgs.MethodType == MethodType.PUT)
            {
                string dataRequest = JsonConvert.SerializeObject(args[methodArgs.BodyIndex]);

                var httpContent = new StringContent(dataRequest, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PutAsync(url, httpContent);
                string dataResponse = await response.Content.ReadAsStringAsync();

                var httpRequest = new HttpRequest(methodArgs.MethodType, url, "application/json", dataRequest);
                CallMiddlewares(response, httpRequest);

                if (!response.IsSuccessStatusCode)
                    throw new RestException(response.StatusCode, dataResponse);

                return dataResponse;
            }

            if (methodArgs.MethodType == MethodType.DELETE)
            {
                HttpResponseMessage response = await client.DeleteAsync(url);
                string dataResponse = await response.Content.ReadAsStringAsync();

                var httpRequest = new HttpRequest(methodArgs.MethodType, url, null, null);
                CallMiddlewares(response, httpRequest);

                if (!response.IsSuccessStatusCode)
                    throw new RestException(response.StatusCode, dataResponse);

                return await response.Content.ReadAsStringAsync();
            }
            return null;
        }

        protected internal async Task<T?> Call<T>(string method, params object[] args)
        {
            LoadVariables();

            (MethodArgs methodArgs, string url) = GetUrl(method, args);

            foreach (var header in Headers)
            {
                HttpClient.DefaultRequestHeaders.Remove(header.Key);
                HttpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
            }

            if (methodArgs.RestMethod is MultipartFormDataMapping)
            {
                using MultipartFormDataContent multipartForm = [];
                FileStream? fileStream = null;

                foreach (Tuple<InField, object?> field in methodArgs.InFields)
                {
                    if (field.Item1 is InPart)
                    {
                        multipartForm.Add(new StringContent((string)field.Item2!), field.Item1.Name!);
                    }

                    if (field.Item1 is InFormStream)
                    {
                        multipartForm.Add(new StreamContent((Stream)field.Item2!), field.Item1.Name!);
                    }

                    if (field.Item1 is InFormFile)
                    {
                        fileStream = new((string)field.Item2!, FileMode.Open, FileAccess.Read);
                        multipartForm.Add(new StreamContent(fileStream), field.Item1.Name!, Path.GetFileName((string)field.Item2!));
                    }
                }

                HttpResponseMessage response = await HttpClient.PostAsync(url, multipartForm);
                string dataResponse = await response.Content.ReadAsStringAsync();
                fileStream?.Close();

                var httpRequest = new HttpRequest(methodArgs.MethodType, url, "not_implement", null);
                CallMiddlewares(response, httpRequest);

                if (!response.IsSuccessStatusCode)
                    throw new RestException(response.StatusCode, dataResponse);

                return JsonConvert.DeserializeObject<T>(dataResponse);

            }

            if (methodArgs.RestMethod is FormUrlEncodedMapping)
            {
                FormUrlEncodedContent form = new(
                    methodArgs.InFields.Where(e => e.Item1 is InPart).Select(e => new KeyValuePair<string, string>(e.Item1.Name!, (string)e.Item2!))
                 );

                HttpResponseMessage response = await HttpClient.PostAsync(url, form);
                string dataResponse = await response.Content.ReadAsStringAsync();

                var httpRequest = new HttpRequest(methodArgs.MethodType, url, "not_implement", null);
                CallMiddlewares(response, httpRequest);

                if (!response.IsSuccessStatusCode)
                    throw new RestException(response.StatusCode, dataResponse);

                return JsonConvert.DeserializeObject<T>(dataResponse);

            }


            if (methodArgs.MethodType == MethodType.DELETE)
            {
                HttpResponseMessage response = await HttpClient.DeleteAsync(url);
                string json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    throw new RestException(response.StatusCode, json);

                return JsonConvert.DeserializeObject<T>(json);

            }

            if (methodArgs.MethodType == MethodType.GET)
            {
                HttpResponseMessage response = await HttpClient.GetAsync(url);
                string dataResponse = await response.Content.ReadAsStringAsync();

                var httpRequest = new HttpRequest(methodArgs.MethodType, url, null, null);
                CallMiddlewares(response, httpRequest);

                if (!response.IsSuccessStatusCode)
                    throw new RestException(response.StatusCode, dataResponse);

                return JsonConvert.DeserializeObject<T>(dataResponse);
            }

            if (methodArgs.MethodType == MethodType.POST)
            {
                string dataRequest = methodArgs.ContainsBody ?
                    JsonConvert.SerializeObject(args[methodArgs.BodyIndex]) : string.Empty;

                HttpContent? httpContent = methodArgs.ContainsBody ?
                    new StringContent(dataRequest, Encoding.UTF8, "application/json") : null;

                HttpResponseMessage response = await HttpClient.PostAsync(url, httpContent);
                string dataResponse = await response.Content.ReadAsStringAsync();

                RestAuthentication? restAuthentication = methodArgs.MethodInfo.GetCustomAttribute<RestAuthentication>();

                if (restAuthentication is not null && response.IsSuccessStatusCode)
                {
                    LoadRestAuthentication(restAuthentication, dataResponse);
                }

                var httpRequest = new HttpRequest(methodArgs.MethodType, url, "application/json", dataRequest);
                CallMiddlewares(response, httpRequest);

                if (!response.IsSuccessStatusCode)
                    throw new RestException(response.StatusCode, dataResponse);

                return JsonConvert.DeserializeObject<T>(dataResponse);
            }

            if (methodArgs.MethodType == MethodType.PUT)
            {
                string dataRequest = JsonConvert.SerializeObject(args[methodArgs.BodyIndex]);
                var httpContent = new StringContent(dataRequest, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await HttpClient.PutAsync(url, httpContent);
                string dataResponse = await response.Content.ReadAsStringAsync();

                var httpRequest = new HttpRequest(methodArgs.MethodType, url, "application/json", dataRequest);
                CallMiddlewares(response, httpRequest);

                if (!response.IsSuccessStatusCode)
                    throw new RestException(response.StatusCode, dataRequest);

                return JsonConvert.DeserializeObject<T>(dataResponse);
            }

            if (methodArgs.MethodType == MethodType.PATCH)
            {
                string dataRequest = JsonConvert.SerializeObject(args[methodArgs.BodyIndex]);
                var httpContent = new StringContent(dataRequest, Encoding.UTF8, "application/json");

                Console.WriteLine(dataRequest);

                HttpResponseMessage response = await HttpClient.PatchAsync(url, httpContent);
                string dataResonse = await response.Content.ReadAsStringAsync();

                var httpRequest = new HttpRequest(methodArgs.MethodType, url, "application/json", dataRequest);
                CallMiddlewares(response, httpRequest);

                if (!response.IsSuccessStatusCode)
                    throw new RestException(response.StatusCode, dataRequest);

                return JsonConvert.DeserializeObject<T>(dataResonse);
            }

            if (methodArgs.MethodType == MethodType.DELETE)
            {
                HttpResponseMessage response = await HttpClient.DeleteAsync(url);
                string dataResponse = await response.Content.ReadAsStringAsync();

                var httpRequest = new HttpRequest(methodArgs.MethodType, url, null, null);
                CallMiddlewares(response, httpRequest);

                if (!response.IsSuccessStatusCode)
                    throw new RestException(response.StatusCode, dataResponse);

                return JsonConvert.DeserializeObject<T>(dataResponse);
            }

            throw new Exception();
        }

        protected internal async Task<object?> Call(string method, Type type, params object[] args)
        {
            LoadVariables();


            (MethodArgs methodArgs, string url) = GetUrl(method, args);

            using HttpClient client = new();

            foreach (var header in Headers)
                client.DefaultRequestHeaders.Add(header.Key, header.Value);

            if (methodArgs.MethodType == MethodType.GET)
            {
                HttpResponseMessage response = await client.GetAsync(url);
                string dataResponse = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    throw new RestException(response.StatusCode, dataResponse);

                return JsonConvert.DeserializeObject(dataResponse, type);
            }

            if (methodArgs.MethodType == MethodType.POST)
            {
                string dataRequest = methodArgs.ContainsBody ?
                    JsonConvert.SerializeObject(args[methodArgs.BodyIndex]) : string.Empty;

                HttpContent? httpContent = methodArgs.ContainsBody ?
                    new StringContent(dataRequest, Encoding.UTF8, "application/json") : null;

                HttpResponseMessage response = await client.PostAsync(url, httpContent);
                string dataResponse = await response.Content.ReadAsStringAsync();

                RestAuthentication? restAuthentication = methodArgs.MethodInfo.GetCustomAttribute<RestAuthentication>();

                if (restAuthentication is not null && response.IsSuccessStatusCode)
                {
                    LoadRestAuthentication(restAuthentication, dataResponse);
                }

                if (!response.IsSuccessStatusCode)
                    throw new RestException(response.StatusCode, dataResponse);

                return JsonConvert.DeserializeObject(dataResponse, type);
            }

            if (methodArgs.MethodType == MethodType.PUT)
            {
                string dataRequest = JsonConvert.SerializeObject(args[0]);
                var httpContent = new StringContent(dataRequest, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PutAsync(url, httpContent);
                string dataResponse = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    throw new RestException(response.StatusCode, dataRequest);

                return JsonConvert.DeserializeObject(dataResponse, type);
            }

            if (methodArgs.MethodType == MethodType.PATCH)
            {
                string dataRequest = JsonConvert.SerializeObject(args[0]);
                var httpContent = new StringContent(dataRequest, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PatchAsync(url, httpContent);
                string dataResponse = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    throw new RestException(response.StatusCode, dataRequest);

                return JsonConvert.DeserializeObject(dataResponse, type);
            }


            if (methodArgs.MethodType == MethodType.DELETE)
            {
                HttpResponseMessage response = await client.DeleteAsync(url);
                string dataResponse = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    throw new RestException(response.StatusCode, dataResponse);

                return JsonConvert.DeserializeObject(dataResponse, type);
            }

            throw new Exception();

        }

        protected async Task<RestResponse<T?>> CallResponse<T>(string method, params object[] args)
        {
            LoadVariables();

            (MethodArgs methodArgs, string url) = GetUrl(method, args);

            foreach (var header in Headers)
            {
                HttpClient.DefaultRequestHeaders.Remove(header.Key);
                HttpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
            }

            if (methodArgs.RestMethod is FormUrlEncodedMapping)
            {
                FormUrlEncodedContent form = new(
                    methodArgs.InFields.Where(e => e.Item1 is InPart).Select(e => new KeyValuePair<string, string>(e.Item1.Name!, (string)e.Item2!))
                 );

                HttpResponseMessage response = await HttpClient.PostAsync(url, form);
                string dataResponse = await response.Content.ReadAsStringAsync();

                var httpRequest = new HttpRequest(methodArgs.MethodType, url, "not_implement", null);
                CallMiddlewares(response, httpRequest);

                if (!response.IsSuccessStatusCode)
                    throw new RestException(response.StatusCode, dataResponse);

                return new RestResponse<T?>(response);
            }

            if (methodArgs.MethodType == MethodType.GET)
            {
                HttpResponseMessage response = await HttpClient.GetAsync(url);
                string dataResponse = await response.Content.ReadAsStringAsync();

                var httpRequest = new HttpRequest(methodArgs.MethodType, url, null, null);
                CallMiddlewares(response, httpRequest);

                if (response.IsSuccessStatusCode)
                {
                    T? value = JsonConvert.DeserializeObject<T>(dataResponse);
                    return new RestResponse<T?>(value, response);
                }

                return new RestResponse<T?>(response);
            }

            if (methodArgs.MethodType == MethodType.POST)
            {
                string dataRequest = methodArgs.ContainsBody ?
                    JsonConvert.SerializeObject(args[methodArgs.BodyIndex]) : string.Empty;

                HttpContent? httpContent = methodArgs.ContainsBody ?
                    new StringContent(dataRequest, Encoding.UTF8, "application/json") : null;

                HttpResponseMessage response = await HttpClient.PostAsync(url, httpContent);
                string dataResponse = await response.Content.ReadAsStringAsync();

                RestAuthentication? restAuthentication = methodArgs.MethodInfo.GetCustomAttribute<RestAuthentication>();

                if (restAuthentication is not null && response.IsSuccessStatusCode)
                {
                    LoadRestAuthentication(restAuthentication, dataResponse);
                }

                var httpRequest = new HttpRequest(methodArgs.MethodType, url, "application/json", dataRequest);
                CallMiddlewares(response, httpRequest);

                if (response.IsSuccessStatusCode)
                {
                    T? value = JsonConvert.DeserializeObject<T>(dataResponse);
                    return new RestResponse<T?>(value, response);
                }


                return new RestResponse<T?>(response);
            }

            if (methodArgs.MethodType == MethodType.PUT)
            {
                string dataRequest = JsonConvert.SerializeObject(args[methodArgs.BodyIndex]);
                var httpContent = new StringContent(dataRequest, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await HttpClient.PutAsync(url, httpContent);
                string dataResponse = await response.Content.ReadAsStringAsync();

                var httpRequest = new HttpRequest(methodArgs.MethodType, url, "application/json", dataRequest);
                CallMiddlewares(response, httpRequest);

                if (response.IsSuccessStatusCode)
                {
                    T? value = JsonConvert.DeserializeObject<T>(dataResponse);
                    return new RestResponse<T?>(value, response);
                }

                return new RestResponse<T?>(response);
            }

            if (methodArgs.MethodType == MethodType.PATCH)
            {
                string dataRequest = JsonConvert.SerializeObject(args[methodArgs.BodyIndex]);
                var httpContent = new StringContent(dataRequest, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await HttpClient.PatchAsync(url, httpContent);
                string dataResponse = await response.Content.ReadAsStringAsync();

                var httpRequest = new HttpRequest(methodArgs.MethodType, url, "application/json", dataRequest);
                CallMiddlewares(response, httpRequest);

                if (response.IsSuccessStatusCode)
                {
                    T? value = JsonConvert.DeserializeObject<T>(dataResponse);
                    return new RestResponse<T?>(value, response);
                }

                return new RestResponse<T?>(response);
            }


            if (methodArgs.MethodType == MethodType.DELETE)
            {
                HttpResponseMessage response = await HttpClient.DeleteAsync(url);
                string dataResponse = await response.Content.ReadAsStringAsync();

                var httpRequest = new HttpRequest(methodArgs.MethodType, url, null, null);
                CallMiddlewares(response, httpRequest);

                if (response.IsSuccessStatusCode)
                {
                    T? value = JsonConvert.DeserializeObject<T>(dataResponse);
                    return new RestResponse<T?>(value, response);
                }

                return new RestResponse<T?>(response);
            }

            throw new Exception();
        }
       
        protected async Task<RestResponse> CallResponse(string method, params object[] args)
        {
            LoadVariables();

            (MethodArgs methodArgs, string url) = GetUrl(method, args);

            foreach (var header in Headers)
            {
                HttpClient.DefaultRequestHeaders.Remove(header.Key);
                HttpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
            }

            if (methodArgs.RestMethod is FormUrlEncodedMapping)
            {
                FormUrlEncodedContent form = new(
                    methodArgs.InFields.Where(e => e.Item1 is InPart).Select(e => new KeyValuePair<string, string>(e.Item1.Name!, (string)e.Item2!))
                 );

                HttpResponseMessage response = await HttpClient.PostAsync(url, form);
                string dataResponse = await response.Content.ReadAsStringAsync();

                var httpRequest = new HttpRequest(methodArgs.MethodType, url, "not_implement", null);
                CallMiddlewares(response, httpRequest);

                if (!response.IsSuccessStatusCode)
                    return new RestResponse(response);

                return new RestResponse(response);
            }

            if (methodArgs.MethodType == MethodType.GET)
            {
                HttpResponseMessage response = await HttpClient.GetAsync(url);
                string dataResponse = await response.Content.ReadAsStringAsync();

                var httpRequest = new HttpRequest(methodArgs.MethodType, url, null, null);
                CallMiddlewares(response, httpRequest);

                if (response.IsSuccessStatusCode)
                    return new RestResponse(response);

                return new RestResponse(response);
            }

            if (methodArgs.MethodType == MethodType.POST)
            {
                string dataRequest = methodArgs.ContainsBody ?
                    JsonConvert.SerializeObject(args[methodArgs.BodyIndex]) : string.Empty;

                HttpContent? httpContent = methodArgs.ContainsBody ?
                    new StringContent(dataRequest, Encoding.UTF8, "application/json") : null;

                HttpResponseMessage response = await HttpClient.PostAsync(url, httpContent);
                string dataResponse = await response.Content.ReadAsStringAsync();

                RestAuthentication? restAuthentication = methodArgs.MethodInfo.GetCustomAttribute<RestAuthentication>();

                if (restAuthentication is not null && response.IsSuccessStatusCode)
                {
                    LoadRestAuthentication(restAuthentication, dataResponse);
                }

                var httpRequest = new HttpRequest(methodArgs.MethodType, url, "application/json", dataRequest);
                CallMiddlewares(response, httpRequest);

                if (response.IsSuccessStatusCode)
                    return new RestResponse(response);

                return new RestResponse(response);
            }

            if (methodArgs.MethodType == MethodType.PUT)
            {
                string dataRequest = JsonConvert.SerializeObject(args[methodArgs.BodyIndex]);
                var httpContent = new StringContent(dataRequest, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await HttpClient.PutAsync(url, httpContent);
                string dataResponse = await response.Content.ReadAsStringAsync();

                var httpRequest = new HttpRequest(methodArgs.MethodType, url, "application/json", dataRequest);
                CallMiddlewares(response, httpRequest);

                if (response.IsSuccessStatusCode)
                    return new RestResponse(response);

                return new RestResponse(response);
            }

            if (methodArgs.MethodType == MethodType.PATCH)
            {
                string dataRequest = JsonConvert.SerializeObject(args[methodArgs.BodyIndex]);
                var httpContent = new StringContent(dataRequest, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await HttpClient.PatchAsync(url, httpContent);
                string dataResponse = await response.Content.ReadAsStringAsync();

                var httpRequest = new HttpRequest(methodArgs.MethodType, url, "application/json", dataRequest);
                CallMiddlewares(response, httpRequest);

                if (response.IsSuccessStatusCode)
                    return new RestResponse(response);

                return new RestResponse(response);
            }


            if (methodArgs.MethodType == MethodType.DELETE)
            {
                HttpResponseMessage response = await HttpClient.DeleteAsync(url);
                string dataResponse = await response.Content.ReadAsStringAsync();

                var httpRequest = new HttpRequest(methodArgs.MethodType, url, null, null);
                CallMiddlewares(response, httpRequest);

                if (response.IsSuccessStatusCode)
                    return new RestResponse(response);

                return new RestResponse(response);
            }

            throw new Exception();
        }

        private void CallMiddlewares(HttpResponseMessage httpResponse, HttpRequest httpRequest) {
            if (Middlewares is null) return;
            foreach (Middleware middleware in Middlewares)
                middleware.Invoke(HttpClient, httpResponse, httpRequest);
        }

        protected internal async Task CallVoid(string method, params object[] args)
        {
            LoadVariables();

            (MethodArgs methodArgs, string url) = GetUrl(method, args);


            foreach (var header in Headers)
            {
                HttpClient.DefaultRequestHeaders.Remove(header.Key);
                HttpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
            }

            if (methodArgs.RestMethod is FormUrlEncodedMapping)
            {
                FormUrlEncodedContent form = new(
                    methodArgs.InFields.Where(e => e.Item1 is InPart).Select(e => new KeyValuePair<string, string>(e.Item1.Name!, (string)e.Item2!))
                 );

                HttpResponseMessage response = await HttpClient.PostAsync(url, form);
                string dataResponse = await response.Content.ReadAsStringAsync();

                var httpRequest = new HttpRequest(methodArgs.MethodType, url, "not_implement", null);
                CallMiddlewares(response, httpRequest);

                if (!response.IsSuccessStatusCode)
                    throw new RestException(response.StatusCode, dataResponse);

                return;
            }

            if (methodArgs.MethodType == MethodType.GET)
            {
                HttpResponseMessage response = await HttpClient.GetAsync(url);
                string dataResponse = await response.Content.ReadAsStringAsync();

                var httpRequest = new HttpRequest(methodArgs.MethodType, url, null, null);
                CallMiddlewares(response, httpRequest);

                if (!response.IsSuccessStatusCode)
                    throw new RestException(response.StatusCode, dataResponse);
            }

            if (methodArgs.MethodType == MethodType.POST)
            {
                string dataRequest = methodArgs.ContainsBody ?
                    JsonConvert.SerializeObject(args[methodArgs.BodyIndex]) : string.Empty;

                HttpContent? httpContent = methodArgs.ContainsBody ?
                    new StringContent(dataRequest, Encoding.UTF8, "application/json") : null;

                HttpResponseMessage response = await HttpClient.PostAsync(url, httpContent);
                string dataResponse = await response.Content.ReadAsStringAsync();

                RestAuthentication? restAuthentication = methodArgs.MethodInfo.GetCustomAttribute<RestAuthentication>();

                if (restAuthentication is not null && response.IsSuccessStatusCode)
                {
                    LoadRestAuthentication(restAuthentication, dataResponse);
                }

                var httpRequest = new HttpRequest(methodArgs.MethodType, url, "application/json", dataRequest);
                CallMiddlewares(response, httpRequest);

                if (!response.IsSuccessStatusCode)
                    throw new RestException(response.StatusCode, dataResponse);

            }

            if (methodArgs.MethodType == MethodType.PUT)
            {
                string dataRequest = JsonConvert.SerializeObject(args[methodArgs.BodyIndex]);
                var httpContent = new StringContent(dataRequest, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await HttpClient.PutAsync(url, httpContent);
                string dataResponse = await response.Content.ReadAsStringAsync();

                var httpRequest = new HttpRequest(methodArgs.MethodType, url, "application/json", dataRequest);
                CallMiddlewares(response, httpRequest);

                if (!response.IsSuccessStatusCode)
                    throw new RestException(response.StatusCode, dataResponse);

            }

            if (methodArgs.MethodType == MethodType.PATCH)
            {
                string dataRequest = JsonConvert.SerializeObject(args[methodArgs.BodyIndex]);
                var httpContent = new StringContent(dataRequest, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await HttpClient.PatchAsync(url, httpContent);
                string dataResponse = await response.Content.ReadAsStringAsync();

                var httpRequest = new HttpRequest(methodArgs.MethodType, url, "application/json", dataRequest);
                CallMiddlewares(response, httpRequest);

                if (!response.IsSuccessStatusCode)
                    throw new RestException(response.StatusCode, dataResponse);
            }

            if (methodArgs.MethodType == MethodType.DELETE)
            {
                HttpResponseMessage response = await HttpClient.DeleteAsync(url);
                string dataResponse = await response.Content.ReadAsStringAsync();

                var httpRequest = new HttpRequest(methodArgs.MethodType, url, null, null);
                CallMiddlewares(response, httpRequest);

                if (!response.IsSuccessStatusCode)
                    throw new RestException(response.StatusCode, dataResponse);
            }
        }

        public void AddHeader(string name, string value) => Headers.AddOrSet(name, value);

        private static void LoadRestAuthentication(RestAuthentication restAuthentication, string json)
        {
            JObject body = JObject.Parse(json);

            if (restAuthentication.AuthenticationType == AuthenticationType.JWT && restAuthentication.AuthenticationMode == AuthenticationMode.BEARER)
            {
                string? token = body.SelectToken(restAuthentication.ObjectPath)?.Value<string>();
                if (token is not null)
                    Env.AddOrSet("opendev.openrestclient.jwt.bearer.token", token);
            }
        }

        private (MethodArgs, string) GetUrl(string method, params object[] args)
        {
            method += args.Length;
            MethodArgs methodArgs = Methods[method][args.Length];
            args = GetArgs(ref methodArgs, args);
            string url = string.Format("{0}/{1}", Host, methodArgs.Url);

            return (methodArgs, $"{string.Format(url, args)}?{methodArgs.Query}");
        }

        private static object[] GetArgs(ref MethodArgs method, object[] args)
        {
            List<object> result = [];
            method.Query = HttpUtility.ParseQueryString(string.Empty);

            for (int i = 0; i < method.InFields.Count; i++)
            {
                Tuple<InField, object?> keyValue = method.InFields[i];
                method.InFields[i] = new Tuple<InField, object?>(keyValue.Item1, args[i]);

                if (keyValue.Item1 is InParam)
                {
                    string? data = args[i]?.ToString();
                    if (string.IsNullOrWhiteSpace(data)) continue;

                    method.Query[keyValue.Item1.Name] = args[i]?.ToString();
                    continue;
                }

                if (keyValue.Item1 is InFormStream || keyValue.Item1 is InPart || keyValue.Item1 is InFormFile || keyValue.Item1 is InBody)
                {
                    continue;
                }

                if (keyValue.Item1 is InJoin)
                {
                    if (!keyValue.Item1!.GetType().IsArray) throw new ArgumentException();

                    InJoin? inJoin = keyValue.Item1 as InJoin;
                    Array? elements = args[i] as Array;
                    List<object> values = new();
                    for (int index = 0; index < elements?.Length; index++)
                        values.Add(elements.GetValue(index)!);
                    args[i] = string.Join(inJoin?.Separator, values);
                }

                result.Add(args[i]);
            }

            return [.. result];
        }

        private static string? GetHost(Type type)
        {
            return Assembly.GetAssembly(type)?.GetCustomAttribute<RestHost>()?.Host;
        }

        public static T BuildApp<T>(List<Middleware> middlewares) where T : class
        {
            dynamic loginService = DispatchProxy.Create<T, DynamicProxy<T>>();
            var restApp = new RestApp(typeof(T))
            {
                Middlewares = middlewares,
            };
            ((DynamicProxy<T>)loginService).SetRestApp(restApp);
            return loginService;
        }

        public static T BuildApp<T>(HttpClient httpClient, List<Middleware> middlewares) where T : class
        {
            dynamic loginService = DispatchProxy.Create<T, DynamicProxy<T>>();
            var restApp = new RestApp(typeof(T))
            {
                Middlewares = middlewares,
                HttpClient = httpClient

            };
            ((DynamicProxy<T>)loginService).SetRestApp(restApp);
            return loginService;
        }

        private static void AddUrls(RestApp app, string? route, Type type)
        {
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                string methodName = method.Name;
                MethodArgs url = new();
                ParameterInfo[] parameters = method.GetParameters();

                Dictionary<int, MethodArgs> urls = [];
                url.InFields = [];
                RestMethod? restMethod = method.GetCustomAttribute<RestMethod>();

                if (restMethod is null) continue;

                url.RestMethod = restMethod;
                url.Url = string.Join("/", new[] { route ?? "", restMethod.Route ?? "" }.Where(e => !string.IsNullOrWhiteSpace(e)));
                url.MethodInfo = method;

                url.MethodType = restMethod!.Method;
                int parametersCount = 0;

                for (int i = 0; i < parameters.Length; i++)
                {
                    InField? inField = parameters[i].GetCustomAttribute<InField>();
                    if (inField is not null)
                    {
                        if (string.IsNullOrWhiteSpace(inField.Name))
                            inField.Name = parameters[i].Name;

                        url.InFields.Add(new Tuple<InField, object?>(inField, null));

                        parametersCount++;
                        if (inField is InBody)
                        {
                            if (url.ContainsBody) throw new ArgumentException("The method cannot have more than one body.");

                            url.BodyIndex = i;
                            url.ContainsBody = true;
                            continue;
                        }

                        if (inField is InPart || inField is InFormFile || inField is InFormStream || inField is InParam)
                        {
                            continue;
                        }

                        url.Url = $"{url.Url}/{{{i}}}";

                    }
                }

                app?.Methods.Add(methodName + parametersCount, urls);
                urls.Add(parametersCount, url);
            }
        }

        private static string? GetRoute(Type type)
        {
            return type.GetCustomAttribute<RestController>()?.Route;
        }

        private struct MethodArgs
        {
            public string? Url { get; set; }
            public bool ContainsBody { get; set; }
            public int BodyIndex { get; set; }
            public MethodType MethodType { get; set; }
            public NameValueCollection Query { get; set; }

            public MethodInfo MethodInfo { get; set; }
            public RestMethod RestMethod { get; set; }

            public List<Tuple<InField, object?>> InFields { get; set; }
        }
    }
}
