using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = nameof(TripleHandRule),
    menuName = "Cards/Hands/Triple Hand Rule"
)]
public sealed class TripleHandRule : CardHandRule
{
    public const string TripleId = "poker.three-of-a-kind";
    public const string TriplePlusId = "poker.three-of-a-kind-plus";
    public const string TriplePlusPlusId = "poker.three-of-a-kind-plus-plus";
    private const int TriplePriority = 200;
    private const int TriplePlusPriority = 201;
    private const int TriplePlusPlusPriority = 202;

    private static readonly int[] TriplePatternCounts = { 1, 1, 1 };
    private static readonly int[] TriplePlusPatternCounts = { 2, 1 };
    private static readonly int[] TriplePlusPlusPatternCounts = { 3 };
    private static readonly Color HandColor = new(0.55f, 0.3f, 1f);

    [TitleGroup("족보별 보너스")]
    [SerializeField]
    [LabelText("트리플")]
    private float _tripleBonus;

    [TitleGroup("족보별 보너스")]
    [SerializeField]
    [LabelText("트리플+")]
    private float _triplePlusBonus;

    [TitleGroup("족보별 보너스")]
    [SerializeField]
    [LabelText("트리플++")]
    private float _triplePlusPlusBonus;

    public override string Id => TripleId;
    public float TripleBonus => _tripleBonus;
    public float TriplePlusBonus => _triplePlusBonus;
    public float TriplePlusPlusBonus => _triplePlusPlusBonus;

    public override void FindMatches(CardLine line, ICollection<CardHandMatch> matches) =>
        PokerHandEvaluator.FindMatches(line, 3, matches, Evaluate);

    private void Evaluate(
        CardLine line,
        IReadOnlyList<CardLineCard> cards,
        IReadOnlyList<PokerCardProfile> profiles,
        ICollection<CardHandMatch> matches
    )
    {
        if (!PokerHandEvaluator.HasCommonRank(profiles)) return;

        if (PokerHandEvaluator.CanMatchPatternCounts(profiles, TriplePlusPlusPatternCounts))
        {
            AddMatch(
                TriplePlusPlusId,
                "트리플++",
                TriplePlusPlusPriority,
                HandColor,
                _triplePlusPlusBonus,
                line,
                cards,
                matches
            );
            return;
        }

        if (PokerHandEvaluator.CanMatchPatternCounts(profiles, TriplePlusPatternCounts))
        {
            AddMatch(
                TriplePlusId,
                "트리플+",
                TriplePlusPriority,
                HandColor,
                _triplePlusBonus,
                line,
                cards,
                matches
            );
            return;
        }

        if (!PokerHandEvaluator.CanMatchPatternCounts(profiles, TriplePatternCounts)) return;

        AddMatch(
            TripleId,
            "트리플",
            TriplePriority,
            HandColor,
            _tripleBonus,
            line,
            cards,
            matches
        );
    }
}
