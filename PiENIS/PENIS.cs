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
            private readonly PenisConfiguration Config;

            public Traverser this[int i]
            {
                get => Atom is ContainerAtom cont && cont.IsList ? new Traverser(cont.Atoms[i], this.Config) :
                    throw new InvalidOperationException();
            }

            public Traverser this[string key]
            {
                get => Atom is ContainerAtom cont && !cont.IsList ?
                    new Traverser(cont.Atoms.FirstOrDefault(o => o.Key == key) ?? throw new KeyNotFoundException(), this.Config) :
                    throw new InvalidOperationException();
            }

            public bool IsList => Atom is ContainerAtom cont && cont.IsList;
            public bool IsObject => Atom is ContainerAtom cont && !cont.IsList;
            public string Key => Atom.Key;

            internal Traverser(IAtom atom, PenisConfiguration config)
            {
                this.Atom = atom;
                this.Config = config;
            }

            public object To(Type type) => PenisConvert.ConvertAtom(this.Atom, type, this.Config);

            public T To<T>() => (T)To(typeof(T));

            public IEnumerator<Traverser> GetEnumerator()
            {
                var config = this.Config;

                return Atom is ContainerAtom cont
                        ? cont.Atoms.Select(o => new Traverser(o, config)).GetEnumerator()
                        : throw new InvalidOperationException();
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        private LexToken[] LexedTokens;
        private IList<IAtom> ParsedAtoms;
        private readonly IFile File;
        private PenisConfiguration Config;

        public PENIS(IFile file, PenisConfiguration config = null)
        {
            this.File = file;
            this.Config = config ?? PenisConfiguration.Default;

            this.Reload();
        }
        
        public Traverser this[string key]
        {
            get => GetTraverser(key, true);
        }

        private Traverser GetTraverser(string key, bool @throw)
        {
            var atom = ParsedAtoms.SingleOrDefault(o => o.Key.Equals(key));

            if (atom == null)
            {
                if (@throw)
                    throw new KeyNotFoundException();
                else
                    return default;
            }

            return new Traverser(atom, this.Config);
        }

        public T Get<T>(string key) => (T)Get(key, typeof(T));
        public T Get<T>(string key, T @default) => (T)Get(key, typeof(T), @default);

        public object Get(string key, Type type) => GetTraverser(key, true).To(type);
        public object Get(string key, Type type, object @default)
        {
            var traverser = GetTraverser(key, false);

            if (traverser.Equals(default(Traverser)))
                return @default;

            return traverser.To(type);
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

        public void Remove(string key)
        {
            var atom = ParsedAtoms.SingleOrDefault(o => o.Key.Equals(key));

            if (atom == null)
                throw new KeyNotFoundException();

            var decorations = atom.Decorations;
            int index = ParsedAtoms.IndexOf(atom);

            ParsedAtoms.Remove(atom);

            if (ParsedAtoms.Count == 0)
                return;
            
            var newParent = 
                  index > 0 ? ParsedAtoms[index - 1]
                : index < ParsedAtoms.Count ? ParsedAtoms[index]
                : null;
            bool isAfterOld = ParsedAtoms.IndexOf(newParent) >= index;

            if (newParent == null)
                return;

            foreach (var item in decorations)
            {
                if (item is EmptyLineDecoration)
                {
                    newParent.Decorations.Add(new EmptyLineDecoration(
                        isAfterOld ? EmptyLineDecoration.Positions.Before : EmptyLineDecoration.Positions.After));
                }
                else if (item is CommentDecoration comment && comment.Position != CommentDecoration.Positions.Inline)
                {
                    newParent.Decorations.Add(new CommentDecoration(comment.Comment,
                        isAfterOld ? CommentDecoration.Positions.Before : CommentDecoration.Positions.After));
                }
            }
        }

        public void Reload()
        {
            this.LexedTokens = Lexer.Lex(this.File.ReadAll().SplitLines(), this.Config).ToArray();
            this.ParsedAtoms = Parser.Parse(LexedTokens, this.Config).ToList();
        }

        public void Save()
        {
            this.File.WriteAll(Lexer.Unlex(Parser.Unparse(this.ParsedAtoms), this.Config));
        }

        public object ToObject(Type type) => PenisConvert.DeserializeObject(ParsedAtoms, type);

        public T ToObject<T>() => (T)ToObject(typeof(T));

        public IEnumerator<Traverser> GetEnumerator()
        {
            var config = this.Config;
            return ParsedAtoms.Select(o => new Traverser(o, config)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
