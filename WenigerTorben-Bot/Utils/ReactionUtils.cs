using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Serilog;
using WenigerTorbenBot.Utils.Reactions;

namespace WenigerTorbenBot.Utils;

public class ReactionUtils
{
    public static string[] FromString(string str, StatefulReactionProvider? statefulReactionProvider = null)
    {
        Log.Debug("Resolving pattern {pattern}", str);

        if (statefulReactionProvider is null)
            statefulReactionProvider = defaultStatefulReactionProviderBuilder.Build();

        List<string> reactionStrings = new List<string>();
        string toAnalyze = str;
        while (toAnalyze.Length > 0 && statefulReactionProvider.GetAvailablePatterns().Any())
        {
            string[] availablePatterns = statefulReactionProvider.GetAvailablePatterns();
            string pattern = string.Empty;
            for (int i = 0; i < toAnalyze.Length; i++)
            {
                pattern += toAnalyze[i];
                string[] newPatterns = availablePatterns.Where(availablePattern => availablePattern.StartsWith(pattern)).ToArray();

                if (newPatterns.Length >= 2)
                    availablePatterns = newPatterns;
                else if (newPatterns.Length == 1)
                {
                    string foundPattern = newPatterns[0];
                    reactionStrings.Add(statefulReactionProvider.Consume(foundPattern));

                    int pos = toAnalyze.IndexOf(foundPattern);
                    toAnalyze = string.Concat(toAnalyze.AsSpan(0, pos), toAnalyze.AsSpan(pos + foundPattern.Length));
                    pattern = string.Empty;
                    break;
                }
                else
                {
                    pattern = pattern[0..^1];

                    string? foundPattern = null;
                    foreach (string p in availablePatterns)
                    {
                        if (p == pattern)
                        {
                            foundPattern = p;
                            break;
                        }
                    }

                    if (foundPattern is null)
                        throw new ArgumentException($"Unable to resolve pattern {pattern}");

                    reactionStrings.Add(statefulReactionProvider.Consume(foundPattern));

                    int pos = toAnalyze.IndexOf(foundPattern);
                    toAnalyze = string.Concat(toAnalyze.AsSpan(0, pos), toAnalyze.AsSpan(pos + foundPattern.Length));
                    pattern = string.Empty;
                    break;
                }
            }

            if (!string.IsNullOrEmpty(pattern))
            {
                string? foundPattern = null;
                foreach (string p in availablePatterns)
                {
                    if (p == pattern)
                    {
                        foundPattern = p;
                        break;
                    }
                }

                if (foundPattern is null)
                    throw new ArgumentException($"Unable to resolve pattern {pattern}");

                reactionStrings.Add(statefulReactionProvider.Consume(foundPattern));

                int pos = toAnalyze.IndexOf(foundPattern);
                toAnalyze = string.Concat(toAnalyze.AsSpan(0, pos), toAnalyze.AsSpan(pos + foundPattern.Length));
                pattern = string.Empty;
            }
        }

        if (toAnalyze.Length > 0)
            throw new ArgumentException("The given string would have unresolved leftovers after resolving with all available patterns");

        return reactionStrings.ToArray();
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
    .WithReaction(" ", ":black_large_square:")
    .WithReaction("X", ":x:")
    .WithReaction("a", ":regional_indicator_a:")
    .WithReaction("b", ":regional_indicator_b:")
    .WithReaction("c", ":regional_indicator_c:")
    .WithReaction("d", ":regional_indicator_d:")
    .WithReaction("e", ":regional_indicator_e:")
    .WithReaction("f", ":regional_indicator_f:")
    .WithReaction("g", ":regional_indicator_g:")
    .WithReaction("h", ":regional_indicator_h:")
    .WithReaction("i", ":regional_indicator_i:")
    .WithReaction("j", ":regional_indicator_j:")
    .WithReaction("k", ":regional_indicator_k:")
    .WithReaction("l", ":regional_indicator_l:")
    .WithReaction("m", ":regional_indicator_m:")
    .WithReaction("n", ":regional_indicator_n:")
    .WithReaction("o", ":regional_indicator_o:")
    .WithReaction("p", ":regional_indicator_p:")
    .WithReaction("q", ":regional_indicator_q:")
    .WithReaction("r", ":regional_indicator_r:")
    .WithReaction("s", ":regional_indicator_s:")
    .WithReaction("t", ":regional_indicator_t:")
    .WithReaction("u", ":regional_indicator_u:")
    .WithReaction("v", ":regional_indicator_v:")
    .WithReaction("w", ":regional_indicator_w:")
    .WithReaction("x", ":regional_indicator_x:")
    .WithReaction("y", ":regional_indicator_y:")
    .WithReaction("z", ":regional_indicator_z:")
    ;
}