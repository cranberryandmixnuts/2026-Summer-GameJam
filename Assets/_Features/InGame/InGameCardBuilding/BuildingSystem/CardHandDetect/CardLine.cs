using System.Collections.Generic;
using UnityEngine;

public enum CardLineDirection
{
    Horizontal,
    Vertical
}

public readonly struct CardLineCard
{
    public Vector2Int Position { get; }
    public Card Card { get; }

    public CardLineCard(Vector2Int position, Card card)
    {
        Position = position;
        Card = card;
    }
}

public sealed class CardLine
{
    private readonly CardLineCard[] _cards;

    public CardLineDirection Direction { get; }
    public IReadOnlyList<CardLineCard> Cards => _cards;

    public CardLine(CardLineDirection direction, IReadOnlyList<CardLineCard> cards)
    {
        Direction = direction;
        _cards = new CardLineCard[cards.Count];

        for (int i = 0; i < cards.Count; i++) _cards[i] = cards[i];
    }
}
