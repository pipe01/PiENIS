using System;
using System.Collections.Generic;
using System.Text;

namespace PiENIS
{
    public sealed class PenisConfiguration
    {
        internal static PenisConfiguration Default { get; } = new PenisConfiguration();

        public bool IgnoreCase { get; set; }
        public int IndentationCount { get; set; } = 4;
    }
}
