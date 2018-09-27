using System;

namespace PiENIS
{
    public interface IFileProvider : IDisposable
    {
        string[] GetLines();
        void SetLines(string contents);
    }
}
