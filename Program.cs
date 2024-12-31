/*
using OpenRestClient.Attributes;
using OpenRestController;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Dynamic;
using System.Reflection;
using OpenRestClient.ApiController.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

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

    // public record User([JsonProperty("email")] string email, [JsonProperty("password")] string password);
    /*
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
    /*
    
    [RestController("whatsapp/contacts")]
    public class ContactService : RestApp
    {
        public ContactService() : base(typeof(ContactService)) { }

        [GetMapping]
        public Task<Contact[]?> GetContacts()
            => Call<Contact[]>(nameof(GetContacts));
    }
    */
    /*
    [RestController("whatsapp/contacts")]
    public interface ContactService 
    {

        [GetMapping]
        public Task<Contact[]?> GetContacts();
    }

    class Program
    {
        private readonly static ILoginService loginService = RestApp.BuildApp<ILoginService>();
       // private readonly static ContactService contactsService = new();
        private readonly static ContactService contactsService = RestApp.BuildApp<ContactService>();

        public static async Task Main()
        {
            Environment.SetEnvironmentVariable("opendev.openrestclient.host", "https://huemchatbot.com:442/api");
            try
            {
                await loginService.Signin(new User {Email="clihsman.cs@gmail.com", Password="cs14503034" });
                Contact[]? result = await contactsService.GetContacts();
                Console.WriteLine(result?.Length);

            }
            catch (RestException ex)
            {
                Console.WriteLine("{0}: {1}", ex.HttpStatusCode, ex.Message);
            }
        }

      
    }
}
    */