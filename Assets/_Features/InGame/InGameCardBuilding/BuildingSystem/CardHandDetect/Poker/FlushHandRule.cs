using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = nameof(FlushHandRule),
    menuName = "Cards/Hands/Flush Hand Rule"
)]
public sealed class FlushHandRule : CardHandRule
{
    private const string FlushId = "poker.flush";
    private const string FlushPlusId = "poker.flush-plus";
    private const string FlushPlusPlusId = "poker.flush-plus-plus";
    private const string FlushPlusPlusPlusId = "poker.flush-plus-plus-plus";
    private const string YachtId = "poker.yacht";
    private const int FlushPriority = 400;
    private const int FlushPlusPriority = 401;
    private const int FlushPlusPlusPriority = 402;
    private const int FlushPlusPlusPlusPriority = 403;
    private const int YachtPriority = 404;

    private static readonly int[] FlushRankCounts = { 1, 1, 1, 1, 1 };
    private static readonly int[] FlushPlusRankCounts = { 2, 1, 1, 1 };
    private static readonly int[] FlushPlusPlusFirstRankCounts = { 3, 1, 1 };
    private static readonly int[] FlushPlusPlusSecondRankCounts = { 2, 2, 1 };
    private static readonly int[] FlushPlusPlusPlusFirstRankCounts = { 4, 1 };
    private static readonly int[] FlushPlusPlusPlusSecondRankCounts = { 3, 2 };
    private static readonly int[] YachtRankCounts = { 5 };
    private static readonly Color HandColor = new(0.1f, 1f, 0.85f);

    [TitleGroup("족보별 보너스")]
    [SerializeField]
    [LabelText("플러시")]
    private float _flushBonus;

    [TitleGroup("족보별 보너스")]
    [SerializeField]
    [LabelText("플러시+")]
    private float _flushPlusBonus;

    [TitleGroup("족보별 보너스")]
    [SerializeField]
    [LabelText("플러시++")]
    private float _flushPlusPlusBonus;

    [TitleGroup("족보별 보너스")]
    [SerializeField]
    [LabelText("플러시+++")]
    private float _flushPlusPlusPlusBonus;

    [TitleGroup("족보별 보너스")]
    [SerializeField]
    [LabelText("야추")]
    private float _yachtBonus;

    public override string Id => FlushId;
    public float FlushBonus => _flushBonus;
    public float FlushPlusBonus => _flushPlusBonus;
    public float FlushPlusPlusBonus => _flushPlusPlusBonus;
    public float FlushPlusPlusPlusBonus => _flushPlusPlusPlusBonus;
    public float YachtBonus => _yachtBonus;

    public override void FindMatches(CardLine line, ICollection<CardHandMatch> matches) =>
        PokerHandEvaluator.FindMatches(line, 5, matches, Evaluate);

    private void Evaluate(
        CardLine line,
        IReadOnlyList<CardLineCard> cards,
        IReadOnlyList<PokerCardProfile> profiles,
        ICollection<CardHandMatch> matches
    )
    {
        if (!PokerHandEvaluator.HasCommonPattern(profiles)) return;

        if (PokerHandEvaluator.CanMatchRankCounts(profiles, YachtRankCounts))
        {
            AddMatch(
                YachtId,
                "야추",
                YachtPriority,
                HandColor,
                _yachtBonus,
                line,
                cards,
                matches
            );
            return;
        }

        bool isPlusPlusPlus =
            PokerHandEvaluator.CanMatchRankCounts(profiles, FlushPlusPlusPlusFirstRankCounts)
            || PokerHandEvaluator.CanMatchRankCounts(profiles, FlushPlusPlusPlusSecondRankCounts);

        if (isPlusPlusPlus)
        {
            AddMatch(
                FlushPlusPlusPlusId,
                "플러시+++",
                FlushPlusPlusPlusPriority,
                HandColor,
                _flushPlusPlusPlusBonus,
                line,
                cards,
                matches
            );
            return;
        }

        bool isPlusPlus =
            PokerHandEvaluator.CanMatchRankCounts(profiles, FlushPlusPlusFirstRankCounts)
            || PokerHandEvaluator.CanMatchRankCounts(profiles, FlushPlusPlusSecondRankCounts);

        if (isPlusPlus)
        {
            AddMatch(
                FlushPlusPlusId,
                "플러시++",
                FlushPlusPlusPriority,
                HandColor,
                _flushPlusPlusBonus,
                line,
                cards,
                matches
            );
            return;
        }

        if (PokerHandEvaluator.CanMatchRankCounts(profiles, FlushPlusRankCounts))
        {
            AddMatch(
                FlushPlusId,
                "플러시+",
                FlushPlusPriority,
                HandColor,
                _flushPlusBonus,
                line,
                cards,
                matches
            );
            return;
        }

        if (!PokerHandEvaluator.CanMatchRankCounts(profiles, FlushRankCounts)) return;

        AddMatch(
            FlushId,
            "플러시",
            FlushPriority,
            HandColor,
            _flushBonus,
            line,
            cards,
            matches
        );
    }
}
