using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenRestClient.ApiController.Exceptions;
using OpenRestClient.Attributes;
using OpenRestController.Enums;
using System.Reflection;
using System.Text;

namespace OpenRestClient
{
    public class RestApp
    {
        private string? Host { get; set; }
        private Dictionary<string, Dictionary<int, MethodArgs>> Methods { get; set; }
        private Dictionary<string, string> Headers { get; set; }

        internal static Dictionary<string, string> Env = new();

        protected RestApp(string host, Type type)
        {
            Host = host;
            Headers = new Dictionary<string, string>();
            Methods = new Dictionary<string, Dictionary<int, MethodArgs>>();
            AddUrls(this, GetRoute(type), type);
            LoadVariables();
        }

        protected RestApp(Type type)
        {
            Host = GetHost(type);
            Headers = new Dictionary<string, string>();
            Methods = new Dictionary<string, Dictionary<int, MethodArgs>>();
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

                if (!response.IsSuccessStatusCode)
                    throw new RestException(response.StatusCode, dataResponse);

#if OPENRESTCLIENT_DEBUG
                PrintDEBUG(methodArgs.MethodType, url, string.Empty, dataResponse);
#endif

                return dataResponse;
            }

            if (methodArgs.MethodType == MethodType.POST)
            {
                string dataRequest = JsonConvert.SerializeObject(args[0]);

                var httpContent = new StringContent(dataRequest, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(url, httpContent);

                string dataResponse = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    throw new RestException(response.StatusCode, dataResponse);

#if OPENRESTCLIENT_DEBUG
                PrintDEBUG(methodArgs.MethodType, url, dataRequest, dataResponse);
#endif

                return dataResponse;
            }

            if (methodArgs.MethodType == MethodType.PATCH)
            {
                string dataRequerst = JsonConvert.SerializeObject(args[0]);

                var httpContent = new StringContent(dataRequerst, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PatchAsync(url, httpContent);

                string dataResponse = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    throw new RestException(response.StatusCode, dataResponse);

#if OPENRESTCLIENT_DEBUG
                PrintDEBUG(methodArgs.MethodType, url, dataRequerst, dataResponse);
#endif

                return dataResponse;
            }

            if (methodArgs.MethodType == MethodType.PUT)
            {
                string dataRequest = JsonConvert.SerializeObject(args[0]);

                var httpContent = new StringContent(dataRequest, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PutAsync(url, httpContent);
                string dataResponse = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    throw new RestException(response.StatusCode, dataResponse);

#if OPENRESTCLIENT_DEBUG
                PrintDEBUG(methodArgs.MethodType, url, dataRequest, dataResponse);
#endif

                return dataResponse;
            }

            if (methodArgs.MethodType == MethodType.DELETE)
            {
                HttpResponseMessage response = await client.DeleteAsync(url);

                string dataResponse = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    throw new RestException(response.StatusCode, dataResponse);

#if OPENRESTCLIENT_DEBUG
                PrintDEBUG(methodArgs.MethodType, url, dataResponse, string.Empty);
#endif

                return await response.Content.ReadAsStringAsync();
            }
            return null;
        }

        protected async Task<T[]?> CallArray<T>(string method, params object[] args)
        {
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

#if OPENRESTCLIENT_DEBUG
                PrintDEBUG(methodArgs.MethodType, url, dataResponse, string.Empty);
#endif

                return JsonConvert.DeserializeObject<T[]>(dataResponse);
            }

            if (methodArgs.MethodType == MethodType.POST)
            {
                string dataRequest = JsonConvert.SerializeObject(args[0]);
                var httpContent = new StringContent(dataRequest, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(url, httpContent);


                if (!response.IsSuccessStatusCode)
                    throw new RestException(response.StatusCode, dataRequest);

#if OPENRESTCLIENT_DEBUG
                PrintDEBUG(methodArgs.MethodType, url, dataRequest, string.Empty);
#endif

                return JsonConvert.DeserializeObject<T[]>(await response.Content.ReadAsStringAsync());
            }

            if (methodArgs.MethodType == MethodType.PUT)
            {
                string dataResponse = JsonConvert.SerializeObject(args[0]);
                var httpContent = new StringContent(dataResponse, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PutAsync(url, httpContent);
                string dataRequest = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    throw new RestException(response.StatusCode, dataResponse);

#if OPENRESTCLIENT_DEBUG
                PrintDEBUG(methodArgs.MethodType, url, dataRequest, dataResponse);
#endif

                return JsonConvert.DeserializeObject<T[]>(dataResponse);
            }

            throw new Exception();
        }

        protected internal async Task<T?> Call<T>(string method, params object[] args)
        {
            LoadVariables();

            (MethodArgs methodArgs, string url) = GetUrl(method, args);

            using HttpClient client = new();

            foreach (var header in Headers)
                client.DefaultRequestHeaders.Add(header.Key, header.Value);

            if (methodArgs.MethodType == MethodType.GET)
            {
                HttpResponseMessage response = await client.GetAsync(url);
                string dataRequest = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    throw new RestException(response.StatusCode, dataRequest);


#if OPENRESTCLIENT_DEBUG
                PrintDEBUG(methodArgs.MethodType, url, dataRequest, string.Empty);
#endif

                return JsonConvert.DeserializeObject<T>(dataRequest);
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

#if OPENRESTCLIENT_DEBUG
                PrintDEBUG(methodArgs.MethodType, url, dataRequest, dataResponse);
#endif

                return JsonConvert.DeserializeObject<T>(dataResponse);
            }

            if (methodArgs.MethodType == MethodType.PUT)
            {
                string dataRequest = JsonConvert.SerializeObject(args[0]);
                var httpContent = new StringContent(dataRequest, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PutAsync(url, httpContent);
                string dataResponse = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    throw new RestException(response.StatusCode, dataRequest);

#if OPENRESTCLIENT_DEBUG
                PrintDEBUG(methodArgs.MethodType, url, dataRequest, dataResponse);
#endif

                return JsonConvert.DeserializeObject<T>(dataResponse);
            }

            if (methodArgs.MethodType == MethodType.PATCH)
            {
                string dataRequest = JsonConvert.SerializeObject(args[0]);
                var httpContent = new StringContent(dataRequest, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PatchAsync(url, httpContent);
                string dataResonse = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    throw new RestException(response.StatusCode, dataRequest);

#if OPENRESTCLIENT_DEBUG
                PrintDEBUG(methodArgs.MethodType, url, dataRequest, dataResonse);
#endif

                return JsonConvert.DeserializeObject<T>(dataResonse);
            }

            if (methodArgs.MethodType == MethodType.DELETE)
            {
                HttpResponseMessage response = await client.DeleteAsync(url);
                string dataResponse = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    throw new RestException(response.StatusCode, dataResponse);

#if OPENRESTCLIENT_DEBUG
                PrintDEBUG(methodArgs.MethodType, url, string.Empty, dataResponse);
#endif

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

#if OPENRESTCLIENT_DEBUG
                PrintDEBUG(methodArgs.MethodType, url, string.Empty, dataResponse);
#endif

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
                    LoadRestAuthentication(restAuthentication, dataRequest);
                }

                if (!response.IsSuccessStatusCode)
                    throw new RestException(response.StatusCode, dataRequest);

#if OPENRESTCLIENT_DEBUG
                PrintDEBUG(methodArgs.MethodType, url, dataRequest, dataResponse);
#endif

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

#if OPENRESTCLIENT_DEBUG
                PrintDEBUG(methodArgs.MethodType, url, dataRequest, dataResponse);
#endif

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

#if OPENRESTCLIENT_DEBUG
                PrintDEBUG(methodArgs.MethodType, url, dataRequest, dataResponse);
#endif

                return JsonConvert.DeserializeObject(dataResponse, type);
            }


            if (methodArgs.MethodType == MethodType.DELETE)
            {
                HttpResponseMessage response = await client.DeleteAsync(url);
                string dataResponse = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    throw new RestException(response.StatusCode, dataResponse);

#if OPENRESTCLIENT_DEBUG
                PrintDEBUG(methodArgs.MethodType, url, string.Empty, dataResponse);
#endif

                return JsonConvert.DeserializeObject(dataResponse, type);
            }

            throw new Exception();
        }

        protected async Task<RestResponse<T?>> CallResponse<T>(string method, params object[] args)
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

                if (response.IsSuccessStatusCode)
                {
                    T? value = JsonConvert.DeserializeObject<T>(dataResponse);
                    return new RestResponse<T?>(value, response);
                }

#if OPENRESTCLIENT_DEBUG
                PrintDEBUG(methodArgs.MethodType, url, string.Empty, dataResponse);
#endif

                return new RestResponse<T?>(response);
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

                if (response.IsSuccessStatusCode)
                {
                    T? value = JsonConvert.DeserializeObject<T>(dataResponse);
                    return new RestResponse<T?>(value, response);
                }

#if OPENRESTCLIENT_DEBUG
                PrintDEBUG(methodArgs.MethodType, url, dataRequest, dataResponse);
#endif

                return new RestResponse<T?>(response);
            }

            if (methodArgs.MethodType == MethodType.PUT)
            {
                string dataRequest = JsonConvert.SerializeObject(args[0]);
                var httpContent = new StringContent(dataRequest, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PutAsync(url, httpContent);
                string dataResponse = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    T? value = JsonConvert.DeserializeObject<T>(dataResponse);
                    return new RestResponse<T?>(value, response);
                }

#if OPENRESTCLIENT_DEBUG
                PrintDEBUG(methodArgs.MethodType, url, dataRequest, dataResponse);
#endif

                return new RestResponse<T?>(response);
            }

            if (methodArgs.MethodType == MethodType.PATCH)
            {
                string dataRequest = JsonConvert.SerializeObject(args[0]);
                var httpContent = new StringContent(dataRequest, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PatchAsync(url, httpContent);
                string dataResponse = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    T? value = JsonConvert.DeserializeObject<T>(dataResponse);
                    return new RestResponse<T?>(value, response);
                }

#if OPENRESTCLIENT_DEBUG
                PrintDEBUG(methodArgs.MethodType, url, dataRequest, dataResponse);
#endif

                return new RestResponse<T?>(response);
            }


            if (methodArgs.MethodType == MethodType.DELETE)
            {
                HttpResponseMessage response = await client.DeleteAsync(url);
                string dataResponse = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    T? value = JsonConvert.DeserializeObject<T>(dataResponse);
                    return new RestResponse<T?>(value, response);
                }

#if OPENRESTCLIENT_DEBUG
                PrintDEBUG(methodArgs.MethodType, url, string.Empty, dataResponse);
#endif

                return new RestResponse<T?>(response);
            }

            throw new Exception();
        }

        protected internal async Task Call(string method, params object[] args)
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

#if OPENRESTCLIENT_DEBUG
                PrintDEBUG(methodArgs.MethodType, url, string.Empty, dataResponse);
#endif

                if (!response.IsSuccessStatusCode)
                    throw new RestException(response.StatusCode, dataResponse);
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

#if OPENRESTCLIENT_DEBUG
                PrintDEBUG(methodArgs.MethodType, url, dataRequest, dataResponse);
#endif
            }

