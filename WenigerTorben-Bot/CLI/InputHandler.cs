using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Serilog;
using WenigerTorbenBot.CLI.Command;

namespace WenigerTorbenBot.CLI;

public class InputHandler : IInputHandler
{

    private readonly List<ICommand> commands;
    private readonly ConcurrentDictionary<int, Action<string?>> interrupts;

    private bool inControl;
    private readonly object controlLock;

    public InputHandler()
    {
        commands = new List<ICommand>();
        interrupts = new ConcurrentDictionary<int, Action<string?>>();
        inControl = false;
        controlLock = new object();

        Log.Debug("Registering CLI commands");
        foreach (Type type in AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes()).Where(assemblyType => !assemblyType.IsInterface && typeof(ICommand).IsAssignableFrom(assemblyType)))
        {
            Log.Debug("Found ICommand implementing type: {type}", type.Name);

            ConstructorInfo[] constructorInfos = type.GetConstructors();
            if (constructorInfos.Length == 0)
            {
                Log.Warning("Tried to construct CLI command {type} but it has no constructor", type.Name);
                continue;
            }

            bool foundConstructor = false;
            List<object> parameters = new List<object>();

            foreach (ConstructorInfo constructorInfo in constructorInfos)
            {
                bool successfully = true;
                ParameterInfo[] parameterInfos = constructorInfo.GetParameters();
                foreach (ParameterInfo parameterInfo in parameterInfos)
                {
                    object? service = DI.ServiceProvider.GetService(parameterInfo.ParameterType);
                    if (service is null)
                    {
                        successfully = false;
                        break;
                    }

                    parameters.Add(service);
                }

                if (successfully)
                {
                    foundConstructor = true;
                    break;
                }
                else
                    parameters.Clear();
            }

            if (!foundConstructor)
            {
                Log.Error("Tried to create instance of CLI command {type} but none of its constructors could be satisfied.", type.Name);
                continue;
            }

            object? obj = Activator.CreateInstance(type, parameters.ToArray());

            if (obj is null)
            {
                Log.Error("Tried to create instance of CLI command {type} but returned object was null.", type.Name);
                continue;
            }

            if (obj is not ICommand command)
            {
                Log.Error("Tried to create instance of CLI command {type} but returned object was not an ICommand.", type.Name);
                continue;
            }

            commands.Add(command);
        }
    }

    public string? ReadInput() => Console.ReadLine();

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

        IEnumerable<ICommand> foundCommands = commands.Where(command => command.Name.ToLower() == cmd.ToLower() || command.Aliases.Any(alias => alias.ToLower() == cmd.ToLower()));
        if (!foundCommands.Any())
        {
            Console.WriteLine($"Command {cmd} not found");
            return;
        }

        foreach (ICommand command in foundCommands)
        {
            try
            {
                command.OnCommand(args);
            }
            catch (Exception e)
            {
                Log.Error(e, "Exception while handling command {command}", command.Name);
            }
        }
    }

    public void ControlThread(Func<bool>? condition = null)
    {
        lock (controlLock)
        {
            if (inControl)
                return;
            else
                inControl = true;
        }

        bool lastConditionState = true;
        while (inControl && lastConditionState)
        {
            if (condition is not null)
                lastConditionState = condition.Invoke();

            if (lastConditionState)
                Handle(ReadInput());
        }

        if (!lastConditionState) //If lastConditionState is false, loop probably ended with inControl = true
            lock (controlLock)
                inControl = false;
    }

    public void ReleaseThread()
    {
        lock (controlLock)
            inControl = false;
    }

    public bool IsInControl()
    {
        lock (controlLock)
            return inControl;
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