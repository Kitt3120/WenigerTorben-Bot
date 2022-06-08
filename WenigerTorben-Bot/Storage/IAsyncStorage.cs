using System.Threading.Tasks;

namespace WenigerTorbenBot.Storage;

public interface IAsyncStorage<T> : IStorage<T>
{
    public Task LoadAsync();
    public Task SaveAsync();
}