using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace PiENIS.Tests
{
    [TestClass]
    public class ParserTest
    {
        private static string[] L(string str) => str.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        [TestMethod]
        public void SimpleKeyValue()
        {
            var ret = Parser.Parse(Lexer.Lex(L("foo: bar")));

            Assert.AreEqual(ret[0], new KeyValueAtom("foo", "bar"));
        }

        [TestMethod]
        public void IgnoreComment()
        {
            var ret = Parser.Parse(Lexer.Lex(L(@"#ignore
foo: bar #this too
#and this")));

            Assert.AreEqual(ret[0], new KeyValueAtom("foo", "bar"));
        }

        [TestMethod]
        public void FlatList()
        {
            var ret = Parser.Parse(Lexer.Lex(L(@"list:
    - first
    - second")));

            Assert.AreEqual(ret[0], new ContainerAtom("list", true, new List<IAtom>
            {
                new KeyValueAtom(null, "first"),
                new KeyValueAtom(null, "second")
            }));
        }

        [TestMethod]
        public void NestedLists()
        {
            var ret = Parser.Parse(Lexer.Lex(L(@"list:
    - first
    -
        - inner first
        - inner second")));

            Assert.AreEqual(ret[0], new ContainerAtom("list", true, new List<IAtom>
            {
                new KeyValueAtom(null, "first"),
                new ContainerAtom(null, true, new List<IAtom>
                {
                    new KeyValueAtom(null, "inner first"),
                    new KeyValueAtom(null, "inner second")
                })
            }));
        }

        [TestMethod]
        public void MultilineString()
        {
            var ret = Parser.Parse(Lexer.Lex(L("hola: \"\"\"\ntest: foo\nlol que tal\n\"\"\"")));

            Assert.AreEqual(ret[0], new KeyValueAtom("hola", "test: foo\nlol que tal"));
        }
    }
}
