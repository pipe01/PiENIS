using System;
using System.Collections.Generic;
using System.Text;

namespace PiENIS
{
    /// <summary>
    /// Configuration for PiENIS.
    /// </summary>
    public sealed class PenisConfiguration
    {
        internal static PenisConfiguration Default { get; } = new PenisConfiguration();

        /// <summary>
        /// True if case should be ignored when matching field and property names to keys and when deserializing enums.
        /// </summary>
        public bool IgnoreCase { get; set; }

        /// <summary>
        /// Amount of spaces for each indentation level.
        /// </summary>
        public int IndentationCount { get; set; } = 4;
    }
}
