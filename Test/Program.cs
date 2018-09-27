using PiENIS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var lines = @"#asd
foo: bar
hola:
    - 123
    - asdasd
    -
        - asd
        - 456
lol: xd
".Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            
            var n = new Parser().Parse(lines).ToArray();
        }
    }
}
