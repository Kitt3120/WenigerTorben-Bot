using WenigerTorbenBot.Services.File;
using WenigerTorbenBot.Services.Storage.Library;

namespace WenigerTorbenBot.Storage.Audio;

public class AudioStorageService : LibraryStorageService<byte[]>, IAudioStorageService
{
    public AudioStorageService(IFileService fileService, string? customDirectory = null) : base(fileService, customDirectory)
    { }

    public override string Name => "AudioStorageService";
    public override string GetDefaultDirectory() => "Audios";
}