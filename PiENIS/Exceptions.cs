using System;
using System.Collections.Generic;
using System.Text;

namespace PiENIS
{
    /// <summary>
    /// Thrown when there's a parser exception (i.e. the file is malformed).
    /// </summary>
    [Serializable]
    public class ParserException : Exception
    {
        public ParserException() { }
        public ParserException(string message) : base(message) { }
        public ParserException(string message, Exception inner) : base(message, inner) { }
        protected ParserException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
    
    /// <summary>
    /// Thrown when there's a conversion error.
    /// </summary>
    [Serializable]
    public class ConvertException : Exception
    {
        public ConvertException() { }
        public ConvertException(string message) : base(message) { }
        public ConvertException(string message, Exception inner) : base(message, inner) { }
        protected ConvertException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
