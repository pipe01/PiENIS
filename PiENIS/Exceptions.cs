using System;
using System.Collections.Generic;
using System.Text;

namespace PiENIS
{
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
