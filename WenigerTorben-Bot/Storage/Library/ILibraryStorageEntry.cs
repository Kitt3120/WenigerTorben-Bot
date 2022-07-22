using System.Collections.Generic;

namespace WenigerTorbenBot.Storage.Library;

public interface ILibraryStorageEntry
{
    public string GetTitle();
    public string? GetDescription();
    public string[]? GetTags();
    public Dictionary<string, string>? GetExtras();
    public string GetFile();
}