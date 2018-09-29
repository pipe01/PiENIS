using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace PiENIS
{
    internal static class Parser
    {
        public static IAtom[] Parse(IEnumerable<LexToken> tokens, PenisConfiguration config)
        {
            var atoms = ParseInner(tokens, config);
            CheckSyntax(atoms);

            return atoms.ToArray();
        }

        private static IEnumerable<IAtom> ParseInner(IEnumerable<LexToken> tokens, PenisConfiguration config)
        {
            var linkedTokens = new LinkedList<LexToken>(tokens);
            LinkedListNode<LexToken> tokenNode = linkedTokens.First;

            var containerStack = new Stack<(LexToken Token, ContainerAtom Atom)>();
            IAtom lastAtom = null;
            var pendingDecorations = new List<IDecoration>();

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
                        yield return lastAtom = atom;
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
                            value = ParseValue(tokenNode.Next.Value.Value);
                        }

                        var atom = new KeyValueAtom(token.Value, value);

                        if (containerStack.Count > 0)
                            containerStack.Peek().Atom.Atoms.Add(atom);
                        else
                            yield return lastAtom = atom;

                        tokenNode = tokenNode.Next;
                    }
                    else //Item container (i.e. object or list) begins
                    {
                        bool isList = tokenNode != linkedTokens.Last && tokenNode.Next.Value.Type == LexToken.Types.BeginListItem;

                        var atom = new ContainerAtom(token.Value, isList);
                        lastAtom = atom;

                        containerStack.Push((token, atom));
                    }
                }
                else if (token.Type == LexToken.Types.EmptyLine)
                {
                    if (lastAtom != null)
                        lastAtom.Decorations.Add(new EmptyLineDecoration(EmptyLineDecoration.Positions.After));
                    else
                        pendingDecorations.Add(new EmptyLineDecoration(EmptyLineDecoration.Positions.Before));

                    continue;
                }
                else if (token.Type == LexToken.Types.Comment)
                {
                    if (lastAtom != null)
                    {
                        lastAtom.Decorations.Add(new CommentDecoration(token.Value,
                            token.FirstInLine ? CommentDecoration.Positions.After : CommentDecoration.Positions.Inline));
                    }
                    else
                    {
                        pendingDecorations.Add(new CommentDecoration(token.Value, CommentDecoration.Positions.Before));
                    }

                    continue;
                }

                foreach (var item in pendingDecorations)
                {
                    lastAtom.Decorations.Add(item);
                }

                pendingDecorations.Clear();
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

        public static IEnumerable<LexToken> Unparse(IEnumerable<IAtom> atoms) => Unparse(atoms, 0);

        public static IEnumerable<LexToken> Unparse(IEnumerable<IAtom> atoms, int levelOffset)
        {
            var containers = new Stack<ContainerAtom>();

            foreach (var atom in atoms)
            {
                int level = containers.Count + levelOffset;

                foreach (var item in atom.Decorations)
                {
                    if (item is EmptyLineDecoration emptyLine && emptyLine.Position == EmptyLineDecoration.Positions.Before)
                    {
                        yield return new LexToken(LexToken.Types.EmptyLine, null, 0, false);
                    }
                    else if (item is CommentDecoration comment && comment.Position == CommentDecoration.Positions.Before)
                    {
                        yield return new LexToken(LexToken.Types.Comment, comment.Comment, 0, true);
                    }
                }

                if (atom.Key == null)
                {
                    yield return new LexToken(LexToken.Types.BeginListItem, null, level, true);
                }
                else
                {
                    yield return new LexToken(LexToken.Types.Key, atom.Key, level, true);
                }

                if (atom is KeyValueAtom kva)
                {
                    if (kva.Value is string str && str.Any(o => o == '\r' || o == '\n'))
                    {
                        yield return new LexToken(LexToken.Types.BeginMultilineString, null, 0, false);

                        foreach (var item in str.SplitLines())
                        {
                            yield return new LexToken(LexToken.Types.MultilineText, item, 0, true);
                        }

                        yield return new LexToken(LexToken.Types.EndMultilineString, null, 0, true);
                    }
                    else
                    {
                        yield return new LexToken(LexToken.Types.Value, kva.Value?.ToString(), level, false);
                    }
                }
                else if (atom is ContainerAtom container)
                {
                    foreach (var item in Unparse(container.Atoms, level + 1))
                        yield return item;
                }

                foreach (var item in atom.Decorations)
                {
                    if (item is EmptyLineDecoration emptyLine && emptyLine.Position == EmptyLineDecoration.Positions.After)
                    {
                        yield return new LexToken(LexToken.Types.EmptyLine, null, 0, true);
                    }
                    else if (item is CommentDecoration comment && comment.Position != CommentDecoration.Positions.Before)
                    {
                        yield return new LexToken(LexToken.Types.Comment, comment.Comment, 0, comment.Position != CommentDecoration.Positions.Inline);
                    }
                }
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
    }
}
