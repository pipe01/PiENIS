using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace PiENIS
{
    internal static class Parser
    {
        public class Config
        {
            public bool ParseLiteralValues { get; set; } = true;
        }
        
        public static IAtom[] Parse(IEnumerable<LexToken> tokens, Config config = default)
        {
            var atoms = ParseInner(tokens, config ?? new Config());
            CheckSyntax(atoms);

            return atoms.ToArray();
        }

        private static IEnumerable<IAtom> ParseInner(IEnumerable<LexToken> tokens, Config config)
        {
            var linkedTokens = new LinkedList<LexToken>(tokens);
            LinkedListNode<LexToken> tokenNode = linkedTokens.First;

            var containerStack = new Stack<(LexToken Token, ContainerAtom Atom)>();

            do
            {
                if (tokenNode == null)
                    break;

                var token = tokenNode.Value;

                //Check if we exited any containers
                while (containerStack.Count > 0 && token.IndentLevel <= containerStack.Peek().Token.IndentLevel)
                {
                    var atom = containerStack.Pop().Atom;

                    if (containerStack.Count > 0)
                        containerStack.Peek().Atom.Atoms.Add(atom);
                    else
                        yield return atom;
                }

                if (token.Type == LexToken.Types.Key || token.Type == LexToken.Types.BeginListItem)
                {
                    var nextType = tokenNode.Next?.Value.Type;

                    //Check for simple "key: value" pair or list item
                    if (nextType == LexToken.Types.Value || nextType == LexToken.Types.BeginMultilineString)
                    {
                        object value;

                        if (nextType == LexToken.Types.BeginMultilineString)
                        {
                            var startingToken = tokenNode;
                            tokenNode = tokenNode.Next;

                            string str = "";

                            while ((tokenNode = tokenNode.Next).Value.Type != LexToken.Types.EndMultilineString)
                            {
                                str += tokenNode.Value.Value + "\n";
                            }

                            value = str.TrimEnd('\n');
                            tokenNode = startingToken;
                        }
                        else
                        {
                            value = config.ParseLiteralValues ? ParseValue(tokenNode.Next.Value.Value) :
                                tokenNode.Next.Value.Value;
                        }

                        var atom = new KeyValueAtom(token.Value, value);

                        if (containerStack.Count > 0)
                            containerStack.Peek().Atom.Atoms.Add(atom);
                        else
                            yield return atom;

                        tokenNode = tokenNode.Next;
                    }
                    else //Item container (i.e. object or list) begins
                    {
                        bool isList = tokenNode != linkedTokens.Last && tokenNode.Next.Value.Type == LexToken.Types.BeginListItem;

                        containerStack.Push((token, new ContainerAtom(token.Value, isList)));
                    }
                }
            } while ((tokenNode = tokenNode.Next) != null);

            if (containerStack.Count != 0)
            {
                (LexToken Token, IAtom Atom) container = default;

                do
                {
                    if (containerStack.Count > 0)
                        container = containerStack.Pop();

                    if (containerStack.Count == 0)
                        yield return container.Atom;
                    else
                        containerStack.Peek().Atom.Atoms.Add(container.Atom);
                } while (containerStack.Count > 0);
            }
        }

        private static object ParseValue(string value)
        {
            if (int.TryParse(value, out var i))
                return i;
            else if (long.TryParse(value, out var l))
                return l;
            else if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var f))
                return f;
            else if (value == "nan")
                return float.NaN;
            else if (value == "infinity")
                return float.PositiveInfinity;
            else if (value == "-infinity")
                return float.NegativeInfinity;
            else if (bool.TryParse(value, out var b))
                return b;

            return value;
        }

        private static void CheckSyntax(IEnumerable<IAtom> atoms)
        {
            foreach (var atom in atoms)
            {
                if (atom is ContainerAtom container)
                {
                    foreach (var child in container.Atoms)
                    {
                        if (child is KeyValueAtom kv && ((kv.Key == null && !container.IsList)
                                                     || (kv.Key != null && container.IsList)))
                        {
                            throw new ParserException("A list may only have list items, and an object may not have list items.");
                        }
                    }
                }
            }
        }
    }
}
