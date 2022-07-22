using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Serilog;
using WenigerTorbenBot.Storage.Config;

namespace WenigerTorbenBot.Storage.Library;

public class LibraryStorage<T> : ConfigStorage<ILibraryStorageEntry>, ILibraryStorage<T>
{
    private readonly string directoryPath;

    public LibraryStorage(string filepath) : base(filepath)
    {
        directoryPath = Path.GetDirectoryName(filepath);
        if (directoryPath is null)
        {
            directoryPath = string.Empty;
            throw new ArgumentException($"Could not get parent directory of {filepath}");
        }
    }
}