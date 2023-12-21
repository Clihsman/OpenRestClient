using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenRestClient.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public abstract class InField : Attribute
    {
    }
}
