using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PiENIS
{
    internal static class Extensions
    {
        public static string[] SplitLines(this string str)
            => str.Replace("\r\n", "\n").Split('\n');

        public static IEnumerable<T> NotNull<T>(this IEnumerable<T> en) where T : class
            => en.Where(o => o != null);
    }
}
