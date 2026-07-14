using System;
using System.Collections.Generic;

public static class PokerHandEvaluator
{
    private readonly struct PokerLineCard
    {
        public CardLineCard LineCard { get; }
        public PokerCardProfile Profile { get; }

        public PokerLineCard(CardLineCard lineCard, PokerCardProfile profile)
        {
            LineCard = lineCard;
            Profile = profile;
        }
    }

    public static void FindMatches(
        CardLine line,
        int requiredCardCount,
        ICollection<CardHandMatch> matches,
        Action<
            CardLine,
            IReadOnlyList<CardLineCard>,
            IReadOnlyList<PokerCardProfile>,
            ICollection<CardHandMatch>
        > evaluate
    )
    {
        List<PokerLineCard> segment = new();

        foreach (CardLineCard lineCard in line.Cards)
        {
            if (lineCard.Card is IPokerHandCard pokerCard)
            {
                if (pokerCard.PokerHandParticipation == PokerHandParticipationMode.Transparent) continue;

                if (pokerCard.PokerHandParticipation == PokerHandParticipationMode.Participant)
                {
                    PokerCardProfile profile = pokerCard.PokerProfile;

                    if (profile.IsValid)
                    {
                        segment.Add(new PokerLineCard(lineCard, profile));
                        continue;
                    }
                }
            }

            FindMatchesInSegment(line, segment, requiredCardCount, matches, evaluate);
            segment.Clear();
        }

        FindMatchesInSegment(line, segment, requiredCardCount, matches, evaluate);
    }

    public static bool HasCommonRank(IReadOnlyList<PokerCardProfile> cards)
    {
        int commonRanks = PokerCardProfile.AllRankMask;

        foreach (PokerCardProfile card in cards) commonRanks &= card.RankMask;

        return commonRanks != 0;
    }

    public static bool HasCommonPattern(IReadOnlyList<PokerCardProfile> cards)
    {
        int commonPatterns = PokerCardProfile.AllPatternMask;

        foreach (PokerCardProfile card in cards) commonPatterns &= card.PatternMask;

        return commonPatterns != 0;
    }

    public static bool CanMatchRankCounts(
        IReadOnlyList<PokerCardProfile> cards,
        IReadOnlyList<int> targetCounts
    ) => CanMatchCounts(cards, targetCounts, true);

    public static bool CanMatchPatternCounts(
        IReadOnlyList<PokerCardProfile> cards,
        IReadOnlyList<int> targetCounts
    ) => CanMatchCounts(cards, targetCounts, false);

    public static bool CanMatchFullHouse(
        IReadOnlyList<PokerCardProfile> cards,
        IReadOnlyList<int> triplePatternCounts,
        IReadOnlyList<int> pairPatternCounts
    )
    {
        PokerCardProfile[] triple = new PokerCardProfile[3];
        PokerCardProfile[] pair = new PokerCardProfile[2];

        for (int first = 0; first < cards.Count - 2; first++)
        {
            for (int second = first + 1; second < cards.Count - 1; second++)
            {
                for (int third = second + 1; third < cards.Count; third++)
                {
                    triple[0] = cards[first];
                    triple[1] = cards[second];
                    triple[2] = cards[third];

                    int pairIndex = 0;

                    for (int i = 0; i < cards.Count; i++)
                    {
                        if (i == first || i == second || i == third) continue;

                        pair[pairIndex] = cards[i];
                        pairIndex++;
                    }

                    int tripleRanks = triple[0].RankMask & triple[1].RankMask & triple[2].RankMask;
                    int pairRanks = pair[0].RankMask & pair[1].RankMask;

                    if (!CanChooseDistinctValues(tripleRanks, pairRanks)) continue;
                    if (!CanMatchPatternCounts(triple, triplePatternCounts)) continue;
                    if (!CanMatchPatternCounts(pair, pairPatternCounts)) continue;

                    return true;
                }
            }
        }

        return false;
    }

    public static bool CanAssignRanks(
        IReadOnlyList<PokerCardProfile> cards,
        IReadOnlyList<int> ranks
    ) => CanAssignRanks(cards, ranks, 0, 0);

