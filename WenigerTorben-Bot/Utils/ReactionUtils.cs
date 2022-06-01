using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using WenigerTorbenBot.Utils.Reactions;

namespace WenigerTorbenBot.Utils;

public class ReactionUtils
{
    public static string[] FromString(string str, StatefulReactionProvider? statefulReactionProvider = null)
    {
        if (statefulReactionProvider is null)
            statefulReactionProvider = defaultStatefulReactionProviderBuilder.Build();

        string[] patterns = statefulReactionProvider.GetAvailablePatterns().OrderByDescending(pattern => pattern.Length).ToArray();

        string filtered = str;
        foreach (string pattern in patterns)
        {
            for (int i = 0; i < statefulReactionProvider.CountFor(pattern); i++)
            {
                int pos = filtered.IndexOf(pattern);
                if (pos >= 0)
                    filtered = string.Concat(filtered.AsSpan(0, pos), string.Empty, filtered.AsSpan(pos + pattern.Length));
            }
        }

        if (filtered.Length > 0)
            throw new ArgumentException("The given string would have unresolved leftovers after resolving with all available patterns");

        string resolved = str;
        foreach (string pattern in patterns)
        {
            for (int i = 0; i < statefulReactionProvider.CountFor(pattern); i++)
            {
                int pos = resolved.IndexOf(pattern);
                if (pos >= 0)
                    resolved = string.Concat(resolved.AsSpan(0, pos), statefulReactionProvider.Consume(pattern), resolved.AsSpan(pos + pattern.Length));
            }
        }

        resolved = resolved[1..];
        resolved = resolved[0..^1];
        return resolved.Split("::").Select(reaction => $":{reaction}:").ToArray();
    }

    private static readonly StatefulReactionProviderBuilder defaultStatefulReactionProviderBuilder = new StatefulReactionProviderBuilder()
    .WithReaction("wc", ":wc")
    .WithReaction("ok", ":ok:")
    .WithReaction("cool", ":cool:")
    .WithReaction("new", ":new:")
    .WithReaction("free", ":free:")
    .WithReaction(" ", ":black_small_square:")
    .WithReaction(" ", ":black_medium_square:")
    .WithReaction(" ", ":black_medium_small_square:")
    .WithReaction(" ", ":black_large_square:");
}