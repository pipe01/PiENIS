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
        /// <summary>
        /// Returned by <see cref="PENIS"/>' indexer.
        /// </summary>
        public struct Traverser : IEnumerable<Traverser>
        {
            private readonly IAtom Atom;
            private readonly PenisConfiguration Config;

            /// <summary>
            /// Gets the i-th array item.
            /// </summary>
            /// <param name="i">The index of the item.</param>
            /// <exception cref="InvalidOperationException">Thrown if the current item isn't a list.</exception>
            public Traverser this[int i]
            {
                get => Atom is ContainerAtom cont && cont.IsList ? new Traverser(cont.Atoms[i], this.Config) :
                    throw new InvalidOperationException();
            }

            /// <summary>
            /// Gets the item with key <paramref name="key"/>.
            /// </summary>
            /// <param name="key">The item's key.</param>
            /// <exception cref="InvalidOperationException">Thrown if the current item isn't an object.</exception>
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

            /// <summary>
            /// Deserializes an item to an object with type <paramref name="type"/>.
            /// </summary>
            /// <param name="type">The type that the item will be deserialized to.</param>
            public object To(Type type) => PenisConvert.ConvertAtom(this.Atom, type, this.Config);

            /// <summary>
            /// Deserializes an item to an object with type <typeparamref name="T"/>.
            /// </summary>
            /// <typeparam name="T">The type that the item will be deserialized to.</typeparam>
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
        private readonly PenisConfiguration Config;

        /// <summary>
        /// Creates a new PENIS instance and loads it from a file.
        /// </summary>
        /// <param name="file">The file provider.
        /// See <see cref="MemoryFile"/> or <see cref="IOFile"/>, or implement your own.</param>
        /// <param name="config">The configuration for the parser.</param>
        public PENIS(IFile file, PenisConfiguration config = null)
        {
            this.File = file;
            this.Config = config ?? PenisConfiguration.Default;

            this.Reload();
        }
        
        /// <summary>
        /// Gets an item by key in the top level. See <see cref="Traverser"/>.
        /// </summary>
        /// <param name="key">The desired item's key.</param>
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

        /// <summary>
        /// Gets an item with key <paramref name="key"/> and deserialized it to an object with type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type that the object will be deserialized to.</typeparam>
        /// <param name="key">The item's key.</param>
        public T Get<T>(string key) => (T)Get(key, typeof(T));

        /// <summary>
        /// Gets an item with key <paramref name="key"/> and deserialized it to an object with type <typeparamref name="T"/>.
        /// Alternatively, returns <paramref name="default"/> if the key cannot be found.
        /// </summary>
        /// <typeparam name="T">The type that the object will be deserialized to.</typeparam>
        /// <param name="key">The item's key.</param>
        /// <param name="default">This will be returned if the key cannot be found.</param>
        public T Get<T>(string key, T @default) => (T)Get(key, typeof(T), @default);

        /// <summary>
        /// Gets an item with key <paramref name="key"/> and deserialized it to an object with type <paramref name="type"/>.
        /// </summary>
        /// <param name="key">The item's key.</param>
        /// <param name="type">The type that the object will be deserialized to.</param>
        public object Get(string key, Type type) => GetTraverser(key, true).To(type);

        /// <summary>
        /// Gets an item with key <paramref name="key"/> and deserialized it to an object with type <paramref name="type"/>.
        /// Alternatively, returns <paramref name="default"/> if the key cannot be found.
        /// </summary>
        /// <param name="key">The item's key.</param>
        /// <param name="type">The type that the object will be deserialized to.</param>
        /// <param name="default">This will be returned if the key cannot be found.</param>
        public object Get(string key, Type type, object @default)
        {
            var traverser = GetTraverser(key, false);

            if (traverser.Equals(default(Traverser)))
                return @default;

            return traverser.To(type);
        }

        /// <summary>
        /// Sets a top level to <paramref name="value"/>, or adds it if it doesn't exist.
        /// </summary>
        /// <param name="key">The item's key.</param>
        /// <param name="value">The new value.</param>
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

        /// <summary>
        /// Removes an item with key <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The item to be removed's key.</param>
        /// <exception cref="KeyNotFoundException">Thrown if the item cannot be found.</exception>
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

        /// <summary>
        /// Discards changes in memory and reloads the data from file.
        /// </summary>
        public void Reload()
        {
            this.LexedTokens = Lexer.Lex(this.File.ReadAll().SplitLines(), this.Config).ToArray();
            this.ParsedAtoms = Parser.Parse(LexedTokens, this.Config).ToList();
        }

        /// <summary>
        /// Saves changes in memory to the file.
        /// </summary>
        public void Save()
        {
            this.File.WriteAll(Serialize());
        }

        /// <summary>
        /// Deserializes the entire file to an object. Item keys will be matched to field or property names. See also <seealso cref="DontPenisAttribute"/>.
        /// </summary>
        /// <param name="type">The type that the file will be converted to.</param>
        public object ToObject(Type type) => PenisConvert.DeserializeObject(ParsedAtoms, type);

        /// <summary>
        /// Deserializes the entire file to an object. Item keys will be matched to field or property names. See also <seealso cref="DontPenisAttribute"/>.
        /// </summary>
        /// <typeparam name="T">The type that the file will be converted to.</typeparam>
        public T ToObject<T>() => (T)ToObject(typeof(T));
        
        internal string Serialize() => Lexer.Unlex(Parser.Unparse(this.ParsedAtoms), this.Config);

        public IEnumerator<Traverser> GetEnumerator()
        {
            var config = this.Config;
            return ParsedAtoms.Select(o => new Traverser(o, config)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
