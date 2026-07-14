using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public sealed class CurrentHand : MonoBehaviour
{
    private bool IsPlaying => Application.isPlaying;

    [Button("현재 족보 출력", ButtonSizes.Large)]
    [EnableIf(nameof(IsPlaying))]
    public void PrintCurrentHands()
    {
        IReadOnlyList<CardHandMatch> matches = CardHandDetector.Instance.CurrentMatches;

        if (matches.Count == 0)
        {
            Debug.Log("현재 인정된 족보가 없습니다.", this);
            return;
        }

        StringBuilder builder = new();
        builder.Append("현재 인정된 족보: ")
            .Append(matches.Count)
            .AppendLine("개");

        for (int matchIndex = 0; matchIndex < matches.Count; matchIndex++)
        {
            CardHandMatch match = matches[matchIndex];

            builder.Append(matchIndex + 1)
                .Append(". ")
                .Append(match.DisplayName)
                .Append(" | 보너스: ")
                .Append(match.Bonus)
                .Append(" | 방향: ")
                .Append(GetDirectionName(match.Line.Direction))
                .Append(" | 카드: ");

            for (int cardIndex = 0; cardIndex < match.Cards.Count; cardIndex++)
            {
                if (cardIndex > 0) builder.Append(" -> ");

                builder.Append(match.Cards[cardIndex].Position);
            }

            if (matchIndex < matches.Count - 1) builder.AppendLine();
        }

        Debug.Log(builder.ToString(), this);
    }

    private static string GetDirectionName(CardLineDirection direction) =>
        direction == CardLineDirection.Horizontal ? "가로" : "세로";
}