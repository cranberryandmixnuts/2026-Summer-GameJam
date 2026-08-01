using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public static class CardProjectileGroupBuilder
{
    public static GameObject Build(IReadOnlyList<Card> cards)
    {
        Bounds bounds = GetWorldBounds(cards);
        Transform layoutTransform = cards[0].transform.parent;
        GameObject group = new("CardBullet");
        Transform groupTransform = group.transform;
        groupTransform.SetPositionAndRotation(bounds.center, layoutTransform.rotation);
        groupTransform.localScale = layoutTransform.lossyScale;

        foreach (Card card in cards)
        {
            card.transform.DOKill();
            card.GetComponent<CardAnimator>().RemoveAngle();
            card.transform.SetParent(groupTransform, true);
        }

        return group;
    }

    private static Bounds GetWorldBounds(IReadOnlyList<Card> cards)
    {
        Bounds bounds = ((RectTransform)cards[0].transform).GetWorldBounds();

        for (int index = 1; index < cards.Count; index++)
        {
            Bounds cardBounds = ((RectTransform)cards[index].transform).GetWorldBounds();
            bounds.Encapsulate(cardBounds.min);
            bounds.Encapsulate(cardBounds.max);
        }

        return bounds;
    }
}
