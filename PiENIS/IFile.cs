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
}
