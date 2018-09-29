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
            public object[] Array;
        }

        static void Main(string[] args)
        {
            const string str = @"#first comment
some: element #inline comment
#another comment

other: element #in
#le comment";

            var file = new MemoryFile(str);
            var s = new PENIS(file);

            Console.WriteLine(file.Content);

            s.Set("other", "i don't know");
            s.Save();
            Console.WriteLine("=============================");
            Console.WriteLine(file.Content);

            s.Remove("some");
            s.Save();
            Console.WriteLine("=============================");
            Console.WriteLine(file.Content);
            Console.ReadKey(true);
        }
    }
}
