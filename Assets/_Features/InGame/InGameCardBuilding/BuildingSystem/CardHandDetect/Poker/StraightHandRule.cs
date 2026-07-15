using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = nameof(StraightHandRule),
    menuName = "Cards/Hands/Straight Hand Rule"
)]
public sealed class StraightHandRule : CardHandRule
{
    public const string StraightId = "poker.straight";
    public const string StraightFlushId = "poker.straight-flush";
    public const string RoyalStraightId = "poker.royal-straight";
    public const string RoyalStraightFlushId = "poker.royal-straight-flush";
    private const int StraightPriority = 300;
    private const int RoyalStraightPriority = 301;
    private const int StraightFlushPriority = 700;
    private const int RoyalStraightFlushPriority = 800;

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
    private static readonly Color StraightColor = new(0.25f, 1f, 0.45f);
    private static readonly Color StraightFlushColor = new(1f, 0.2f, 0.8f);
    private static readonly Color RoyalColor = new(1f, 0.85f, 0.15f);

    [TitleGroup("족보별 보너스")]
    [SerializeField]
    [LabelText("스트레이트")]
    private float _straightBonus;

    [TitleGroup("족보별 보너스")]
    [SerializeField]
    [LabelText("스트레이트 플러시")]
    private float _straightFlushBonus;

    [TitleGroup("족보별 보너스")]
    [SerializeField]
    [LabelText("로열 스트레이트")]
    private float _royalStraightBonus;

    [TitleGroup("족보별 보너스")]
    [SerializeField]
    [LabelText("로열 스트레이트 플러시")]
    private float _royalStraightFlushBonus;

    public override string Id => StraightId;
    public float StraightBonus => _straightBonus;
    public float StraightFlushBonus => _straightFlushBonus;
    public float RoyalStraightBonus => _royalStraightBonus;
    public float RoyalStraightFlushBonus => _royalStraightFlushBonus;

    public override void FindMatches(CardLine line, ICollection<CardHandMatch> matches) =>
        PokerHandEvaluator.FindMatches(line, 5, matches, Evaluate);

    private void Evaluate(
        CardLine line,
        IReadOnlyList<CardLineCard> cards,
        IReadOnlyList<PokerCardProfile> profiles,
        ICollection<CardHandMatch> matches
    )
    {
        bool isRoyal = PokerHandEvaluator.CanAssignRanks(profiles, RoyalRanks);
        bool hasCommonPattern = PokerHandEvaluator.HasCommonPattern(profiles);

        if (isRoyal && hasCommonPattern)
        {
            AddMatch(
                RoyalStraightFlushId,
                "로열 스트레이트 플러시",
                RoyalStraightFlushPriority,
                RoyalColor,
                _royalStraightFlushBonus,
                line,
                cards,
                matches
            );
            return;
        }

        bool isStraight = IsStraight(profiles);

        if (isStraight && hasCommonPattern)
        {
            AddMatch(
                StraightFlushId,
                "스트레이트 플러시",
                StraightFlushPriority,
                StraightFlushColor,
                _straightFlushBonus,
                line,
                cards,
                matches
            );
            return;
        }

        if (isRoyal)
        {
            AddMatch(
                RoyalStraightId,
                "로열 스트레이트",
                RoyalStraightPriority,
                RoyalColor,
                _royalStraightBonus,
                line,
                cards,
                matches
            );
            return;
        }

        if (!isStraight) return;

        AddMatch(
            StraightId,
            "스트레이트",
            StraightPriority,
            StraightColor,
            _straightBonus,
            line,
            cards,
            matches
        );
    }

    private static bool IsStraight(IReadOnlyList<PokerCardProfile> profiles)
    {
        foreach (int[] ranks in StraightRanks)
        {
            if (PokerHandEvaluator.CanAssignRanks(profiles, ranks)) return true;
        }

        return false;
    }
}