            if (methodArgs.MethodType == MethodType.PUT)
            {
                string dataRequest = JsonConvert.SerializeObject(args[0]);
                var httpContent = new StringContent(dataRequest, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PutAsync(url, httpContent);
                string dataResponse = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    throw new RestException(response.StatusCode, dataResponse);

#if OPENRESTCLIENT_DEBUG
                PrintDEBUG(methodArgs.MethodType, url, dataRequest, dataResponse);
#endif
            }

            if (methodArgs.MethodType == MethodType.PATCH)
            {
                string dataRequest = JsonConvert.SerializeObject(args[0]);
                var httpContent = new StringContent(dataRequest, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PatchAsync(url, httpContent);
                string dataResponse = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    throw new RestException(response.StatusCode, dataResponse);

#if OPENRESTCLIENT_DEBUG
                PrintDEBUG(methodArgs.MethodType, url, dataRequest, dataResponse);
#endif
            }

            if (methodArgs.MethodType == MethodType.DELETE)
            {
                HttpResponseMessage response = await client.DeleteAsync(url);
                string dataResponse = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    throw new RestException(response.StatusCode, dataResponse);

#if OPENRESTCLIENT_DEBUG
                PrintDEBUG(methodArgs.MethodType, url, dataResponse, dataResponse);
#endif
            }
        }

#if OPENRESTCLIENT_DEBUG

        private void PrintDEBUG(MethodType methodType, string ur, string jsonrequest, string jsonresponse)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(methodType.ToString());
            Console.Write(" ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(ur);
            Console.WriteLine();



#if OPENRESTCLIENT_DEBUG_JSON

            if (!string.IsNullOrEmpty(jsonrequest))
            {
                Console.WriteLine($"REQUEST");
                ParseJson(jsonrequest);
            }

            if (!string.IsNullOrEmpty(jsonresponse))
            {
                Console.WriteLine($"RESPONSE");
                ParseJson(jsonresponse);
            }
#endif

        }

        private static void ParseJson(string json)
        {
            json = JsonPrettify(json);
            StringReader reader = new StringReader(json);
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                ParseLine(line);
                Console.WriteLine();
            }
        }

