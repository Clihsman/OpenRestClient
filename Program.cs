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
        Task Authenticate([InBody] User user);

        static ILoginService Build() => RestApp.BuildApp<ILoginService>();
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
        private readonly static LoginService loginService2 = new();
        private readonly static ParametersService contactsService = new();
        private readonly static UploadFileService uploadFileService = new();

        public static async Task Main()
        {
            Environment.SetEnvironmentVariable("opendev.openrestclient.host", "http://localhost:3000/api");
            Environment.SetEnvironmentVariable("opendev.openrestclient.debug", "true");
            try
            {
               
              //  await loginService.Authenticate(new User {Email="clihsman.cs@gmail.com", Password= "cs14503034" });
                FileFDD? fileFDD = await uploadFileService.UploadFile("abc", "isaac", 6236529);

                Console.WriteLine(fileFDD?.Id);

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