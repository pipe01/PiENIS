using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace PiENIS
{
    internal interface IAtom
    {
        string Key { get; }
    }

    [DebuggerDisplay("{Key}: {Value}")]
    internal struct KeyValueAtom : IAtom, IEquatable<KeyValueAtom>
    {
        public string Key { get; }
        public object Value { get; }

        public KeyValueAtom(string key, object value) : this()
        {
            this.Key = key;
            this.Value = value;
        }

        public bool Equals(KeyValueAtom other) =>
               (this.Key?.Equals(other.Key) ?? other.Key == null)
            && (this.Value?.Equals(other.Value) ?? other.Value == null);

        public override bool Equals(object obj) => obj is KeyValueAtom kva && Equals(kva);

        public override int GetHashCode() => (this.Key?.GetHashCode() ?? 0) * 17 + (this.Value?.GetHashCode() ?? 0);
    }

    [DebuggerDisplay("{Key}: {Atoms.Count} children")]
    internal struct ContainerAtom : IAtom, IEquatable<ContainerAtom>
    {
        public string Key { get; }
        public bool IsList { get; }
        public IList<IAtom> Atoms { get; }

        public ContainerAtom(string key, bool isList, IList<IAtom> atoms = null)
        {
            this.Key = key;
            this.IsList = isList;
            this.Atoms = atoms ?? new List<IAtom>(new IAtom[0]);
        }

        public bool Equals(ContainerAtom other)
            => (this.Key?.Equals(other.Key) ?? other.Key == null)
            && this.IsList == other.IsList && this.Atoms.Count == other.Atoms.Count
            && (this.Atoms?.SequenceEqual(other.Atoms) ?? other.Atoms == null);

        public override bool Equals(object obj) => obj is ContainerAtom con && Equals(con);

        public override int GetHashCode()
            => (this.Key?.GetHashCode() * 17 ?? 0)
             + this.IsList.GetHashCode() * 17
             + (this.Atoms?.GetHashCode() ?? 0);
    }
}