        public static string JsonPrettify(string json)
        {
            using (var stringReader = new StringReader(json))
            using (var stringWriter = new StringWriter())
            {
                var jsonReader = new JsonTextReader(stringReader);
                var jsonWriter = new JsonTextWriter(stringWriter) { Formatting = Formatting.Indented };
                jsonWriter.WriteToken(jsonReader);
                return stringWriter.ToString();
            }
        }

        private static void ParseLine(string line)
        {
            bool alternarColor = true;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '{' || c == '}' || c == ':' || c == ' ')
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(c.ToString());
                }

                if (c == '[' || c == ']')
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(c.ToString());
                }

                if (c == ',')
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write(c.ToString());
                    alternarColor = false;
                }

                if (c == '"')
                {
                    if (alternarColor)
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.Write(c.ToString());
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        Console.Write(c.ToString());
                    }

                    bool b = false;
                    while (true)
                    {
                        i++;
                        c = line[i];
                        if (c == '\\')
                            b = true;

                        if (c == '"')
                        {
                            if (!b)
                                break;
                            else
                                b = false;
                        }

                        if (alternarColor)
                        {
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.Write(c.ToString());
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.DarkYellow;
                            Console.Write(c.ToString());
                        }
                    }

                    if (alternarColor)
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.Write(c.ToString());
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        Console.Write(c.ToString());
                    }
                    alternarColor = !alternarColor;
                }

                if (char.IsNumber(c))
                {
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.Write(c.ToString());
                }

                if (char.IsLetter(c))
                {
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.Write(c.ToString());
                }
            }
        }

