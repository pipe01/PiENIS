using System;
using System.Collections.Generic;
using System.Text;

namespace PiENIS
{
    public class PENIS
    {
        internal Line[] Lines { get; }

        private readonly IFileProvider Provider;

        private PENIS(IFileProvider provider)
        {
            this.Provider = provider;
        }
        
        public static PENIS LoadFromFile(IFileProvider provider)
        {
            return new PENIS(provider); //TODO Call Parser
        }
    }
}
