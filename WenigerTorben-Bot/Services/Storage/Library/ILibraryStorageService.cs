using WenigerTorbenBot.Storage.Config;
using WenigerTorbenBot.Storage.Library;

namespace WenigerTorbenBot.Services.Storage.Library;

public interface ILibraryStorageService<T> : IConfigStorageService<LibraryStorageEntry<T>>
{

}