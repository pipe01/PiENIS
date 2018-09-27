using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

[assembly: InternalsVisibleTo("Test")]

namespace PiENIS
{
    internal class Parser
    {
        public const string Comment = "#";
        public const char KeySeparator = ':';
        public const char EscapeCharacter = '\\';
        public const string ListItemStart = "-";
        public const int IndentationSpaces = 4;

        private bool ParsingMultiline;

        public IEnumerable<INode> Parse(string[] lines) => Parse(new LinkedList<Line>(TransformLines(lines)));

        private IEnumerable<INode> Parse(LinkedList<Line> lines)
        {
            var nodeStack = new Stack<IParentNode>();
            LinkedListNode<Line> current = lines.First;

            do
            {
                if (current.Value.Level == 0)
                {
                    var node = ParseLineNode(current);

                    if (node != null)
                        yield return node;
                }
            } while ((current = current.Next) != null);
        }

        private INode ParseLineNode(LinkedListNode<Line> current, Line? parentLine = null)
        {
            var line = current.Value;

            if (line.Type == LineType.Comment)
                return null;

            if (ParsingMultiline)
            {
                //TODO
                return null;
            }

            if (line.Level > 0 && parentLine == null)
                throw new LineFormatException(line.Number, "invalid indentation");

            int separator = GetSeparatorInLine(line.Data);

            string key = separator >= 0 ? line.Data.Substring(0, separator).Trim() : null;
            string value = separator >= 0 ? line.Data.Substring(separator + 1).Trim() : null;

            //Parse list or object item
            if (parentLine != null && line.Level == parentLine?.Level + 1)
            {
                if (line.Data.TrimStart().StartsWith(ListItemStart))
                {
                    string itemValue = line.Data.TrimStart().Substring(ListItemStart.Length);

                    if (string.IsNullOrWhiteSpace(itemValue))
                        return ParseLineNode(current.Next, line);
                    
                    return new LiteralNode(DataConverter.ParseValue(itemValue));
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(value))
                        return ParseLineNode(current.Next, line);

                    return new SimpleKeyValueNode(key, new LiteralNode(DataConverter.ParseValue(value)));
                }
            }

            if (separator == -1)
                throw new LineFormatException(line.Number, "invalid line");
            
            if (string.IsNullOrWhiteSpace(value))
            {
                return ParseObjectOrList(current, line, key);
            }

            return new SimpleKeyValueNode(key, new LiteralNode(DataConverter.ParseValue(value)));
        }

        private INode ParseObjectOrList(LinkedListNode<Line> current, Line line, string key)
        {
            var nextNode = ParseLineNode(current.Next, line);

            IParentNode parentNode;

            if (nextNode is IKeyNode)
            {
                parentNode = new ObjectNode(key);
            }
            else
            {
                parentNode = new ListNode(key);
            }

            var currItem = current.Next;

            do
            {
                if (currItem.Value.Level == line.Level + 1)
                    parentNode.Children.Add(ParseLineNode(currItem, line));
                else
                    break;
            } while ((currItem = currItem.Next) != null && currItem.Value.Level > line.Level);

            return parentNode;
        }

        private static IEnumerable<Line> TransformLines(string[] raw)
        {
            return raw.Select((o, i) =>
            {
                var type = o.StartsWith(Comment) ? LineType.Comment :
                            string.IsNullOrWhiteSpace(o) ? LineType.Empty :
                            LineType.Data;

                return new Line(i, type, o);
            });
        }

        private static int GetSeparatorInLine(string line)
        {
            for (int i = 0; i < line.Length; i++)
            {
                if (line[i] == KeySeparator) //TODO Check for strings or whatever
                    return i;
            }

            return -1;
        }
    }
}
