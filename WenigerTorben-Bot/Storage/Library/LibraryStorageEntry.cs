using System.Collections.Generic;

namespace WenigerTorbenBot.Storage.Library;

public class LibraryStorageEntry : ILibraryStorageEntry
{
    private readonly string title;
    private readonly string? description;
    private readonly string[]? tags;
    private readonly Dictionary<string, string>? extras;
    private readonly string file;

    public LibraryStorageEntry(string title, string? description, string[]? tags, Dictionary<string, string>? extras, string file)
    {
        this.title = title;
        this.description = description;
        this.tags = tags;
        this.extras = extras;
        this.file = file;
    }

    public string GetTitle() => title;

    public string? GetDescription() => description;

    public string[]? GetTags() => tags;

    public Dictionary<string, string>? GetExtras() => extras;

    public string GetFile() => file;
}