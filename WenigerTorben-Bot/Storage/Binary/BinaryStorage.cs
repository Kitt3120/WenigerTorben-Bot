using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Serilog;

namespace WenigerTorbenBot.Storage.Binary;

public class BinaryStorage<T> : Storage<T>, IBinaryStorage<T>
{
    private readonly BinaryFormatter binaryFormatter;

    public BinaryStorage(string filepath) : base(filepath)
    {
        this.binaryFormatter = new BinaryFormatter();
    }

    protected override Dictionary<string, T>? DoLoad()
    {
        FileStream fileStream = File.OpenRead(filepath);
        object deserializedObject = binaryFormatter.Deserialize(fileStream);
        if (deserializedObject is Dictionary<string, T> loadedStorage)
            return loadedStorage;
        return null;
    }
    protected override void DoSave() => binaryFormatter.Serialize(File.OpenWrite(filepath), storage);

}