#endif

        public void AddHeader(string name, string value)
        {
            Headers.AddOrSet(name, value);
        }

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
            args = GetArgs(methodArgs, args);
            string url = string.Format("{0}/{1}", Host, methodArgs.Url);
            return (methodArgs, string.Format(url, args));
        }

        private static object[] GetArgs(MethodArgs method, object[] args)
        {
            for (int i = 0; i < method.InFields.Count; i++)
            {

                if (method.InFields[i] is InJoin)
                {
                    if (!args[i].GetType().IsArray) throw new ArgumentException();

                    InJoin? inJoin = method.InFields[i] as InJoin;
                    Array? elements = args[i] as Array;
                    List<object> values = new List<object>();
                    for (int index = 0; index < elements?.Length; index++)
                        values.Add(elements.GetValue(index)!);
                    args[i] = string.Join(inJoin?.Separator, values);
                }
            }

            return args;
        }

        private static string? GetHost(Type type)
        {
            return Assembly.GetAssembly(type)?.GetCustomAttribute<RestHost>()?.Host;
        }

        public static T GetRestApp<T>(string host) where T : RestApp, new()
        {
            T app = new()
            {
                Host = host,
                Methods = new Dictionary<string, Dictionary<int, MethodArgs>>()
            };

            Type type = typeof(T);
            AddUrls(app, GetRoute(type), typeof(T));
            return app;
        }

        public static T BuildApp<T>() where T : class
        {
            dynamic loginService = DispatchProxy.Create<T, DynamicProxy<T>>();
            RestApp restApp = new RestApp(typeof(T));
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

                Dictionary<int, MethodArgs> urls = new();
                url.InFields = new List<InField>();
                RestMethod? restMethod = method.GetCustomAttribute<RestMethod>();

                if (restMethod is null) continue;

                url.Url = string.Join("/", new[] { route ?? "", restMethod.Route ?? "" }.Where(e => !string.IsNullOrWhiteSpace(e)));
                url.MethodInfo = method;

                url.MethodType = restMethod!.Method;
                int parametersCount = 0;

                for (int i = 0; i < parameters.Length; i++)
                {
                    InField? inField = parameters[i].GetCustomAttribute<InField>();
                    if (inField is not null)
                    {
                        parametersCount++;
                        if (inField is InBody)
                        {
                            if (url.ContainsBody) throw new ArgumentException("The method cannot have more than one body.");

                            url.BodyIndex = i;
                            url.ContainsBody = true;
                            continue;
                        }

                        url.Url = $"{url.Url}/{{{i}}}";
                        url.InFields.Add(inField);

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

            public MethodInfo MethodInfo { get; set; }

            public List<InField> InFields { get; set; }
        }
    }
}
