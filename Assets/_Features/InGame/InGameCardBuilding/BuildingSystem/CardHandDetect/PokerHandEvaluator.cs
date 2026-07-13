using System.Collections.Generic;

public static class PokerHandEvaluator
{
    private static readonly int[][] StraightRanks =
    {
        new[] { 1, 2, 3, 4, 5 },
        new[] { 2, 3, 4, 5, 6 },
        new[] { 3, 4, 5, 6, 7 },
        new[] { 4, 5, 6, 7, 8 },
        new[] { 5, 6, 7, 8, 9 },
        new[] { 6, 7, 8, 9, 10 },
        new[] { 7, 8, 9, 10, 11 },
        new[] { 8, 9, 10, 11, 12 },
        new[] { 9, 10, 11, 12, 13 },
        new[] { 10, 11, 12, 13, 1 }
    };

    private static readonly int[] RoyalRanks = { 10, 11, 12, 13, 1 };

    public static bool IsMatch(PokerHandType handType, IReadOnlyList<PokerCardProfile> cards)
    {
        return handType switch
        {
            PokerHandType.HighCard => IsHighCard(cards),
            PokerHandType.OnePair => HasCommonRank(cards),
            PokerHandType.TwoPair => IsTwoPair(cards),
            PokerHandType.ThreeOfAKind => HasCommonRank(cards),
            PokerHandType.Straight => IsStraight(cards),
            PokerHandType.Flush => HasCommonPattern(cards),
            PokerHandType.FullHouse => IsFullHouse(cards),
            PokerHandType.FourOfAKind => HasCommonRank(cards),
            PokerHandType.StraightFlush => HasCommonPattern(cards) && IsStraight(cards),
            PokerHandType.RoyalFlush => HasCommonPattern(cards) && CanAssignRanks(cards, RoyalRanks),
            _ => false
        };
    }

    private static bool IsHighCard(IReadOnlyList<PokerCardProfile> cards)
    {
        if (HasAnyPair(cards)) return false;
        if (IsStraight(cards)) return false;
        if (HasCommonPattern(cards)) return false;

        return true;
    }

    private static bool HasAnyPair(IReadOnlyList<PokerCardProfile> cards)
    {
        for (int first = 0; first < cards.Count - 1; first++)
        {
            for (int second = first + 1; second < cards.Count; second++)
            {
                if ((cards[first].RankMask & cards[second].RankMask) != 0) return true;
            }
        }

        return false;
    }

    private static bool HasCommonRank(IReadOnlyList<PokerCardProfile> cards)
    {
        ushort commonRanks = PokerCardProfile.AllRankMask;

        foreach (PokerCardProfile card in cards) commonRanks &= card.RankMask;

        return commonRanks != 0;
    }

    private static bool HasCommonPattern(IReadOnlyList<PokerCardProfile> cards)
    {
        byte commonPatterns = PokerCardProfile.AllPatternMask;

        foreach (PokerCardProfile card in cards) commonPatterns &= card.PatternMask;

        return commonPatterns != 0;
    }

    private static bool IsTwoPair(IReadOnlyList<PokerCardProfile> cards)
    {
        return CanFormDistinctGroups(cards[0].RankMask & cards[1].RankMask, cards[2].RankMask & cards[3].RankMask)
            || CanFormDistinctGroups(cards[0].RankMask & cards[2].RankMask, cards[1].RankMask & cards[3].RankMask)
            || CanFormDistinctGroups(cards[0].RankMask & cards[3].RankMask, cards[1].RankMask & cards[2].RankMask);
    }

    private static bool IsFullHouse(IReadOnlyList<PokerCardProfile> cards)
    {
        for (int first = 0; first < cards.Count - 2; first++)
        {
            for (int second = first + 1; second < cards.Count - 1; second++)
            {
                for (int third = second + 1; third < cards.Count; third++)
                {
                    ushort tripleRanks = (ushort)(
                        cards[first].RankMask
                        & cards[second].RankMask
                        & cards[third].RankMask
                    );
                    ushort pairRanks = PokerCardProfile.AllRankMask;

                    for (int i = 0; i < cards.Count; i++)
                    {
                        if (i == first || i == second || i == third) continue;

                        pairRanks &= cards[i].RankMask;
                    }

                    if (CanFormDistinctGroups(tripleRanks, pairRanks)) return true;
                }
            }
        }

        return false;
    }

    private static bool CanFormDistinctGroups(int firstRanks, int secondRanks)
    {
        for (int rank = PokerCardProfile.MinimumRank; rank <= PokerCardProfile.MaximumRank; rank++)
        {
            ushort rankMask = PokerCardProfile.GetRankMask(rank);

            if ((firstRanks & rankMask) == 0) continue;
            if ((secondRanks & ~rankMask) != 0) return true;
        }

        return false;
    }

    private static bool IsStraight(IReadOnlyList<PokerCardProfile> cards)
    {
        foreach (int[] ranks in StraightRanks)
        {
            if (CanAssignRanks(cards, ranks)) return true;
        }

        return false;
    }

    private static bool CanAssignRanks(IReadOnlyList<PokerCardProfile> cards, IReadOnlyList<int> ranks)
    {
        return CanAssignRanks(cards, ranks, 0, 0);
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