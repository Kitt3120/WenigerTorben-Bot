using System;
using System.IO;
using WenigerTorbenBot.Storage;
using WenigerTorbenBot.Utils;

namespace WenigerTorbenBot;
public class Program
{
    public static Config config { get; private set; }
    public static void Main(string[] args)
    {
        FileUtils.GenerateDirectories();
        config = new Config();
    }
}


