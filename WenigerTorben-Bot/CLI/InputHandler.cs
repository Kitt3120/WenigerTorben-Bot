using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WenigerTorbenBot.CLI.Command;

namespace WenigerTorbenBot.CLI;

public class InputHandler : IInputHandler
{

    private readonly List<ICommand> commands;
    private readonly ConcurrentDictionary<int, Action<string?>> interrupts;

    public InputHandler()
    {
        commands = new List<ICommand>();
        interrupts = new ConcurrentDictionary<int, Action<string?>>();

        foreach (Type type in AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes()).Where(assemblyType => !assemblyType.IsInterface && typeof(ICommand).IsAssignableFrom(assemblyType)))
        {
            ConstructorInfo[] constructorInfos = type.GetConstructors();
            List<object> parameters = new List<object>();
            bool successfully = true;

            if (constructorInfos.Length > 0)
            {
                ParameterInfo[] parameterInfos = constructorInfos[0].GetParameters();
                foreach (ParameterInfo parameterInfo in parameterInfos)
                {
                    object? obj = DI.ServiceProvider.GetService(parameterInfo.ParameterType);
                    if (obj is null)
                        successfully = false;
                    else
                        parameters.Add(obj);
                }
            }

            if (successfully)
            {
                object? obj = Activator.CreateInstance(type, parameters.ToArray());
                if (obj is not null && obj is ICommand command)
                    commands.Add(command);
            }
        }
    }

    public void Handle(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return;

        if (!interrupts.IsEmpty)
        {
            int id = interrupts.Keys.Min();
            interrupts[id].Invoke(input);
            return;
        }

        string cmd = input.Split(" ")[0];
        string[] args = input.Replace(cmd, "").Trim().Split(" ");

        IEnumerable<ICommand> foundCommands = commands.Where(command => command.Name.ToLower() == input.ToLower() || command.Aliases.Any(alias => alias.ToLower() == input.ToLower()));
        if (!foundCommands.Any())
        {
            Console.WriteLine($"Command {cmd} not found");
            return;
        }

        foreach (ICommand command in foundCommands)
            try
            {
                command.OnCommand(args);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception while handling command {command.Name}: {e.Message}");
            }
    }

    public int RegisterInterrupt(Action<string?> callback)
    {
        int id = 0;
        if (!interrupts.IsEmpty)
            id = interrupts.Keys.Max() + 1;
        interrupts.TryAdd(id, callback);
        return id;
    }

    public void ReleaseInterrupt(int key) => interrupts.TryRemove(key, out Action<string>? removedItem);
}