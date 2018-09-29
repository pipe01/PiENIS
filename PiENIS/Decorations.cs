using System;
using System.Collections.Generic;
using System.Text;

namespace PiENIS
{
    internal interface IDecoration
    {
    }

    internal struct EmptyLineDecoration : IDecoration
    {
        public enum Positions
        {
            Before,
            After
        }

        public Positions Position { get; }

        public EmptyLineDecoration(Positions position)
        {
            this.Position = position;
        }
    }

    internal struct CommentDecoration : IDecoration
    {
        public enum Positions
        {
            Before,
            Inline,
            After
        }

        public Positions Position { get; }
        public string Comment { get; }

        public CommentDecoration(string comment, Positions position)
        {
            this.Comment = comment;
            this.Position = position;
        }
    }
}
