using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = nameof(FourCardHandRule),
    menuName = "Cards/Hands/Four Card Hand Rule"
)]
public sealed class FourCardHandRule : CardHandRule
{
    private const string FourCardId = "poker.four-of-a-kind";
    private const string FourCardPlusId = "poker.four-of-a-kind-plus";
    private const string FourCardPlusPlusId = "poker.four-of-a-kind-plus-plus";
    private const string FourCardPlusPlusPlusId = "poker.four-of-a-kind-plus-plus-plus";
    private const int FourCardPriority = 600;
    private const int FourCardPlusPriority = 601;
    private const int FourCardPlusPlusPriority = 602;
    private const int FourCardPlusPlusPlusPriority = 603;

    private static readonly int[] FourCardPatternCounts = { 1, 1, 1, 1 };
    private static readonly int[] FourCardPlusPatternCounts = { 2, 1, 1 };
    private static readonly int[] FourCardPlusPlusFirstPatternCounts = { 3, 1 };
    private static readonly int[] FourCardPlusPlusSecondPatternCounts = { 2, 2 };
    private static readonly int[] FourCardPlusPlusPlusPatternCounts = { 4 };
    private static readonly Color HandColor = new(1f, 0.2f, 0.2f);

    [TitleGroup("족보별 보너스")]
    [SerializeField]
    [LabelText("포카드")]
    private float _fourCardBonus;

    [TitleGroup("족보별 보너스")]
    [SerializeField]
    [LabelText("포카드+")]
    private float _fourCardPlusBonus;

    [TitleGroup("족보별 보너스")]
    [SerializeField]
    [LabelText("포카드++")]
    private float _fourCardPlusPlusBonus;

    [TitleGroup("족보별 보너스")]
    [SerializeField]
    [LabelText("포카드+++")]
    private float _fourCardPlusPlusPlusBonus;

    public override string Id => FourCardId;
    public float FourCardBonus => _fourCardBonus;
    public float FourCardPlusBonus => _fourCardPlusBonus;
    public float FourCardPlusPlusBonus => _fourCardPlusPlusBonus;
    public float FourCardPlusPlusPlusBonus => _fourCardPlusPlusPlusBonus;

    public override void FindMatches(CardLine line, ICollection<CardHandMatch> matches) =>
        PokerHandEvaluator.FindMatches(line, 4, matches, Evaluate);

    private void Evaluate(
        CardLine line,
        IReadOnlyList<CardLineCard> cards,
        IReadOnlyList<PokerCardProfile> profiles,
        ICollection<CardHandMatch> matches
    )
    {
        if (!PokerHandEvaluator.HasCommonRank(profiles)) return;

        if (PokerHandEvaluator.CanMatchPatternCounts(profiles, FourCardPlusPlusPlusPatternCounts))
        {
            AddMatch(
                FourCardPlusPlusPlusId,
                "포카드+++",
                FourCardPlusPlusPlusPriority,
                HandColor,
                _fourCardPlusPlusPlusBonus,
                line,
                cards,
                matches
            );
            return;
        }

        bool isPlusPlus =
            PokerHandEvaluator.CanMatchPatternCounts(profiles, FourCardPlusPlusFirstPatternCounts)
            || PokerHandEvaluator.CanMatchPatternCounts(profiles, FourCardPlusPlusSecondPatternCounts);

        if (isPlusPlus)
        {
            AddMatch(
                FourCardPlusPlusId,
                "포카드++",
                FourCardPlusPlusPriority,
                HandColor,
                _fourCardPlusPlusBonus,
                line,
                cards,
                matches
            );
            return;
        }

        if (PokerHandEvaluator.CanMatchPatternCounts(profiles, FourCardPlusPatternCounts))
        {
            AddMatch(
                FourCardPlusId,
                "포카드+",
                FourCardPlusPriority,
                HandColor,
                _fourCardPlusBonus,
                line,
                cards,
                matches
            );
            return;
        }

        if (!PokerHandEvaluator.CanMatchPatternCounts(profiles, FourCardPatternCounts)) return;

        AddMatch(
            FourCardId,
            "포카드",
            FourCardPriority,
            HandColor,
            _fourCardBonus,
            line,
            cards,
            matches
        );
    }
}