    private static void FindMatchesInSegment(
        CardLine line,
        IReadOnlyList<PokerLineCard> segment,
        int requiredCardCount,
        ICollection<CardHandMatch> matches,
        Action<
            CardLine,
            IReadOnlyList<CardLineCard>,
            IReadOnlyList<PokerCardProfile>,
            ICollection<CardHandMatch>
        > evaluate
    )
    {
        if (segment.Count < requiredCardCount) return;

        CardLineCard[] selectedCards = new CardLineCard[requiredCardCount];
        PokerCardProfile[] selectedProfiles = new PokerCardProfile[requiredCardCount];

        for (int start = 0; start <= segment.Count - requiredCardCount; start++)
        {
            for (int i = 0; i < requiredCardCount; i++)
            {
                selectedCards[i] = segment[start + i].LineCard;
                selectedProfiles[i] = segment[start + i].Profile;
            }

            evaluate(line, selectedCards, selectedProfiles, matches);
        }
    }

    private static bool CanMatchCounts(
        IReadOnlyList<PokerCardProfile> cards,
        IReadOnlyList<int> targetCounts,
        bool useRanks
    )
    {
        int[] remainingCounts = new int[targetCounts.Count];
        int[] groupMasks = new int[targetCounts.Count];

        for (int i = 0; i < targetCounts.Count; i++) remainingCounts[i] = targetCounts[i];

        return CanMatchCounts(cards, remainingCounts, groupMasks, useRanks, 0);
    }

    private static bool CanMatchCounts(
        IReadOnlyList<PokerCardProfile> cards,
        int[] remainingCounts,
        int[] groupMasks,
        bool useRanks,
        int cardIndex
    )
    {
        if (cardIndex == cards.Count) return CanAssignDistinctValues(groupMasks, 0, 0);

        int cardMask = useRanks ? cards[cardIndex].RankMask : cards[cardIndex].PatternMask;

        for (int groupIndex = 0; groupIndex < remainingCounts.Length; groupIndex++)
        {
            if (remainingCounts[groupIndex] == 0) continue;

            int previousMask = groupMasks[groupIndex];
            int commonMask = previousMask == 0 ? cardMask : previousMask & cardMask;

            if (commonMask == 0) continue;

            groupMasks[groupIndex] = commonMask;
            remainingCounts[groupIndex]--;

            if (CanMatchCounts(cards, remainingCounts, groupMasks, useRanks, cardIndex + 1)) return true;

            remainingCounts[groupIndex]++;
            groupMasks[groupIndex] = previousMask;
        }

        return false;
    }

    private static bool CanAssignDistinctValues(int[] groupMasks, int groupIndex, int usedValues)
    {
        if (groupIndex == groupMasks.Length) return true;

        int availableValues = groupMasks[groupIndex] & ~usedValues;

        while (availableValues != 0)
        {
            int value = availableValues & -availableValues;

            if (CanAssignDistinctValues(groupMasks, groupIndex + 1, usedValues | value)) return true;

            availableValues &= ~value;
        }

        return false;
    }

    private static bool CanChooseDistinctValues(int firstValues, int secondValues)
    {
        int availableFirstValues = firstValues;

        while (availableFirstValues != 0)
        {
            int firstValue = availableFirstValues & -availableFirstValues;

            if ((secondValues & ~firstValue) != 0) return true;

            availableFirstValues &= ~firstValue;
        }

        return false;
    }

    private static bool CanAssignRanks(
        IReadOnlyList<PokerCardProfile> cards,
        IReadOnlyList<int> ranks,
        int cardIndex,
        int usedRankIndices
    )
    {
        if (cardIndex == cards.Count) return true;

        for (int rankIndex = 0; rankIndex < ranks.Count; rankIndex++)
        {
            int rankIndexMask = 1 << rankIndex;

            if ((usedRankIndices & rankIndexMask) != 0) continue;

            ushort rankMask = PokerCardProfile.GetRankMask(ranks[rankIndex]);

            if ((cards[cardIndex].RankMask & rankMask) == 0) continue;
            if (CanAssignRanks(cards, ranks, cardIndex + 1, usedRankIndices | rankIndexMask)) return true;
        }

        return false;
    }
}
