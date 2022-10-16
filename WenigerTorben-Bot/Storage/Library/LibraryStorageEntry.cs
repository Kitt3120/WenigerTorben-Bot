using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace WenigerTorbenBot.Storage.Library;

public class LibraryStorageEntry<T>
{
    //These have to be public readonly properties to be seen as data by the JSON serializer

    //TODO: Replace with IAudioSourceMetadata
    public string Id { get; private set; }
    public string Title { get; private set; }
    public string? Description { get; private set; }
    public string[]? Tags { get; private set; }
    public Dictionary<string, string>? Extras { get; private set; }
    public string File { get; private set; }

    private readonly BinaryFormatter binaryFormatter;

    public LibraryStorageEntry(string title, string? description, string[]? tags, Dictionary<string, string>? extras, string file)
    {
        this.Id = Guid.NewGuid().ToString();
        this.Title = title;
        this.Description = description;
        this.Tags = tags;
        this.Extras = extras;
        this.File = file;

        this.binaryFormatter = new BinaryFormatter();
    }

    public string GetTitle() => Title;

    public string? GetDescription() => Description;

    public string[]? GetTags() => Tags;

    public Dictionary<string, string>? GetExtras() => Extras;

    public string GetFile() => File;

    //TODO: Move ReadAsync() and WriteAsync() to LibraryStorage and maybe just wrap here

    public async Task<T?> ReadAsync()
    {
        FileStream fileStream = System.IO.File.OpenRead(File);
        object deserializedObject = binaryFormatter.Deserialize(fileStream);
        await fileStream.DisposeAsync();
        if (deserializedObject is T loadedEntry)
            return loadedEntry;
        return default;
    }
    public async Task WriteAsync(T data)
    {
        FileStream fileStream = System.IO.File.OpenWrite(File);
        binaryFormatter.Serialize(fileStream, data);
        await fileStream.DisposeAsync();
    }

    internal void Delete() => System.IO.File.Delete(File);

}