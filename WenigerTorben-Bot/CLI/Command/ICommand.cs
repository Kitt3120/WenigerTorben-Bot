namespace WenigerTorbenBot.CLI.Command;

public interface ICommand
{
    public string Name { get; }
    public string[] Aliases { get; }

    public void OnCommand(string[] args);
}