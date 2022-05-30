using System;

namespace WenigerTorbenBot.CLI;

public interface IInputHandler
{
    public string? ReadInput();
    public void Handle(string? input);
    public void ControlThread(Func<bool>? condition = null);
    public void ReleaseThread();
    public bool IsInControl();

    public int RegisterInterrupt(Action<string?> callback);
    public void ReleaseInterrupt(int id);
}