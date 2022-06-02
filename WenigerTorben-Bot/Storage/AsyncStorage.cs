using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Serilog;

namespace WenigerTorbenBot.Storage;

public abstract class AsyncStorage<T> : Storage<T>, IAsyncStorage<T>
{
    protected AsyncStorage(string filepath) : base(filepath)
    { }

    public async Task LoadAsync()
    {
        if (!File.Exists(filepath))
        {
            Log.Debug("No config found at {filepath}, skipped LoadAsync()", filepath);
            return;
        }

        try
        {
            Dictionary<string, T>? loadedStorage = await DoLoadAsync();
            if (loadedStorage is null)
                throw new Exception("Deserialized storage was null"); //TODO: Proper exception
            storage = loadedStorage;
        }
        catch (Exception e)
        {
            Log.Error(e, "Error while loading storage. Keeping previous state.");
        }
    }

    public async Task SaveAsync()
    {
        try
        {
            await DoSaveAsync();
        }
        catch (Exception e)
        {
            Log.Error(e, "Error while saving storage. Storage was not saved to disk.");
        }
    }

    public abstract Task<Dictionary<string, T>?> DoLoadAsync();

    public abstract Task DoSaveAsync();
}