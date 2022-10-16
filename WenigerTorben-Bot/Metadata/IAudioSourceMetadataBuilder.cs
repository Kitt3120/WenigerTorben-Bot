using System;
using System.Collections.Generic;

namespace WenigerTorbenBot.Metadata;

public interface IAudioSourceMetadataBuilder
{
    public IAudioSourceMetadata Build();
    public IAudioSourceMetadataBuilder WithTitle(string? title);
    public IAudioSourceMetadataBuilder WithDescription(string? description);
    public IAudioSourceMetadataBuilder WithAuthor(string? author);
    public IAudioSourceMetadataBuilder WithDuration(TimeSpan? duration);
    public IAudioSourceMetadataBuilder WithOrigin(string? origin);
    public IAudioSourceMetadataBuilder WithTags(string[]? tags);
    public IAudioSourceMetadataBuilder WithExtras(Dictionary<string, string>? extras);
    public IAudioSourceMetadataBuilder WithFile(string? file);
}