using System;

namespace WenigerTorbenBot.CLI;

public interface IInputHandler
{
    public void Handle(string? input);

    public int RegisterInterrupt(Action<string?> callback);
    public void ReleaseInterrupt(int id);
}