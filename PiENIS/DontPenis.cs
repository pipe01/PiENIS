using System;
using System.Collections.Generic;
using System.Text;

namespace PiENIS
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class DontPenisAttribute : Attribute
    {
    }
}
