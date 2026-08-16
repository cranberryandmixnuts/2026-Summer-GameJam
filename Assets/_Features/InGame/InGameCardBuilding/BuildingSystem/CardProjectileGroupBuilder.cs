using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public static class CardProjectileGroupBuilder
{
    public static void AttachCards(IReadOnlyList<Card> cards, Transform projectileTransform)
    {
        Bounds bounds = GetWorldBounds(cards);
        Transform layoutTransform = cards[0].transform.parent;

        projectileTransform.SetPositionAndRotation(bounds.center, layoutTransform.rotation);
        projectileTransform.localScale = layoutTransform.lossyScale;

        foreach (Card card in cards)
        {
            card.transform.DOKill();
            card.GetComponent<CardAnimator>().RemoveAngle();
            card.transform.SetParent(projectileTransform, true);
        }
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
