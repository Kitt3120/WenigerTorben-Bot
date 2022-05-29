using System;

namespace WenigerTorbenBot.CLI.Command;

public class ExitCommand : ICommand
{
    public string Name => "Exit";

    public string[] Aliases => Array.Empty<string>();

    public void OnCommand(string[] args) => Program.Shutdown();
}