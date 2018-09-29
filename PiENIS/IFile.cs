using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PiENIS
{
    /// <summary>
    /// Used as an interface between a real file and PiENIS.
    /// </summary>
    public interface IFile
    {
        /// <summary>
        /// Reads the entire file.
        /// </summary>
        string ReadAll();

        /// <summary>
        /// Replaces the file's content.
        /// </summary>
        /// <param name="contents">The new file content.</param>
        void WriteAll(string contents);
    }

    /// <summary>
    /// Represents a file with a <see cref="File"/> as the backing store.
    /// </summary>
    public sealed class IOFile : IFile
    {
        private readonly string Path;

        /// <summary>
        /// Creates an <see cref="IOFile"/> that points to <paramref name="path"/>.
        /// </summary>
        /// <param name="path">The file's path</param>
        /// <exception cref="FileNotFoundException">Thrown if the file doesn't exist.</exception>
        public IOFile(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException();

            this.Path = path;
        }

        /// <summary>
        /// Reads the entire file.
        /// </summary>
        public string ReadAll() => File.ReadAllText(this.Path);

        /// <summary>
        /// Replaces the file's content.
        /// </summary>
        /// <param name="contents">The new file content.</param>
        public void WriteAll(string contents) => File.WriteAllText(this.Path, contents);
    }

    /// <summary>
    /// Represents a file without a physical media (it only exists on memory).
    /// </summary>
    public sealed class MemoryFile : IFile
    {
        /// <summary>
        /// The file's raw content.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Creates a <see cref="MemoryFile"/> with <paramref name="content"/>.
        /// </summary>
        /// <param name="content">The file's content.</param>
        public MemoryFile(string content)
        {
            this.Content = content;
        }

        /// <summary>
        /// Reads the entire file.
        /// </summary>
        public string ReadAll() => Content;
        
        /// <summary>
        /// Replaces the file's content.
        /// </summary>
        /// <param name="contents">The new file content.</param>
        public void WriteAll(string contents) => Content = contents;
    }
}
