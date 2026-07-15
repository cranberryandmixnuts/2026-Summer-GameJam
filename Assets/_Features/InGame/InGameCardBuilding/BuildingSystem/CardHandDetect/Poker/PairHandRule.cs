using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = nameof(PairHandRule),
    menuName = "Cards/Hands/Pair Hand Rule"
)]
public sealed class PairHandRule : CardHandRule
{
    public const string PairId = "poker.one-pair";
    public const string PairPlusId = "poker.one-pair-plus";
    private const int PairPriority = 100;
    private const int PairPlusPriority = 101;

    private static readonly int[] PairPatternCounts = { 1, 1 };
    private static readonly int[] PairPlusPatternCounts = { 2 };
    private static readonly Color HandColor = new(0.25f, 0.8f, 1f);

    [TitleGroup("족보별 보너스")]
    [SerializeField]
    [LabelText("페어")]
    private float _pairBonus;

    [TitleGroup("족보별 보너스")]
    [SerializeField]
    [LabelText("페어+")]
    private float _pairPlusBonus;

    public override string Id => PairId;
    public float PairBonus => _pairBonus;
    public float PairPlusBonus => _pairPlusBonus;

    public override void FindMatches(CardLine line, ICollection<CardHandMatch> matches) =>
        PokerHandEvaluator.FindMatches(line, 2, matches, Evaluate);

    private void Evaluate(
        CardLine line,
        IReadOnlyList<CardLineCard> cards,
        IReadOnlyList<PokerCardProfile> profiles,
        ICollection<CardHandMatch> matches
    )
    {
        if (!PokerHandEvaluator.HasCommonRank(profiles)) return;

        if (PokerHandEvaluator.CanMatchPatternCounts(profiles, PairPlusPatternCounts))
        {
            AddMatch(
                PairPlusId,
                "페어+",
                PairPlusPriority,
                HandColor,
                _pairPlusBonus,
                line,
                cards,
                matches
            );
            return;
        }

        if (!PokerHandEvaluator.CanMatchPatternCounts(profiles, PairPatternCounts)) return;

        AddMatch(
            PairId,
            "페어",
            PairPriority,
            HandColor,
            _pairBonus,
            line,
            cards,
            matches
        );
    }
}
