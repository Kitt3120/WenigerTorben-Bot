using System;
using System.Collections.Generic;
using System.Linq;

namespace WenigerTorbenBot.Utils.Reactions;

public class StatefulReactionProvider
{
    private readonly Dictionary<string, IList<string>> source;

    public StatefulReactionProvider(Dictionary<string, IList<string>> source)
    {
        this.source = source;
    }

    public string[] GetAvailablePatterns() => source.Keys.ToArray();

    public string Consume(string pattern)
    {

        if (!source.ContainsKey(pattern))
            throw new ArgumentException($"No reactions registered for given pattern {pattern}");

        if (source[pattern].Count == 0)
            throw new ArgumentException($"No reactions left to consume for given pattern {pattern}");

        IList<string> reactions = source[pattern];
        string reaction = reactions.First();
        reactions.RemoveAt(0);

        return reaction;
    }

    public int CountFor(string pattern)
    {
        if (!source.ContainsKey(pattern))
            return 0;
        else
            return source[pattern].Count;
    }

    public bool HasReactionsLeft() => GetAvailablePatterns().Any(pattern => CountFor(pattern) > 0);
}