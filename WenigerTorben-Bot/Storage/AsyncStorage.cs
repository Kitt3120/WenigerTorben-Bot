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
            Log.Debug("No storage found at {filepath}, skipped LoadAsync()", filepath);
            return;
        }

        try
        {
            Dictionary<string, T>? loadedStorage = await DoLoadAsync();
            if (loadedStorage is null)
            {
                Log.Error("Failed to load storage {filepath}: Deserialized storage was null. Keeping previous state.", filepath);
                return;
            }
            storage = loadedStorage;
        }
        catch (Exception e)
        {
            Log.Error(e, "Error while loading storage {filepath}. Keeping previous state.", filepath);
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
            Log.Error(e, "Error while saving storage {filepath}. Storage was not saved to disk.", filepath);
        }
    }

    public abstract Task<Dictionary<string, T>?> DoLoadAsync();

    public abstract Task DoSaveAsync();
}