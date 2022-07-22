using WenigerTorbenBot.Storage.Config;

namespace WenigerTorbenBot.Storage.Library;

public interface ILibraryStorage<T> : IConfigStorage<ILibraryStorageEntry>
{ }