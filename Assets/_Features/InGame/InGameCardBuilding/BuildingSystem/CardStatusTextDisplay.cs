using DG.Tweening;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public sealed class CardStatusTextDisplay : SingletonBehaviour<CardStatusTextDisplay, SceneScope>
{
    private const string LevelColor = "FFCE7A";

    private sealed class MatchSummary
    {
        public string DisplayName { get; }
        public Color DisplayColor { get; }
        public int Count { get; set; }

        public MatchSummary(string displayName, Color displayColor)
        {
            DisplayName = displayName;
            DisplayColor = displayColor;
            Count = 1;
        }
    }

    private readonly Dictionary<string, int> matchIndices = new();
    private readonly List<MatchSummary> matchSummaries = new();
    private readonly StringBuilder textBuilder = new();

    private TMP_Text text;
    private CardField cardField;
    private CardHandDetector handDetector;
    private PlayerHand playerHand;
    private float baseDamage;
    private float multiplier = 1f;
    private int remainingHandCapacity;
    private int handCardLimit;
    private Tween baseDamageTween;
    private Tween multiplierTween;

    protected override void SingletonAwake() => text = GetComponent<TMP_Text>();

    private void Start()
    {
        cardField = CardField.Instance;
        handDetector = CardHandDetector.Instance;
        playerHand = PlayerHand.Instance;

        cardField.StatusChanged += HandleStatusChanged;
        handDetector.MatchesChanged += HandleMatchesChanged;
        playerHand.CapacityChanged += HandleCapacityChanged;

        baseDamage = cardField.FinalBaseDamage;
        multiplier = cardField.FinalMultiplier;
        HandleMatchesChanged(handDetector.CurrentMatches);
        HandleCapacityChanged(playerHand.RemainingCapacity, playerHand.HandCardLimit);
    }

    protected override void SingletonOnDestroy()
    {
        if (cardField != null) cardField.StatusChanged -= HandleStatusChanged;
        if (handDetector != null) handDetector.MatchesChanged -= HandleMatchesChanged;
        if (playerHand != null) playerHand.CapacityChanged -= HandleCapacityChanged;

        baseDamageTween?.Kill();
        multiplierTween?.Kill();
    }

    private void HandleStatusChanged(float targetBaseDamage, float targetMultiplier)
    {
        baseDamageTween?.Kill();
        multiplierTween?.Kill();

        baseDamageTween = DOTween.To(
            () => baseDamage,
            value => baseDamage = value,
            targetBaseDamage,
            0.5f
        )
        .SetEase(Ease.OutQuad)
        .OnUpdate(RefreshText);

        multiplierTween = DOTween.To(
            () => multiplier,
            value => multiplier = value,
            targetMultiplier,
            0.5f
        )
        .SetEase(Ease.OutQuad)
        .OnUpdate(RefreshText);
    }

    private void HandleMatchesChanged(IReadOnlyList<CardHandMatch> matches)
    {
        matchIndices.Clear();
        matchSummaries.Clear();

        foreach (CardHandMatch match in matches)
        {
            if (matchIndices.TryGetValue(match.Id, out int summaryIndex))
            {
                matchSummaries[summaryIndex].Count++;
                continue;
            }

            matchIndices.Add(match.Id, matchSummaries.Count);
            matchSummaries.Add(new MatchSummary(match.DisplayName, match.DisplayColor));
        }

        RefreshText();
    }

    private void HandleCapacityChanged(int remainingCapacity, int maximumCapacity)
    {
        remainingHandCapacity = remainingCapacity;
        handCardLimit = maximumCapacity;
        RefreshText();
    }

    private void RefreshText()
    {
        textBuilder.Clear();
        textBuilder
            .Append("데미지: ")
            .Append(baseDamage.ToString("0.00"))
            .AppendLine()
            .Append("배수: ")
            .Append(multiplier.ToString("0.00"))
            .AppendLine()
            .Append("남은 손패: ")
            .Append(remainingHandCapacity)
            .Append('/')
            .Append(handCardLimit);

        if (matchSummaries.Count > 0)
        {
            textBuilder.Append("\n\n족보 보너스");

            foreach (MatchSummary summary in matchSummaries)
            {
                textBuilder.Append("\n  - ");
                AppendDisplayName(summary);

                if (summary.Count > 1)
                    textBuilder.Append(" (").Append(summary.Count).Append(')');
            }
        }

        text.text = textBuilder.ToString();
    }

    private void AppendDisplayName(MatchSummary summary)
    {
        int baseNameLength = summary.DisplayName.Length;

        while (baseNameLength > 0 && summary.DisplayName[baseNameLength - 1] == '+')
            baseNameLength--;

        textBuilder
            .Append("<color=#")
            .Append(ColorUtility.ToHtmlStringRGB(summary.DisplayColor))
            .Append('>')
            .Append(summary.DisplayName, 0, baseNameLength)
            .Append("</color>");

        if (baseNameLength == summary.DisplayName.Length) return;

        textBuilder
            .Append("<color=#")
            .Append(LevelColor)
            .Append('>')
            .Append(
                summary.DisplayName,
                baseNameLength,
                summary.DisplayName.Length - baseNameLength
            )
            .Append("</color>");
    }
}
