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

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            if (targetMethod is null)
                throw new Exception("Target method cannot be null.");

            RestReturnType? returnTypeAttribute = targetMethod.GetCustomAttribute<RestReturnType>();
            Type returnType = returnTypeAttribute?.ReturnType ?? targetMethod.ReturnType;

            if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
            {
                Type taskResultType = returnType.GetGenericArguments()[0];

                var callMethod = typeof(RestApp).GetMethod("Call", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.MakeGenericMethod(taskResultType);
                return callMethod?.Invoke(RestApp, [targetMethod.Name, args!]);
            }

            return RestApp?.CallVoid(targetMethod.Name, args!);
        }
    }
}
