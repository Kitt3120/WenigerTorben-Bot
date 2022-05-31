using System.Collections.Generic;
using System.Threading.Tasks;
using WenigerTorbenBot.Storage.Config;

namespace WenigerTorbenBot.Services.Config;

public interface IConfigService : IService
{
    public string GetConfigsDirectory();

    public string GetConfigFilePath(string guildId = "global");

    public IEnumerable<string> GetGuildIds();

    public bool Exists(string guildId = "global");

    public IConfig Get(string guildId = "global");

    public void Delete(string guildId);

    public void Load(string guildId = "global");

    public Task LoadAsync(string guildId = "global");

    public void LoadAll();

    public Task LoadAllAsync();

    public void Save(string guildId = "global");

    public Task SaveAsync(string guildId = "global");

    public void SaveAll();

    public Task SaveAllAsync();

}