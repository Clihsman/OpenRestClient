using OpenInjector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenRestClient.ApiController.Config;

[InjectorConfiguration]
public class RestConfig
{
   public HttpClient HttpClient { get; set; }
}
