using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PiENIS
{
    internal enum LineType
    {
        Empty,
        Comment,
        Data
    }

    internal struct Line
    {
        public int Number { get; }
        public LineType Type { get; }
        public string Data { get; }

        public int Level => Data.TakeWhile(o => o == ' ').Count() / Parser.IndentationSpaces;

        public Line(int number, LineType type, string data)
        {
            this.Number = number;
            this.Type = type;
            this.Data = data;
        }
    }
}
