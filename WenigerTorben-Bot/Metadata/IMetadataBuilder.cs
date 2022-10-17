using System;
using System.Collections.Generic;

namespace WenigerTorbenBot.Metadata;

public interface IMetadataBuilder
{
    public IMetadata Build();
    public IMetadataBuilder WithID(string? id);
    public IMetadataBuilder WithTitle(string? title);
    public IMetadataBuilder WithDescription(string? description);
    public IMetadataBuilder WithAuthor(string? author);
    public IMetadataBuilder WithDuration(TimeSpan? duration);
    public IMetadataBuilder WithOrigin(string? origin);
    public IMetadataBuilder WithTags(string[]? tags);
    public IMetadataBuilder WithExtras(Dictionary<string, string>? extras);
}