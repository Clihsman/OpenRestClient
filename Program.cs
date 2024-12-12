/*
using OpenRestClient.Attributes;
using OpenRestClient.ApiController.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace OpenRestClient
{


    public class User
    {
        [JsonProperty("email")]
        public string? Email;

        [JsonProperty("password")]
        public string? Password;
    }

    public class Contact {
        [JsonProperty("name")]
        public string? Name;
    }

    [RestController("auth")]
    public interface ILoginService
    {
        [PostMapping("signin")]
        [RestAuthentication(AuthenticationType.JWT, AuthenticationMode.BEARER, "token")]
        Task Signin([InBody] User user);
    }

    [RestController("auth")]
    public class LoginService : RestApp
    {
        public LoginService() : base(typeof(LoginService)) { }

        [PostMapping("signin")]
        [RestAuthentication(AuthenticationType.JWT, AuthenticationMode.BEARER, "token")]
        public Task Signin([InBody] User user)
            => Call(nameof(Signin), user);
    }

    [RestController("whatsapp/contacts")]
    public class ContactService : RestApp
    {
        public ContactService() : base(typeof(ContactService)) { }

        [GetMapping]
        public Task<Contact[]?> GetContacts()
            => Call<Contact[]>(nameof(GetContacts));
    }

    class Program
    {
        private readonly static ILoginService loginService = RestApp.BuildApp<ILoginService>();
        private readonly static LoginService loginService2 = new();
        private readonly static ContactService contactsService = new();

        public static async Task Main()
        {
            Environment.SetEnvironmentVariable("opendev.openrestclient.host", "https://huemchatbot.com:442/api");
            try
            {


                await loginService2.Signin(new User {Email="clihsman.cs@gmail.com", Password="cs14503034" });

             //   Contact[]? result = await contactsService.GetContacts();
              //  Console.WriteLine(result!.Length);

            }
            catch (RestException ex)
            {
                Console.WriteLine("{0}: {1}", ex.HttpStatusCode, ex.Message);
            }
        }

      
    }
}
*/