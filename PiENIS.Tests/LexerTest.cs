using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PiENIS.Tests
{
    [TestClass]
    public class LexerTest
    {
        private static string[] L(string str) => str.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        [TestMethod]
        public void SimpleKeyValue()
        {
            AssertTypesMatch(Lexer.Lex(L("foo: bar")), LexToken.Types.Key, LexToken.Types.Value);
        }

        [TestMethod]
        public void SimpleKeyValueWithTrailingComment()
        {
            AssertTypesMatch(Lexer.Lex(L("foo: bar #cm")), LexToken.Types.Key, LexToken.Types.Value, LexToken.Types.Comment);
        }

        [TestMethod]
        public void SimpleObject()
        {
            string str = @"foo:
    bar: idk
    prop: 123";

            AssertTypesMatch(Lexer.Lex(L(str)),
                LexToken.Types.Key,
                LexToken.Types.Key, LexToken.Types.Value,
                LexToken.Types.Key, LexToken.Types.Value);
        }

        [TestMethod]
        public void SimpleObjectWithComments()
        {
            string str = @"foo: #this
    bar: idk #shouldn't
    prop: 123 #matter";

            AssertTypesMatch(Lexer.Lex(L(str)), 
                LexToken.Types.Key, LexToken.Types.Comment,
                LexToken.Types.Key, LexToken.Types.Value, LexToken.Types.Comment,
                LexToken.Types.Key, LexToken.Types.Value, LexToken.Types.Comment);
        }

        [TestMethod]
        public void SimpleList()
        {
            string str = @"foo:
    - idk
    - 123";

            AssertTypesMatch(Lexer.Lex(L(str)),
                LexToken.Types.Key,
                LexToken.Types.BeginListItem, LexToken.Types.Value,
                LexToken.Types.BeginListItem, LexToken.Types.Value);
        }

        [TestMethod]
        public void SimpleListWithComments()
        {
            string str = @"foo: #this
    - idk #shouldn't
    - 123 #matter";

            AssertTypesMatch(Lexer.Lex(L(str)),
                LexToken.Types.Key, LexToken.Types.Comment,
                LexToken.Types.BeginListItem, LexToken.Types.Value, LexToken.Types.Comment,
                LexToken.Types.BeginListItem, LexToken.Types.Value, LexToken.Types.Comment);
        }

        [TestMethod]
        public void MultilineString()
        {
            string str = "string: \"\"\"\n" +
                @"one line
two lines
three lines
" + "\"\"\"\nsome: dummy";

            var l = Lexer.Lex(L(str)).ToArray();

            var expected = new (LexToken.Types, string)[]
            {
                (LexToken.Types.Key, "string"),
                (LexToken.Types.BeginMultilineString, null),
                (LexToken.Types.MultilineText, "one line"),
                (LexToken.Types.MultilineText, "two lines"),
                (LexToken.Types.MultilineText, "three lines"),
                (LexToken.Types.EndMultilineString, null),
                (LexToken.Types.Key, "some"),
                (LexToken.Types.Value, "dummy"),
            };

            int i = 0;
            Assert.IsTrue(l.All(o => o.Type == expected[i].Item1 && o.Value == expected[i++].Item2));
        }

        [TestMethod]
        public void Comment()
        {
            AssertTypesMatch(Lexer.Lex(L("#this is #a comment")), LexToken.Types.Comment);
        }

        [TestMethod]
        public void Indentation()
        {
            string str = @"no: indent
obj:
    some:
        indent: lol
hi: sup";

            var expected = new[] { 0, 0, 0, 1, 2, 2, 0, 0 };
            int i = 0;

            Assert.IsTrue(Lexer.Lex(L(str)).All(o => o.IndentLevel == expected[i++]));
        }

        private static void AssertTypesMatch(IEnumerable<LexToken> tokens, params LexToken.Types[] types)
        {
            if (tokens.Count() != types.Length)
                Assert.Fail($"Expected {types.Length} tokens, got {tokens.Count()}");

            int i = 0;

            foreach (var item in tokens)
            {
                if (item.Type != types[i])
                    Assert.Fail($"Expected {types[i]}, got {item.Type} at token index {i}");

                i++;
            }
        }
    }
}
