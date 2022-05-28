/*
WenigerTorben-Bot - Der WenigerTorben-Bot für den FHDW Discord Server der Bergischen Banausen
Copyright(C) 2022  Torben Schweren

This program is free software: you can redistribute it and / or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.If not, see < https://www.gnu.org/licenses/>.
*/

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using WenigerTorbenBot.Services;
using WenigerTorbenBot.Services.Config;
using WenigerTorbenBot.Services.Discord;
using WenigerTorbenBot.Services.Health;
using WenigerTorbenBot.Utils;

namespace WenigerTorbenBot;
public class Program
{

    public static void Main()
    {
        MainAsync().GetAwaiter().GetResult();
    }

    public static async Task MainAsync()
    {
        PrintLicense();
        Console.WriteLine("\n");

        DI.Init();

        foreach(Service service in ServiceRegistry.GetServices())
            service.Start();

        IHealthService? healthService = ServiceRegistry.Get<IHealthService>();
        if(healthService is null || !healthService.IsOverallHealthGood())
        {
            Console.WriteLine("Some essential service(s) were not able to initialize successfully. Shutting down.");
            Environment.Exit(1);
        }

        Console.ReadKey();
        
        foreach(Service service in ServiceRegistry.GetServices().Reverse())
            await service.Stop();
    }

    public static void PrintLicense()
    {
        Console.WriteLine($"WenigerTorben-Bot - Der WenigerTorben-Bot für den FHDW Discord Server der Bergischen Banausen{Environment.NewLine}" +
        $"Copyright(C) 2022  Torben Schweren{Environment.NewLine}" +
        $"{Environment.NewLine}" +
        $"This program is free software: you can redistribute it and / or modify{Environment.NewLine}" +
        $"it under the terms of the GNU General Public License as published by{Environment.NewLine}" +
        $"the Free Software Foundation, either version 3 of the License, or{Environment.NewLine}" +
        $"(at your option) any later version.{Environment.NewLine}" +
        $"{Environment.NewLine}" +
        $"This program is distributed in the hope that it will be useful,{Environment.NewLine}" +
        $"but WITHOUT ANY WARRANTY; without even the implied warranty of{Environment.NewLine}" +
        $"MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the{Environment.NewLine}" +
        $"GNU General Public License for more details.{Environment.NewLine}" +
        $"{Environment.NewLine}" +
        $"You should have received a copy of the GNU General Public License{Environment.NewLine}" +
        $"along with this program.If not, see < https://www.gnu.org/licenses/>.");
    }
}


