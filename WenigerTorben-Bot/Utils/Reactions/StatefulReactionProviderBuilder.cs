using System;
using System.Collections.Generic;
using System.Linq;

namespace WenigerTorbenBot.Utils.Reactions;

public class StatefulReactionProviderBuilder
{
    private readonly Dictionary<string, IList<string>> source;

    public StatefulReactionProviderBuilder()
    {
        this.source = new Dictionary<string, IList<string>>();
    }

    public StatefulReactionProviderBuilder WithReaction(string pattern, string reaction)
    {
        if (source.Values.SelectMany(reactions => reactions).Contains(reaction))
            throw new ArgumentException($"Reaction {reaction} already registered");

        IList<string> reactions;
        if (source.ContainsKey(pattern))
            reactions = source[pattern];
        else
        {
            reactions = new List<string>();
            source[pattern] = reactions;
        }

        reactions.Add(reaction);
        return this;
    }

    public StatefulReactionProvider Build()
    {
        Dictionary<string, IList<string>> copyOfSource = new Dictionary<string, IList<string>>();
        foreach (string key in source.Keys)
        {
            copyOfSource[key] = new List<string>();
            foreach (string pattern in source[key])
                copyOfSource[key].Add(pattern);
        }

        return new StatefulReactionProvider(copyOfSource);
    }
}