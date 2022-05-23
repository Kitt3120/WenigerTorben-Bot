using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WenigerTorbenBot.Storage;
using WenigerTorbenBot.Utils;

namespace WenigerTorbenBot;
public class Program
{
    public static Config config { get; private set; }
    public static void Main(string[] args)
    {
        new Program().Init(args).GetAwaiter().GetResult();
    }

    public async Task Init(string[] args)
    {
        FileUtils.GenerateDirectories();

        try
        {
            config = new Config();
        }
        catch (JsonSerializationException e)
        {
            Console.WriteLine($"There was an error while reading the config file: {e.Message}.{Environment.NewLine}Fix any errors in the config file or delete it to generate a new one.");
            Environment.Exit(1);
        }
        await config.SaveAsync();
    }
}


