using System;
using System.Collections.Generic;
using System.Text;

namespace PiENIS
{
    internal static class Extensions
    {
        public static string[] SplitLines(this string str)
            => str.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
    }
}
