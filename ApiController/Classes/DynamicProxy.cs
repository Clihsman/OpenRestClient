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
        /*
        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            if (targetMethod is null) throw new Exception();
            RestReturnType? returnType = targetMethod.GetCustomAttribute<RestReturnType>();

            if (returnType == null) {
                RestApp!.Call(targetMethod!.Name, returnType is null ? targetMethod.ReturnType : returnType.ReturnType, args!);
                return null;
            }

            object result;
            RestApp!.CallObject(targetMethod!.Name, returnType is null ? targetMethod.ReturnType : returnType.ReturnType, out result, args!);
            return result;
        }
        */

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            if (targetMethod is null)
                throw new Exception("Target method cannot be null.");

            // Obtener el tipo de retorno del método
            RestReturnType? returnTypeAttribute = targetMethod.GetCustomAttribute<RestReturnType>();
            Type returnType = returnTypeAttribute?.ReturnType ?? targetMethod.ReturnType;

            // Si el tipo de retorno es Task<T>
            if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
            {
                // Obtener el tipo genérico T de Task<T>
                Type taskResultType = returnType.GetGenericArguments()[0];

                var callMethod = typeof(RestApp).GetMethod("Call", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.MakeGenericMethod(taskResultType);
                return callMethod?.Invoke(RestApp, new object[] { targetMethod.Name, args! });
            }

            // Si el tipo de retorno no es Task<T>, manejar directamente
            return RestApp?.CallVoid(targetMethod.Name, args!);
        }

    }
}
