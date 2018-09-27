using System;
using System.Collections.Generic;
using System.Text;

namespace PiENIS
{
    public interface INode
    {
    }

    public interface IKeyNode : INode
    {
        string Key { get; }
    }

    public interface IValueNode : INode
    {
        INode Value { get; }
    }

    public interface IParentNode : INode
    {
        IList<INode> Children { get; }
    }

    public struct LiteralNode : INode
    {
        public object Value { get; }

        public LiteralNode(object value)
        {
            this.Value = value;
        }
    }

    public struct SimpleKeyValueNode : IKeyNode, IValueNode
    {
        public string Key { get; }
        public INode Value { get; }

        public SimpleKeyValueNode(string key, INode value)
        {
            this.Key = key;
            this.Value = value;
        }
    }

    public struct ObjectNode : IKeyNode, IParentNode
    {
        public IList<INode> Children { get; }
        public string Key { get; }

        public ObjectNode(string key)
        {
            this.Key = key;
            this.Children = new List<INode>();
        }

        public ObjectNode(IList<INode> children, string key)
        {
            this.Children = children;
            this.Key = key;
        }
    }

    public struct ListNode : IKeyNode, IParentNode
    {
        public IList<INode> Children { get; }
        public string Key { get; }

        public ListNode(string key)
        {
            this.Key = key;
            this.Children = new List<INode>();
        }

        public ListNode(string key, IList<INode> children)
        {
            this.Key = key;
            this.Children = children;
        }
    }
}
