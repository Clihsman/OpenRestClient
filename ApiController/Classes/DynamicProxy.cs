using OpenRestClient.Attributes;
using System.Reflection;

namespace OpenRestClient
{
    public class DynamicProxy<T> : DispatchProxy where T : class
    {
        private RestApp? RestApp;

        public void SetRestApp(RestApp? restApp)
        {
            RestApp = restApp;
        }

        protected override Task<object?> Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            if (targetMethod is null) throw new Exception();
            RestReturnType? returnType = targetMethod.GetCustomAttribute<RestReturnType>();   
            return RestApp!.Call(targetMethod!.Name, returnType is null ? targetMethod.ReturnType : returnType.ReturnType, args!);
        }
    }
}
