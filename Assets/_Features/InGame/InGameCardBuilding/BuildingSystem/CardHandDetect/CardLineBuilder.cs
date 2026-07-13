using System.Collections.Generic;
using UnityEngine;

public static class CardLineBuilder
{
    public static List<CardLine> Build(IReadOnlyDictionary<Vector2Int, Card> placedCards)
    {
        List<Vector2Int> positions = new(placedCards.Keys);
        positions.Sort(ComparePositions);

        List<CardLine> lines = new();

        foreach (Vector2Int position in positions)
        {
            if (!placedCards.ContainsKey(position + Vector2Int.left)) AddLine(placedCards, position, Vector2Int.right, CardLineDirection.Horizontal, lines);

            if (!placedCards.ContainsKey(position + Vector2Int.down)) AddLine(placedCards, position, Vector2Int.up, CardLineDirection.Vertical, lines);
        }

        return lines;
    }

    private static void AddLine(
        IReadOnlyDictionary<Vector2Int, Card> placedCards,
        Vector2Int start,
        Vector2Int step,
        CardLineDirection direction,
        ICollection<CardLine> lines
    )
    {
        List<CardLineCard> cards = new();
        Vector2Int position = start;

        while (placedCards.TryGetValue(position, out Card card))
        {
            cards.Add(new CardLineCard(position, card));
            position += step;
        }

        if (cards.Count < 2) return;

        lines.Add(new CardLine(direction, cards));
    }

    private static int ComparePositions(Vector2Int left, Vector2Int right)
    {
        int yComparison = left.y.CompareTo(right.y);
        return yComparison != 0 ? yComparison : left.x.CompareTo(right.x);
    }
}
