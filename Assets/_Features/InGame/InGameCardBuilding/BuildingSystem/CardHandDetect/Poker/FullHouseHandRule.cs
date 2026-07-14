using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = nameof(FullHouseHandRule),
    menuName = "Cards/Hands/Full House Hand Rule"
)]
public sealed class FullHouseHandRule : CardHandRule
{
    private const string FullHouseId = "poker.full-house";
    private const string FullHousePlusId = "poker.full-house-plus";
    private const string FullHousePlusPlusId = "poker.full-house-plus-plus";
    private const string PerfectFullHouseId = "poker.perfect-full-house";
    private const string PerfectFullHousePlusId = "poker.perfect-full-house-plus";
    private const int FullHousePriority = 500;
    private const int FullHousePlusPriority = 501;
    private const int FullHousePlusPlusPriority = 502;
    private const int PerfectFullHousePriority = 503;
    private const int PerfectFullHousePlusPriority = 504;

    private static readonly int[] PairPatternCounts = { 1, 1 };
    private static readonly int[] PairPlusPatternCounts = { 2 };
    private static readonly int[] TriplePatternCounts = { 1, 1, 1 };
    private static readonly int[] TriplePlusPatternCounts = { 2, 1 };
    private static readonly int[] TriplePlusPlusPatternCounts = { 3 };
    private static readonly Color HandColor = new(1f, 0.55f, 0.15f);

    [TitleGroup("족보별 보너스")]
    [SerializeField]
    [LabelText("풀하우스")]
    private float _fullHouseBonus;

    [TitleGroup("족보별 보너스")]
    [SerializeField]
    [LabelText("풀하우스+")]
    private float _fullHousePlusBonus;

    [TitleGroup("족보별 보너스")]
    [SerializeField]
    [LabelText("풀하우스++")]
    private float _fullHousePlusPlusBonus;

    [TitleGroup("족보별 보너스")]
    [SerializeField]
    [LabelText("퍼펙트 풀하우스")]
    private float _perfectFullHouseBonus;

    [TitleGroup("족보별 보너스")]
    [SerializeField]
    [LabelText("퍼펙트 풀하우스+")]
    private float _perfectFullHousePlusBonus;

    public override string Id => FullHouseId;
    public float FullHouseBonus => _fullHouseBonus;
    public float FullHousePlusBonus => _fullHousePlusBonus;
    public float FullHousePlusPlusBonus => _fullHousePlusPlusBonus;
    public float PerfectFullHouseBonus => _perfectFullHouseBonus;
    public float PerfectFullHousePlusBonus => _perfectFullHousePlusBonus;

    public override void FindMatches(CardLine line, ICollection<CardHandMatch> matches) =>
        PokerHandEvaluator.FindMatches(line, 5, matches, Evaluate);

    private void Evaluate(
        CardLine line,
        IReadOnlyList<CardLineCard> cards,
        IReadOnlyList<PokerCardProfile> profiles,
        ICollection<CardHandMatch> matches
    )
    {
        if (PokerHandEvaluator.CanMatchFullHouse(
            profiles,
            TriplePlusPlusPatternCounts,
            PairPlusPatternCounts
        ))
        {
            AddMatch(
                PerfectFullHousePlusId,
                "퍼펙트 풀하우스+",
                PerfectFullHousePlusPriority,
                HandColor,
                _perfectFullHousePlusBonus,
                line,
                cards,
                matches
            );
            return;
        }

        if (PokerHandEvaluator.CanMatchFullHouse(
            profiles,
            TriplePlusPlusPatternCounts,
            PairPatternCounts
        ))
        {
            AddMatch(
                PerfectFullHouseId,
                "퍼펙트 풀하우스",
                PerfectFullHousePriority,
                HandColor,
                _perfectFullHouseBonus,
                line,
                cards,
                matches
            );
            return;
        }

        if (PokerHandEvaluator.CanMatchFullHouse(
            profiles,
            TriplePlusPatternCounts,
            PairPlusPatternCounts
        ))
        {
            AddMatch(
                FullHousePlusPlusId,
                "풀하우스++",
                FullHousePlusPlusPriority,
                HandColor,
                _fullHousePlusPlusBonus,
                line,
                cards,
                matches
            );
            return;
        }

        bool isPlus =
            PokerHandEvaluator.CanMatchFullHouse(
                profiles,
                TriplePlusPatternCounts,
                PairPatternCounts
            )
            || PokerHandEvaluator.CanMatchFullHouse(
                profiles,
                TriplePatternCounts,
                PairPlusPatternCounts
            );

        if (isPlus)
        {
            AddMatch(
                FullHousePlusId,
                "풀하우스+",
                FullHousePlusPriority,
                HandColor,
                _fullHousePlusBonus,
                line,
                cards,
                matches
            );
            return;
        }

        if (!PokerHandEvaluator.CanMatchFullHouse(
            profiles,
            TriplePatternCounts,
            PairPatternCounts
        )) return;

        AddMatch(
            FullHouseId,
            "풀하우스",
            FullHousePriority,
            HandColor,
            _fullHouseBonus,
            line,
            cards,
            matches
        );
    }
}
