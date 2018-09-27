using System;
using System.Collections.Generic;
using System.Text;

namespace PiENIS
{
    [Serializable]
    public class LineFormatException : FormatException
    {
        protected LineFormatException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        public LineFormatException(int lineNumber, string message) : base($"Line {lineNumber}: {message}")
        {
        }
    }
}
