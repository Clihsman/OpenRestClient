using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenRestClient.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class InJoin : InField
    {
        public string Separator { get; set; }
        public InJoin(string separator)
        {
            Separator = separator;
        }
    }
}
