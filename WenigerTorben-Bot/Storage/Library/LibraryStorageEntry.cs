using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using WenigerTorbenBot.Metadata;

namespace WenigerTorbenBot.Storage.Library;

public class LibraryStorageEntry<T>
{
    //These have to be public readonly properties to be seen as data by the JSON serializer
    public string Path { get; }
    public Metadata.Metadata Metadata { get; }

    private readonly BinaryFormatter binaryFormatter;

    public LibraryStorageEntry(string path, Metadata.Metadata metadata)
    {
        Path = path;
        Metadata = metadata;

        this.binaryFormatter = new BinaryFormatter();
    }

    //TODO: Move ReadAsync() and WriteAsync() to LibraryStorage and maybe just wrap here

    public async Task<T?> ReadAsync()
    {
        FileStream fileStream = File.OpenRead(Path);
        object deserializedObject = binaryFormatter.Deserialize(fileStream);
        await fileStream.DisposeAsync();
        if (deserializedObject is T loadedEntry)
            return loadedEntry;
        return default;
    }

    public async Task WriteAsync(T data)
    {
        FileStream fileStream = File.OpenWrite(Path);
        binaryFormatter.Serialize(fileStream, data);
        await fileStream.DisposeAsync();
    }

    internal void Delete() => File.Delete(Path);

}