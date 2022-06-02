using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WenigerTorbenBot.Storage.Config;

public class ConfigStorage<T> : AsyncStorage<T>
{
    public ConfigStorage(string filepath) : base(filepath)
    { }

    protected override Dictionary<string, T>? DoLoad() => JsonConvert.DeserializeObject<Dictionary<string, T>>(File.ReadAllText(filepath));

    protected override void DoSave() => File.WriteAllText(filepath, JsonConvert.SerializeObject(storage, Formatting.Indented));

    public override async Task<Dictionary<string, T>?> DoLoadAsync() => JsonConvert.DeserializeObject<Dictionary<string, T>>(await File.ReadAllTextAsync(filepath));

    public override async Task DoSaveAsync() => await File.WriteAllTextAsync(filepath, JsonConvert.SerializeObject(storage, Formatting.Indented));

}