namespace WenigerTorbenBot.Storage;

public interface IStorage<T>
{
    public bool Exists(string key);

    public T? Get(string key);

    public G? Get<G>(string key);

    public T? this[string key]
    {
        get;
        set;
    }

    public void Set(string key, T? value);

    public T GetOrSet(string key, T defaultValue);

    public void Remove(string key);

    public void Load();

    public void Save();

    public void Delete();

}