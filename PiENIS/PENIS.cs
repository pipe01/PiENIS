using System;
using System.Collections;
using System.Collections.Generic;
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

        private readonly LexToken[] LexedTokens;
        private readonly IAtom[] ParsedAtoms;

        public PENIS(IFile file)
        {
            this.LexedTokens = Lexer.Lex(file.ReadAll().SplitLines()).ToArray();
            this.ParsedAtoms = Parser.Parse(LexedTokens);
        }
        
        public Traverser this[string key]
        {
            get
            {
                var atom = ParsedAtoms.SingleOrDefault(o => o.Key == key);

                if (atom == null)
                    throw new KeyNotFoundException();

                return new Traverser(atom);
            }
        }

        public object ToObject(Type type) => PenisConvert.DeserializeObject(ParsedAtoms, type);

        public T ToObject<T>() => (T)ToObject(typeof(T));

        public IEnumerator<Traverser> GetEnumerator() => ParsedAtoms.Select(o => new Traverser(o)).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
