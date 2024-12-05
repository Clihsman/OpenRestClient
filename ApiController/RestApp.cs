using Newtonsoft.Json;
using OpenRestClient.Attributes;
using OpenRestController.Attributes;
using OpenRestController.Enums;
using System.Net;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Text;

namespace OpenRestController
{
    public abstract class RestApp
    {
        private string? Host { get; set; }
        private Dictionary<string, Dictionary<int, MethodArgs>> Methods { get; set; }
        private Dictionary<string, string> Headers { get; set; }

        protected RestApp(string host, Type type)
        {
            Host = host;
            Headers = new Dictionary<string, string>();
            Methods = new Dictionary<string, Dictionary<int, MethodArgs>>();
            AddUrls(this, GetRoute(type), type);
        }

        protected RestApp(Type type)
        {
            Host = GetHost(type);
            Headers = new Dictionary<string, string>();
            Methods = new Dictionary<string, Dictionary<int, MethodArgs>>();
            AddUrls(this, GetRoute(type), type);
        }

        protected async Task<string?> CallString(string method, params object[] args)
        {
            (string methodType, string url) = GetUrl(method, args);
            using (HttpClient client = new HttpClient())
            {
                foreach (var header in Headers)
                    client.DefaultRequestHeaders.Add(header.Key, header.Value);

                if (methodType == "get")
                {
                    HttpResponseMessage response = await client.GetAsync(url);
                    return await response.Content.ReadAsStringAsync();
                }
                if (methodType == "post")
                {
                    string json = JsonConvert.SerializeObject(args[0]);
                    var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await client.PostAsync(url, httpContent);
                    return await response.Content.ReadAsStringAsync();
                }

                if (methodType == "put")
                {
                    string json = JsonConvert.SerializeObject(args[0]);
                    var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await client.PutAsync(url, httpContent);
                    return await response.Content.ReadAsStringAsync();
                }
                if (methodType == "delete")
                {
                    HttpResponseMessage response = await client.DeleteAsync(url);
                    return await response.Content.ReadAsStringAsync();
                }
            }
            return null;
        }

        protected async Task<T[]?> CallArray<T>(string method, params object[] args)
        {
            (string methodType, string url) = GetUrl(method, args);
            Console.WriteLine(url);
            using (HttpClient client = new())
            {
                foreach (var header in Headers)
                    client.DefaultRequestHeaders.Add(header.Key, header.Value);

                if (methodType == "get")
                {
                    HttpResponseMessage response = await client.GetAsync(url);
                    string json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<T[]>(json);
                }
                if (methodType == "post")
                {
                    string json = JsonConvert.SerializeObject(args[0]);
                    var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await client.PostAsync(url, httpContent);
                    return JsonConvert.DeserializeObject<T[]>(json);
                }

                if (methodType == "put")
                {
                    string json = JsonConvert.SerializeObject(args[0]);
                    var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await client.PutAsync(url, httpContent);
                    json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<T[]>(json);
                }
            }
            return null;
        }

        protected async Task<T?> Call<T>(string method, params object[] args)
        {
            (string methodType, string url) = GetUrl(method, args);

            using (HttpClient client = new())
            {
                foreach (var header in Headers)
                    client.DefaultRequestHeaders.Add(header.Key, header.Value);

                if (methodType == "get")
                {
                    HttpResponseMessage response = await client.GetAsync(url);
                    string json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<T>(json);
                }
                if (methodType == "post")
                {
                    string json = JsonConvert.SerializeObject(args[0]);
                    var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await client.PostAsync(url, httpContent);
                    json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<T>(json);
                }

                if (methodType == "put")
                {
                    string json = JsonConvert.SerializeObject(args[0]);
                    var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await client.PutAsync(url, httpContent);
                    json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<T>(json);
                }

                if (methodType == "delete")
                {
                    HttpResponseMessage response = await client.DeleteAsync(url);
                    string json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<T>(json);
                }
            }
            return default;
        }

        public void AddHeader(string name, string value)
        {
            Headers.Add(name, value);
        }

        private (string, string) GetUrl(string method, params object[] args)
        {
            method += args.Length;
            MethodArgs methodArgs = Methods[method][args.Length];
            args = GetArgs(methodArgs, args);
            string url = string.Format("{0}/{1}", Host, methodArgs.Url);
            return (GetMethodName(methodArgs.MethodType), string.Format(url, args));
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
                        values.Add(elements?.GetValue(index));
                    args[i] = string.Join(inJoin?.Separator, values);
                }
            }

            return args;
        }

        private static string? GetHost(Type type)
        {
            return Assembly.GetAssembly(type)?.GetCustomAttribute<RestHost>()?.Host;
        }

        private static string GetMethodName(MethodType methodType)
        {
            string methodName = methodType == MethodType.GET ? "get" : "";
            methodName = methodType == MethodType.POST ? "post" : methodName;
            methodName = methodType == MethodType.PUT ? "put" : methodName;
            methodName = methodType == MethodType.DELETE ? "delete" : methodName;
            methodName = methodType == MethodType.OPTIONS ? "options" : methodName;
            return methodName;
        }

        public static T GetRestApp<T>(string host) where T : RestApp, new()
        {
            T app = new T
            {
                Host = host,
                Methods = new Dictionary<string, Dictionary<int, MethodArgs>>()
            };

            Type type = typeof(T);
            AddUrls(app, GetRoute(type), typeof(T));

            return app;
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

                url.Url = route;
                //int index = restMethod?.Method == MethodType.POST || restMethod?.Method == MethodType.PUT ? 1 : 0;


                url.MethodType = restMethod!.Method;
                url.Url = $"{url.Url}/{restMethod.Route}";

                for (int i = 0; i < parameters.Length; i++)
                {
                    InField? inField = parameters[i].GetCustomAttribute<InField>();
                    if (inField is not null)
                    {
                        url.Url = $"{url.Url}/{{{i}}}";
                        url.InFields.Add(inField);
                    }         
                }
                app?.Methods.Add(methodName + parameters.Length, urls);
                urls.Add(parameters.Length, url);
            }
        }

        private static string? GetRoute(Type type)
        {
            return type.GetCustomAttribute<RestController>()?.Route;
        }

        private struct MethodArgs
        {
            public string? Url { get; set; }
            public MethodType MethodType { get; set; }

            public List<InField> InFields { get; set; }
        }
    }
}
