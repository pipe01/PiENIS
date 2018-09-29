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
        public class Test
        {
            public class IDK
            {
                public bool LoL { get; set; }
            }

            public enum EnumT
            {
                AA,
                BB,
                CC
            }

            public object Integer { get; set; }
            public string String { get; set; }
            public IDK Foo { get; set; }
            public int[][] Numbers { get; set; }
            public EnumT Ena;
        }

        static void Main(string[] args)
        {
            var str = @"
Integer: 123
String: Hello World!
Foo:
    LoL: true
Numbers:
    -
        - 1
        - 2
    -
        - 3
        - 4
    -
        - 5
        - 6
Ena: BB
";

            var l = new PENIS(new MemoryFile(str));
        }

        private class MemoryFile : IFile
        {
            private string Content;

            public MemoryFile(string content)
            {
                this.Content = content;
            }

            public string ReadAll() => Content;

            public void WriteAll(string contents) => this.Content = contents;
        }
    }
}
