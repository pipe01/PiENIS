using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PiENIS
{
    public class PENIS : IEnumerable<PENIS.Traverser>
    {
        public struct Traverser : IEnumerable<Traverser>
        {
            private readonly IAtom Atom;

            public Traverser this[int i]
            {
                get => Atom is ContainerAtom cont && cont.IsList ? new Traverser(cont.Atoms[i]) :
                    throw new InvalidOperationException();
            }

            public Traverser this[string key]
            {
                get => Atom is ContainerAtom cont && !cont.IsList ?
                    new Traverser(cont.Atoms.FirstOrDefault(o => o.Key == key) ?? throw new KeyNotFoundException()) :
                    throw new InvalidOperationException();
            }

            public bool IsList => Atom is ContainerAtom cont && cont.IsList;
            public bool IsObject => Atom is ContainerAtom cont && !cont.IsList;
            public string Key => Atom.Key;

            internal Traverser(IAtom atom)
            {
                this.Atom = atom;
            }

            public object To(Type type) => PenisConvert.ConvertAtom(this.Atom, type);

            public T To<T>() => (T)To(typeof(T));

            public IEnumerator<Traverser> GetEnumerator()
                => Atom is ContainerAtom cont
                    ? cont.Atoms.Select(o => new Traverser(o)).GetEnumerator()
                    : throw new InvalidOperationException();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        private LexToken[] LexedTokens;
        private IList<IAtom> ParsedAtoms;
        private readonly IFile File;

        public PENIS(IFile file)
        {
            this.File = file;
            this.Reload();
        }
        
        public Traverser this[string key]
        {
            get => GetTraverser(key, true);
        }

        public T Get<T>(string key, T @default = default) => (T)Get(key, typeof(T), @default);

        public object Get(string key, Type type, object @default = null)
        {
            var traverser = GetTraverser(key, false);

            if (traverser.Equals(default(Traverser)))
                return @default;

            return traverser.To(type);
        }

        private Traverser GetTraverser(string key, bool @throw)
        {
            var atom = ParsedAtoms.SingleOrDefault(o => o.Key == key);

            if (atom == null)
            {
                if (@throw)
                    throw new KeyNotFoundException();
                else
                    return default;
            }

            return new Traverser(atom);
        }

        public void Set(string key, object value)
        {
            var atom = this.ParsedAtoms.SingleOrDefault(o => o.Key.Equals(key));
            int index = -1;
            IEnumerable<IDecoration> decorations = null;
            Type previousType = null;

            if (atom != null)
            {
                index = this.ParsedAtoms.IndexOf(atom);
                decorations = atom.Decorations;
                previousType = atom.GetType();
            }
            
            atom = PenisConvert.GetAtom(key, value);
            
            if (decorations != null)
            {
                foreach (var item in decorations)
                {
                    //Skip comment if the new atom isn't the same type as the previous one (KeyValue != Container)
                    if (!(item is CommentDecoration comment)
                        || comment.Position != CommentDecoration.Positions.Inline
                        || previousType == null
                        || atom.GetType() == previousType)
                    {
                        atom.Decorations.Add(item);
                    }
                }
            }

            if (index == -1)
                this.ParsedAtoms.Add(atom);
            else
                this.ParsedAtoms[index] = atom;
        }

        public void Reload()
        {
            this.LexedTokens = Lexer.Lex(this.File.ReadAll().SplitLines()).ToArray();
            this.ParsedAtoms = Parser.Parse(LexedTokens).ToList();
        }

        public void Save()
        {
            this.File.WriteAll(Lexer.Unlex(Parser.Unparse(this.ParsedAtoms)));
        }

        public object ToObject(Type type) => PenisConvert.DeserializeObject(ParsedAtoms, type);

        public T ToObject<T>() => (T)ToObject(typeof(T));

        public IEnumerator<Traverser> GetEnumerator() => ParsedAtoms.Select(o => new Traverser(o)).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
