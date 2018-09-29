using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PiENIS
{
    public interface IFile
    {
        string ReadAll();
        void WriteAll(string contents);
    }

    public sealed class IOFile : IFile
    {
        private readonly string Path;

        public IOFile(string path)
        {
            this.Path = path;
        }

        public string ReadAll() => File.ReadAllText(this.Path);

        public void WriteAll(string contents) => File.WriteAllText(this.Path, contents);
    }

    public sealed class MemoryFile : IFile
    {
        public string Content { get; set; }

        public MemoryFile(string content)
        {
            this.Content = content;
        }

        public string ReadAll() => Content;
        
        public void WriteAll(string contents) => Content = contents;
    }
}
