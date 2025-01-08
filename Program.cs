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
        Task Authenticate([InBody] User user);

        static ILoginService Build() => RestApp.BuildApp<ILoginService>();
    }
    /*
    
    [RestController("whatsapp/contacts")]
    public class ContactService : RestApp
    {
        public LoginService() : base(typeof(LoginService)) { }

        [PostMapping("signin")]
        [RestAuthentication(AuthenticationType.JWT, AuthenticationMode.BEARER, "token")]
        public Task Signin([InBody] User user)
            => Call(nameof(Signin), user);
    }
    */
    /*
    [RestController("whatsapp/contacts")]
    public interface ContactService 
    {

        [GetMapping]
        public Task<Contact[]?> GetContacts();
    }

    public class Specialty
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }


    [RestController("parameters")]
    public class ParametersService : RestApp
    {
        public ParametersService() : base(typeof(ParametersService)) { }

        [GetMapping("specialties")]
        public Task<List<Specialty>?> GetSpecialties()
            => Call<List<Specialty>>(nameof(GetSpecialties));
    }

    public class FileFDD
    {
        [JsonProperty("id")]
        public int Id { get; set; }
    }

    [RestController("upload")]
    public class UploadFileService : RestApp
    {
        public UploadFileService() : base(typeof(UploadFileService)) { }

        [PostMapping]
        public Task<FileFDD?> UploadFile([InField] string abc, [InQuery] string name, [InQuery] int document)
            => Call<FileFDD>(nameof(UploadFile), abc, name, document);
    }


    class Program
    {
        private readonly static ILoginService loginService = RestApp.BuildApp<ILoginService>();
       // private readonly static ContactService contactsService = new();
        private readonly static ContactService contactsService = RestApp.BuildApp<ContactService>();

        public static async Task Main()
        {
            Environment.SetEnvironmentVariable("opendev.openrestclient.host", "http://localhost:3000/api");
            Environment.SetEnvironmentVariable("opendev.openrestclient.debug", "true");
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
            Console.ReadKey();
        }

      
    }
}
    */
