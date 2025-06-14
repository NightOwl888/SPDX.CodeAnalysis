using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SPDX.CodeAnalysis
{
    public interface IFileSystem
    {
        bool DirectoryExists(string path);
        IEnumerable<string> EnumerateFiles(string path, string pattern);
        IEnumerable<string> EnumerateDirectories(string path, string pattern);
        TextReader OpenText(string path);
    }
}
