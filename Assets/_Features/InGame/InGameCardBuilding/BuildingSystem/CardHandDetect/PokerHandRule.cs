using System.Collections.Generic;
using UnityEngine;

public enum PokerHandType
{
    OnePair,
    ThreeOfAKind,
    Straight,
    Flush,
    FullHouse,
    FourOfAKind,
    StraightFlush,
    RoyalFlush
}

[CreateAssetMenu(
    fileName = nameof(PokerHandRule),
    menuName = "Cards/Hands/Poker Hand Rule"
)]
public sealed class PokerHandRule : CardHandRule
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

    [SerializeField]
    private PokerHandType _handType;

    public PokerHandType HandType => _handType;
    public override string Id => GetId(_handType);
    public override string DisplayName => GetDisplayName(_handType);
    public override int Priority => GetPriority(_handType);
    public override Color LineColor => GetLineColor(_handType);

    public void Configure(PokerHandType handType)
    {
        _handType = handType;
    }

    public override void FindMatches(CardLine line, ICollection<CardHandMatch> matches)
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

            FindMatchesInSegment(line, segment, matches);
            segment.Clear();
        }

        FindMatchesInSegment(line, segment, matches);
    }

    private void FindMatchesInSegment(
        CardLine line,
        IReadOnlyList<PokerLineCard> segment,
        ICollection<CardHandMatch> matches
    )
    {
        int requiredCardCount = GetRequiredCardCount(_handType);

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

            AddSelectionIfMatched(line, selectedCards, selectedProfiles, matches);
        }

        return;
    }

    private void FindCombinationMatches(
        CardLine line,
        IReadOnlyList<PokerLineCard> segment,
        int sourceIndex,
        int selectedIndex,
        CardLineCard[] selectedCards,
        PokerCardProfile[] selectedProfiles,
        ICollection<CardHandMatch> matches
    )
    {
        if (selectedIndex == selectedCards.Length)
        {
            AddSelectionIfMatched(line, selectedCards, selectedProfiles, matches);
            return;
        }

        int remainingSelectionCount = selectedCards.Length - selectedIndex;
        int lastSourceIndex = segment.Count - remainingSelectionCount;

        for (int i = sourceIndex; i <= lastSourceIndex; i++)
        {
            selectedCards[selectedIndex] = segment[i].LineCard;
            selectedProfiles[selectedIndex] = segment[i].Profile;

            FindCombinationMatches(
                line,
                segment,
                i + 1,
                selectedIndex + 1,
                selectedCards,
                selectedProfiles,
                matches
            );
        }
    }

    private void AddSelectionIfMatched(
        CardLine line,
        IReadOnlyList<CardLineCard> selectedCards,
        IReadOnlyList<PokerCardProfile> selectedProfiles,
        ICollection<CardHandMatch> matches
    )
    {
        if (!PokerHandEvaluator.IsMatch(_handType, selectedProfiles)) return;

        AddMatch(line, selectedCards, matches);
    }

    private static int GetRequiredCardCount(PokerHandType handType)
    {
        return handType switch
        {
            PokerHandType.OnePair => 2,
            PokerHandType.ThreeOfAKind => 3,
            PokerHandType.Straight => 5,
            PokerHandType.Flush => 5,
            PokerHandType.FullHouse => 5,
            PokerHandType.FourOfAKind => 4,
            PokerHandType.StraightFlush => 5,
            PokerHandType.RoyalFlush => 5,
            _ => 0
        };
    }

    private static string GetId(PokerHandType handType)
    {
        return handType switch
        {
            PokerHandType.OnePair => "poker.one-pair",
            PokerHandType.ThreeOfAKind => "poker.three-of-a-kind",
            PokerHandType.Straight => "poker.straight",
            PokerHandType.Flush => "poker.flush",
            PokerHandType.FullHouse => "poker.full-house",
            PokerHandType.FourOfAKind => "poker.four-of-a-kind",
            PokerHandType.StraightFlush => "poker.straight-flush",
            PokerHandType.RoyalFlush => "poker.royal-flush",
            _ => string.Empty
        };
    }

    private static string GetDisplayName(PokerHandType handType)
    {
        return handType switch
        {
            PokerHandType.OnePair => "원 페어",
            PokerHandType.ThreeOfAKind => "트리플",
            PokerHandType.Straight => "스트레이트",
            PokerHandType.Flush => "플러시",
            PokerHandType.FullHouse => "풀 하우스",
            PokerHandType.FourOfAKind => "포 카드",
            PokerHandType.StraightFlush => "스트레이트 플러시",
            PokerHandType.RoyalFlush => "로열 플러시",
            _ => string.Empty
        };
    }

    private static int GetPriority(PokerHandType handType)
    {
        return handType switch
        {
            PokerHandType.OnePair => 100,
            PokerHandType.ThreeOfAKind => 200,
            PokerHandType.Straight => 300,
            PokerHandType.Flush => 400,
            PokerHandType.FullHouse => 500,
            PokerHandType.FourOfAKind => 600,
            PokerHandType.StraightFlush => 700,
            PokerHandType.RoyalFlush => 800,
            _ => 0
        };
    }

    private static Color GetLineColor(PokerHandType handType)
    {
        return handType switch
        {
            PokerHandType.OnePair => new Color(0.25f, 0.8f, 1f),
            PokerHandType.ThreeOfAKind => new Color(0.55f, 0.3f, 1f),
            PokerHandType.Straight => new Color(0.25f, 1f, 0.45f),
            PokerHandType.Flush => new Color(0.1f, 1f, 0.85f),
            PokerHandType.FullHouse => new Color(1f, 0.55f, 0.15f),
            PokerHandType.FourOfAKind => new Color(1f, 0.2f, 0.2f),
            PokerHandType.StraightFlush => new Color(1f, 0.2f, 0.8f),
            PokerHandType.RoyalFlush => new Color(1f, 0.85f, 0.15f),
            _ => Color.white
        };
    }
}
