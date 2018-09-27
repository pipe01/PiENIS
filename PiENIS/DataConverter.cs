using System;
using System.Collections.Generic;
using System.Text;

namespace PiENIS
{
    internal static class DataConverter
    {
        public static object ParseValue(string value)
        {
            if (int.TryParse(value, out var i))
                return i;
            if (long.TryParse(value, out var l))
                return l;
            if (ulong.TryParse(value, out var ul))
                return ul;

            if (float.TryParse(value, out var f))
                return f;
            if (value == "infinity")
                return float.PositiveInfinity;
            if (value == "-infinity")
                return float.NegativeInfinity;
            if (value == "nan")
                return float.NaN;

            if (bool.TryParse(value, out var b))
                return b;

            if (DateTime.TryParse(value, out var dt))
                return dt;

            return value.Trim();
        }
    }
}
