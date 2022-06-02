using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Microsoft.VisualBasic;

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
            throw new ArgumentException($"No reactions available for given pattern {pattern}");

        IList<string> reactions = source[pattern];
        string reaction = reactions.First();
        reactions.RemoveAt(0);

        if (reactions.Count == 0)
            source.Remove(pattern);

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