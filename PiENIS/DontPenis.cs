using System;
using System.Collections.Generic;
using System.Text;

namespace PiENIS
{
    /// <summary>
    /// Makes PiENIS ignore this field or property for (de)serialization.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class DontPenisAttribute : Attribute
    {
    }
}
