using System;
using System.Threading;

namespace WenigerTorbenBot.CLI;

public interface IInputHandler
{
    public void Handle(string? input);

    public int Interrupt(Action<string?> callback);
    public void Release(int id);
}