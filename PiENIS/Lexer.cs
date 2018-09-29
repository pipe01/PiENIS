using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

[assembly: InternalsVisibleTo("Test")]
[assembly: InternalsVisibleTo("PiENIS.Tests")]

namespace PiENIS
{
    [DebuggerDisplay("{Type}: {Value}")]
    internal struct LexToken
    {
        public enum Types
        {
            Key,
            BeginMultilineString,
            EndMultilineString,
            MultilineText,
            Value,
            Comment,
            BeginListItem
        }

        public Types Type { get; }
        public string Value { get; }
        public int Line { get; }
        public int StartIndex { get; }
        public int EndIndex { get; }
        public int IndentLevel { get; }

        public LexToken(Types type, string value, int line, int startIndex, int endIndex, int indentLevel)
        {
            this.Type = type;
            this.Value = value;
            this.Line = line;
            this.StartIndex = startIndex;
            this.EndIndex = endIndex;
            this.IndentLevel = indentLevel;
        }
    }

    internal static class Lexer
    {
        private class State
        {
            public bool InMultiline { get; set; }
        }

        public const char CommentBegin = '#';
        public const char IndentationChar = ' ';
        public const int IndentationCount = 4;
        public const char KeyValueSeparator = ':';
        public const char ListItemBegin = '-';
        public const char EscapeCharacter = '\\';
        public const string BeginMultilineString = "\"\"\"";
        public const string EndMultilineString = "\"\"\"";
        public const char OpenString = '"';
        public const char CloseString = '"';

        public static IEnumerable<LexToken> Lex(string[] lines)
        {
            int i = 0;
            var state = new State();
            
            foreach (var line in lines)
            {
                foreach (var tk in ParseLine(line, i++, state))
                    yield return tk;
            }
        }

        private static IEnumerable<LexToken> ParseLine(string line, int lineIndex, State state)
        {
            int indentation = line.TakeWhile(o => o == IndentationChar).Count() / IndentationCount;
            //line = line.Substring(indentation * IndentationCount);

            bool onIndentation = true;

            for (int i = 0; i < line.Length; i++)
            {
                if (state.InMultiline)
                {
                    if (line.Trim() == EndMultilineString)
                    {
                        state.InMultiline = false;
                        yield return new LexToken(LexToken.Types.EndMultilineString, null, lineIndex, 0, line.Length - 1, 0);
                    }
                    else
                    {
                        if (line[0] == OpenString && line[line.Length - 1] == CloseString && line.Length > 1)
                        {
                            line = line.TrimStart(OpenString).TrimEnd(CloseString);
                        }
                        else
                        {
                            line = line.Trim();
                        }

                        yield return new LexToken(LexToken.Types.MultilineText, line, lineIndex, 0, line.Length - 1, 0);
                    }

                    yield break;
                }

                if (line[i] == CommentBegin && (i == 0 || (i > 0 && line[i - 1] != EscapeCharacter)))
                {
                    string comment = line.Substring(i + 1);

                    yield return new LexToken(LexToken.Types.Comment, comment, lineIndex, i, line.Length - 1, indentation);
                    yield break;
                }
                else if (line[i] == KeyValueSeparator)
                {
                    string value = TakeWhile(line, i + 1, o => o != CommentBegin);

                    if (!string.IsNullOrWhiteSpace(value))
                        yield return ParseValue(value, i, 1);

                    i += value.Length;
                }
                else if (line[i] == ListItemBegin)
                {
                    yield return new LexToken(LexToken.Types.BeginListItem, null, lineIndex, i, i + 1, indentation);

                    string value = TakeWhile(line, i + 1, o => o != CommentBegin);

                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        yield return ParseValue(value, i, 0);
                    }

                    i++; //Skip list item begin char
                    i += value.Length;

                    if (value.EndsWith(" "))
                        i--;
                }
                else if (line[i] != IndentationChar || !onIndentation)
                {
                    onIndentation = false;

                    string key = TakeWhile(line, i, o => o != CommentBegin && o != KeyValueSeparator);

                    yield return new LexToken(LexToken.Types.Key, key.Trim(), lineIndex, i, i + key.Length, indentation);

                    i += key.Length - 1;
                }
            }

            LexToken ParseValue(string value, int i, int startOffset)
            {
                if (value.Trim() == BeginMultilineString)
                {
                    state.InMultiline = true;

                    return new LexToken(LexToken.Types.BeginMultilineString, null, lineIndex, i + startOffset, i + 1 + value.Length, indentation);
                }

                return new LexToken(LexToken.Types.Value, value.Trim(), lineIndex, i + startOffset, i + 1 + value.Length, indentation);
            }
        }

        private static string TakeWhile(string str, int offset, Predicate<char> pred)
        {
            string ret = "";

            for (; offset < str.Length; offset++)
            {
                if (pred(str[offset]))
                    ret += str[offset];
                else
                    break;
            }

            return ret;
        }
    }
}
