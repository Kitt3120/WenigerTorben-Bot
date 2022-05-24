using System.Threading.Tasks;

namespace WenigerTorbenBot.Services.Config;

public interface IConfigService
{
    public bool Exists(string key);

    public object Get(string key);

    public T Get<T>(string key);

    public object this[string key]
    {
        get;
        set;
    }

    public void Set(string key, object value);

    public void Set<T>(string key, T value);

    public object GetOrSet(string key, object defaultValue);

    public T GetOrSet<T>(string key, T defaultValue);

    public void Remove(string key);

    public void Load();

    public Task LoadAsync();

    public void Save();

    public Task SaveAsync();
}