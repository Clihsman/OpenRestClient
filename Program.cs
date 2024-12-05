using OpenRestClient.Attributes;
using OpenRestController.Attributes;
using OpenRestController;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Dynamic;
using System.Reflection;

namespace OpenRestClient
{

    /*
    public record UserRequest(string Token, int UserId);

    [RestController("auth")]
    public class LoginService : RestApp
    {
        public LoginService() : base(typeof(LoginService)) { }

        [PostMapping("signin/pepito/juan")]
        [RestAuthentication(AuthenticationType.JWT, AuthenticationMode.BEARER, "token")]
        public Task<UserRequest?> Signin([InBody] string email, string password)
            => Call<UserRequest>(nameof(Signin), new { email, password });
    }

    */
    /*
    [RestController("auth")]

    public record UserRequest(string Token, int UserId);

    [PostMapping("signin")]
    [RestAuthentication(AuthenticationType.JWT, AuthenticationMode.BEARER, "token")]
    */
    public interface LoginService
    {
        [PostMapping("signin")]
        [RestAuthentication(AuthenticationType.JWT, AuthenticationMode.BEARER, "token")]
        void Signin();
    }

    public class Dnd : DynamicObject
    {
        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            Console.WriteLine($"Método llamado: {binder.Name}");
            Console.WriteLine($"Argumentos: {string.Join(", ", args)}");
            result = null;
            return true;
        }
    }

    public class DynamicProxy<T> : DispatchProxy where T : class
    {
        private DynamicObject _dynamicObject;

        public void SetDynamicObject(DynamicObject dynamicObject)
        {
            _dynamicObject = dynamicObject;
        }

        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            string methodName = targetMethod.Name;

            PostMapping mapping = targetMethod.GetCustomAttribute<PostMapping>();

            Console.WriteLine(mapping.Route);

            Console.WriteLine(methodName);
            return null;
            // Llama al método dinámico usando Reflection
            /*
            if (_dynamicObject.TryInvokeMember(new InvokeMemberBinderImpl(methodName), args, out var result))
            {
                return result;
            }

            throw new MissingMethodException($"El método {methodName} no fue encontrado en el objeto dinámico.");*/
        }
        /*
        private class InvokeMemberBinderImpl : InvokeMemberBinder
        {
            public InvokeMemberBinderImpl(string name) : base(name, ignoreCase: false) { }

            public override DynamicMetaObject FallbackInvoke(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject? errorSuggestion)
            {
                throw new NotImplementedException();
            }

            public override DynamicMetaObject FallbackInvokeMember(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject? errorSuggestion)
            {
                throw new NotImplementedException();
            }

            public override System.Collections.Generic.IEnumerable<string> GetDynamicMemberNames()
            {
                return Array.Empty<string>();
            }
        }
        */
    }

    class Program
    {
        public static async Task Main()
        {
            /*
            // Configura el objeto dinámico
            var dynamicObject = new Dnd();

            // Crea el proxy
            var loginService = DispatchProxy.Create<LoginService, DynamicProxy<LoginService>>();
            ((DynamicProxy<LoginService>)loginService).SetDynamicObject(dynamicObject);

            // Llama al método de la interfaz
            loginService.Signin(); // Llama dinámicamente a "Signin" en la clase Dnd
            */

            LoginService loginService = Bean<LoginService>();
            loginService.Signin();
        }

        private static T Bean<T>() where T : class
        {
            var dynamicObject = new Dnd();

            dynamic loginService =  DispatchProxy.Create<T, DynamicProxy<T>>();
            ((DynamicProxy<T>)loginService).SetDynamicObject(dynamicObject);

            return loginService;
        }
    }
}
