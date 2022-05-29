using System.Threading;

namespace WenigerTorbenBot.CLI;

public interface IInputHandler
{
    public void Handle(string? input);
}