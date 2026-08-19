using System.Text;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public sealed class CardHoverStatusTextDisplay : SingletonBehaviour<CardHoverStatusTextDisplay, SceneScope>
{
    private TMP_Text text;

    private readonly StringBuilder textBuilder = new();

    private PlayerHand playerHand;

    protected override void SingletonAwake() => text = GetComponent<TMP_Text>();

    private void Start()
    {
        playerHand = PlayerHand.Instance;
        playerHand.HoveredCardChanged += HandleHoveredCardChanged;
        HandleHoveredCardChanged(playerHand.HoveredCard);
    }

    protected override void SingletonOnDestroy()
    {
        if (playerHand != null) playerHand.HoveredCardChanged -= HandleHoveredCardChanged;
    }

    private void HandleHoveredCardChanged(Card card)
    {
        if (card == null)
        {
            text.text = string.Empty;
            return;
        }

        textBuilder.Clear();

        if (!string.IsNullOrWhiteSpace(card.Explanation))
        {
            textBuilder
                .Append(card.Explanation)
                .AppendLine()
                .AppendLine();
        }

        textBuilder
            .Append("기본 데미지: ")
            .Append(card.BaseDamage.ToString("0.00"))
            .AppendLine()
            .Append("추가 배수: ")
            .Append(card.AdditionalMultiplier.ToString("0.00"));

        text.text = textBuilder.ToString();
    }
}
